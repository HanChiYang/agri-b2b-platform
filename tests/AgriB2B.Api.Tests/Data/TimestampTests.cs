using AgriB2B.Api.Data;
using AgriB2B.Api.Data.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace AgriB2B.Api.Tests.Data;

[Collection(nameof(ApiCollection))]
[Trait("Spec", "phase-2-data-model")]
public class TimestampTests(ApiFactory factory)
{
    // 和真實時間不同，才看得出程式有沒有從 TimeProvider 取時間；
    // 微秒以下是 0，存進 PostgreSQL 再讀出來才會完全相同
    private static readonly DateTimeOffset StartTime = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

    // 秒以下是 .1234567，微秒以下還有 7（100 奈秒）；截到微秒是 .123456
    private static readonly DateTimeOffset Unaligned = StartTime.AddTicks(1_234_567);
    private static readonly DateTimeOffset Truncated = StartTime.AddTicks(1_234_560);

    [Fact(DisplayName = "AC-4：新增資料時自動填入 created_at、updated_at，都是目前時間（UTC），兩者相同")]
    public async Task Insert_FillsCreatedAtAndUpdatedAt()
    {
        // 每讀一次時間就往前 1 毫秒：CreatedAt、UpdatedAt 或每一筆資料各自取一次時間的話，值就會不同
        var time = new FakeTimeProvider(StartTime) { AutoAdvanceAmount = TimeSpan.FromMilliseconds(1) };
        await using var app = WithTime(time);
        var item = TestData.NewOrderItem();
        var history = TestData.NewStatusHistory();
        EntityBase[] entities =
        [
            item.Order.Distributor.User,
            item.Order.Distributor,
            item.ProductVariant.Product.Category,
            item.ProductVariant.Product,
            item.ProductVariant,
            item.Order,
            item,
            history,
        ];

        await app.SaveAsync(entities);
        var afterSave = time.GetUtcNow();

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(
            db.Model.GetEntityTypes().Select(type => type.ClrType.Name).Order(StringComparer.Ordinal),
            entities.Select(entity => entity.GetType().Name).Order(StringComparer.Ordinal));
        var saved = new List<EntityBase>();
        foreach (var entity in entities)
        {
            saved.Add((EntityBase)(await db.FindAsync(entity.GetType(), entity.Id))!);
        }

        // 時間來自測試換上的 TimeProvider，不是電腦的時鐘
        var savedAt = saved[0].CreatedAt;
        Assert.InRange(savedAt, StartTime, afterSave);
        Assert.Equal(TimeSpan.Zero, savedAt.Offset);
        Assert.All(saved, entity =>
        {
            Assert.Equal(savedAt, entity.CreatedAt);
            Assert.Equal(savedAt, entity.UpdatedAt);
        });
    }

    [Fact(DisplayName = "AC-5：修改已經存在的資料時，updated_at 改成目前時間，created_at 不變")]
    public async Task Update_ChangesOnlyUpdatedAt()
    {
        var time = new FakeTimeProvider(StartTime);
        await using var app = WithTime(time);
        var category = TestData.NewCategory();
        await app.SaveAsync(category);
        time.Advance(TimeSpan.FromHours(1));

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var saved = await db.Categories.SingleAsync(c => c.Id == category.Id);
            saved.Name = "除草劑";
            await db.SaveChangesAsync();
        }

        var updated = await app.QueryAsync(db => db.Categories.SingleAsync(c => c.Id == category.Id));
        Assert.Equal(StartTime, updated.CreatedAt);
        Assert.Equal(StartTime.AddHours(1), updated.UpdatedAt);
    }

    [Fact(DisplayName = "AC-5：關掉自動偵測變更、自己呼叫 DetectChanges 時，修改資料也會更新 updated_at")]
    public async Task UpdateWithoutAutoDetectChanges_ChangesUpdatedAt()
    {
        var time = new FakeTimeProvider(StartTime);
        await using var app = WithTime(time);
        var category = TestData.NewCategory();
        await app.SaveAsync(category);
        time.Advance(TimeSpan.FromHours(1));

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ChangeTracker.AutoDetectChangesEnabled = false;
            var saved = await db.Categories.SingleAsync(c => c.Id == category.Id);
            saved.Name = "除草劑";
            db.ChangeTracker.DetectChanges();
            await db.SaveChangesAsync();
        }

        var updated = await app.QueryAsync(db => db.Categories.SingleAsync(c => c.Id == category.Id));
        Assert.Equal("除草劑", updated.Name);
        Assert.Equal(StartTime, updated.CreatedAt);
        Assert.Equal(StartTime.AddHours(1), updated.UpdatedAt);
    }

    // Npgsql 寫入時會直接捨去微秒以下，所以沒有截掉的話，資料庫是 .123456，物件上卻還是 .1234567
    [Fact(DisplayName = "AC-12：目前時間有微秒以下的部分時，存檔後物件上的時間截到微秒，和資料庫一致")]
    public async Task Insert_TruncatesToMicroseconds()
    {
        var time = new FakeTimeProvider(Unaligned);
        await using var app = WithTime(time);
        var category = TestData.NewCategory();

        await app.SaveAsync(category);

        var saved = await app.QueryAsync(db => db.Categories.SingleAsync(c => c.Id == category.Id));
        Assert.Equal(Truncated, category.CreatedAt);
        Assert.Equal(Truncated, category.UpdatedAt);
        Assert.Equal(Truncated, saved.CreatedAt);
        Assert.Equal(Truncated, saved.UpdatedAt);
    }

    [Fact(DisplayName = "AC-12：新增資料時，其他時間欄位也截到微秒，和資料庫一致")]
    public async Task Insert_TruncatesOtherTimes()
    {
        var user = TestData.NewUser();
        user.LockoutUntil = Unaligned;
        user.LastLoginAt = Unaligned;

        await factory.SaveAsync(user);

        var saved = await factory.QueryAsync(db => db.Users.SingleAsync(u => u.Id == user.Id));
        Assert.Equal(Truncated, user.LockoutUntil);
        Assert.Equal(Truncated, user.LastLoginAt);
        Assert.Equal(Truncated, saved.LockoutUntil);
        Assert.Equal(Truncated, saved.LastLoginAt);
    }

    [Fact(DisplayName = "AC-12：修改資料時，改到的時間欄位也截到微秒，和資料庫一致")]
    public async Task Update_TruncatesChangedTimes()
    {
        var user = TestData.NewUser();
        await factory.SaveAsync(user);

        User tracked;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            tracked = await db.Users.SingleAsync(u => u.Id == user.Id);
            tracked.LastLoginAt = Unaligned;
            await db.SaveChangesAsync();
        }

        var saved = await factory.QueryAsync(db => db.Users.SingleAsync(u => u.Id == user.Id));
        Assert.Equal(Truncated, tracked.LastLoginAt);
        Assert.Equal(Truncated, saved.LastLoginAt);
    }

    // 另外啟動一個 API，只有這裡的測試換成固定的時間，不影響其他測試
    private WebApplicationFactory<Program> WithTime(TimeProvider time) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton(time)));
}
