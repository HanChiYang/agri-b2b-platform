using Microsoft.EntityFrameworkCore;

namespace AgriB2B.Api.Tests.Data;

[Collection(nameof(ApiCollection))]
[Trait("Spec", "phase-2-data-model")]
public class MigrationTests(ApiFactory factory)
{
    [Fact(DisplayName = "AC-1：打開 Database:MigrateOnStartup 啟動 API 時，自動套用 migration，建立 8 張資料表")]
    public async Task Startup_AppliesMigrations()
    {
        // ApiFactory 打開了 Database:MigrateOnStartup，測試沒有自己套用 migration；資料表存在，就表示是 API 啟動時套用的
        var applied = await factory.QueryAsync(db => db.Database.GetAppliedMigrationsAsync());
        var pending = await factory.QueryAsync(db => db.Database.GetPendingMigrationsAsync());
        var tables = await factory.QueryAsync(db => db.Database.SqlQuery<string>($"""
            SELECT table_name AS "Value"
            FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name <> '__EFMigrationsHistory'
            """).ToListAsync());

        Assert.NotEmpty(applied);
        Assert.Empty(pending);
        Assert.Equal(
            [
                "categories",
                "distributors",
                "order_items",
                "order_status_histories",
                "orders",
                "product_variants",
                "products",
                "users",
            ],
            tables.Order(StringComparer.Ordinal));
    }

    [Fact(DisplayName = "AC-2：實體類別和設定都有對應的 migration")]
    public async Task Model_HasNoPendingChanges()
    {
        // 目前 API 啟動時套用 migration，EF Core 發現 model 和 migration 不一致就會拋出例外，
        // 所有測試會在這行之前就一起失敗。保留這個檢查，是為了哪天測試不在啟動時套用 migration，還是會被擋下
        var hasPendingChanges = await factory.QueryAsync(db => Task.FromResult(db.Database.HasPendingModelChanges()));

        Assert.False(hasPendingChanges, "實體類別或設定改了，卻沒有建立新的 migration。請執行 dotnet ef migrations add <名稱>");
    }
}
