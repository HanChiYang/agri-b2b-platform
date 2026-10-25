using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriB2B.Api.Tests.Data;

[Collection(nameof(ApiCollection))]
[Trait("Spec", "phase-2-data-model")]
public class StoredValueTests(ApiFactory factory)
{
    [Fact(DisplayName = "AC-10：角色和訂單狀態在資料庫存成文字")]
    public async Task Enums_AreStoredAsText()
    {
        var admin = TestData.NewUser(UserRole.Admin);
        var order = TestData.NewOrder();
        await factory.SaveAsync(admin, order);

        var role = await factory.QueryAsync(db => db.Database
            .SqlQuery<string>($"""SELECT role AS "Value" FROM users WHERE id = {admin.Id}""")
            .SingleAsync());
        var status = await factory.QueryAsync(db => db.Database
            .SqlQuery<string>($"""SELECT status AS "Value" FROM orders WHERE id = {order.Id}""")
            .SingleAsync());

        Assert.Equal("Admin", role);
        Assert.Equal("Pending", status);
    }

    [Fact(DisplayName = "AC-11：新增時沒有指定的話，帳號預設啟用、要改密碼、登入失敗次數 0")]
    public async Task NewUser_HasDefaults()
    {
        var user = TestData.NewUser();
        await factory.SaveAsync(user);

        var saved = await factory.QueryAsync(db => db.Users.SingleAsync(u => u.Id == user.Id));

        Assert.True(saved.IsActive);
        Assert.True(saved.MustChangePassword);
        Assert.Equal(0, saved.FailedLoginCount);
    }

    [Fact(DisplayName = "AC-11：新增時沒有指定的話，分類、商品、包裝規格預設下架")]
    public async Task NewCatalogItems_AreInactiveByDefault()
    {
        var variant = TestData.NewVariant();
        await factory.SaveAsync(variant);

        var savedVariant = await factory.QueryAsync(db => db.ProductVariants.SingleAsync(v => v.Id == variant.Id));
        var savedProduct = await factory.QueryAsync(db => db.Products.SingleAsync(p => p.Id == variant.ProductId));
        var savedCategory = await factory.QueryAsync(db => db.Categories.SingleAsync(c => c.Id == variant.Product.CategoryId));

        Assert.False(savedVariant.IsActive);
        Assert.False(savedProduct.IsActive);
        Assert.False(savedCategory.IsActive);
    }
}
