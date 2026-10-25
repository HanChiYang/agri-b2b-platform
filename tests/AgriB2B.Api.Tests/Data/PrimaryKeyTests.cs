using AgriB2B.Api.Data;
using AgriB2B.Api.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace AgriB2B.Api.Tests.Data;

[Collection(nameof(ApiCollection))]
[Trait("Spec", "uuid-primary-keys")]
public class PrimaryKeyTests(ApiFactory factory)
{
    [Fact(DisplayName = "AC-1：沒有指定 id 時，存檔時自動產生 UUIDv7，不是全 0，彼此不重複，用這個 id 讀得到同一筆")]
    public async Task Insert_GeneratesUuidV7Ids()
    {
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
        Assert.All(entities, entity => Assert.Equal(Guid.Empty, entity.Id));

        await factory.SaveAsync(entities);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(
            db.Model.GetEntityTypes().Select(type => type.ClrType.Name).Order(StringComparer.Ordinal),
            entities.Select(entity => entity.GetType().Name).Order(StringComparer.Ordinal));
        Assert.Equal(entities.Length, entities.Select(entity => entity.Id).Distinct().Count());
        foreach (var entity in entities)
        {
            Assert.NotEqual(Guid.Empty, entity.Id);
            Assert.Equal(7, entity.Id.Version);
            var saved = (EntityBase?)await db.FindAsync(entity.GetType(), entity.Id);
            Assert.NotNull(saved);
            Assert.Equal(entity.Id, saved.Id);
        }
    }
}
