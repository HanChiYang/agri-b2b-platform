using AgriB2B.Api.Data.Entities;
using Npgsql;

namespace AgriB2B.Api.Tests.Data;

/// <summary>
/// 每個測試只讓一個欄位違反限制，再檢查錯誤的種類和限制的名稱，確認是被這條限制擋下，而不是其他原因。
/// </summary>
[Collection(nameof(ApiCollection))]
[Trait("Spec", "phase-2-data-model")]
public class ConstraintTests(ApiFactory factory)
{
    [Fact(DisplayName = "AC-6：帳號重複時資料庫拒絕")]
    public async Task DuplicateAccount_IsRejected()
    {
        var first = TestData.NewUser();
        await factory.SaveAsync(first);
        var second = TestData.NewUser();
        second.Account = first.Account;

        var error = await factory.SaveRejectedAsync(second);

        AssertRejected(error, PostgresErrorCodes.UniqueViolation, "ix_users_account");
    }

    [Fact(DisplayName = "AC-6：同一個帳號建立兩筆經銷商資料時資料庫拒絕")]
    public async Task DuplicateDistributorUser_IsRejected()
    {
        var first = TestData.NewDistributor();
        await factory.SaveAsync(first);

        var error = await factory.SaveRejectedAsync(TestData.NewDistributor(userId: first.UserId));

        AssertRejected(error, PostgresErrorCodes.UniqueViolation, "ix_distributors_user_id");
    }

    [Fact(DisplayName = "AC-6：統一編號重複時資料庫拒絕")]
    public async Task DuplicateTaxId_IsRejected()
    {
        var first = TestData.NewDistributor();
        await factory.SaveAsync(first);
        var second = TestData.NewDistributor();
        second.TaxId = first.TaxId;

        var error = await factory.SaveRejectedAsync(second);

        AssertRejected(error, PostgresErrorCodes.UniqueViolation, "ix_distributors_tax_id");
    }

    [Fact(DisplayName = "AC-6：沒有填統一編號的經銷商可以有好幾家")]
    public async Task DistributorsWithoutTaxId_AreAccepted()
    {
        var first = TestData.NewDistributor();
        first.TaxId = null;
        var second = TestData.NewDistributor();
        second.TaxId = null;

        await factory.SaveAsync(first);
        await factory.SaveAsync(second);
    }

    [Fact(DisplayName = "AC-6：料號重複時資料庫拒絕")]
    public async Task DuplicateSku_IsRejected()
    {
        var first = TestData.NewVariant();
        await factory.SaveAsync(first);
        var second = TestData.NewVariant(productId: first.ProductId);
        second.Sku = first.Sku;

        var error = await factory.SaveRejectedAsync(second);

        AssertRejected(error, PostgresErrorCodes.UniqueViolation, "ix_product_variants_sku");
    }

    [Fact(DisplayName = "AC-6：訂單編號重複時資料庫拒絕")]
    public async Task DuplicateOrderNo_IsRejected()
    {
        var first = TestData.NewOrder();
        await factory.SaveAsync(first);
        var second = TestData.NewOrder(distributorId: first.DistributorId);
        second.OrderNo = first.OrderNo;

        var error = await factory.SaveRejectedAsync(second);

        AssertRejected(error, PostgresErrorCodes.UniqueViolation, "ix_orders_order_no");
    }

    [Fact(DisplayName = "AC-7：商品的分類不存在時資料庫拒絕")]
    public async Task ProductWithMissingCategory_IsRejected()
    {
        var error = await factory.SaveRejectedAsync(TestData.NewProduct(categoryId: -1));

        AssertRejected(error, PostgresErrorCodes.ForeignKeyViolation, "fk_products_categories_category_id");
    }

    [Fact(DisplayName = "AC-9：包裝規格的單價是負數時資料庫拒絕")]
    public async Task NegativeVariantPrice_IsRejected()
    {
        var variant = TestData.NewVariant();
        variant.UnitPrice = -1;

        var error = await factory.SaveRejectedAsync(variant);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_product_variants_unit_price");
    }

    [Fact(DisplayName = "AC-9：訂單總金額是負數時資料庫拒絕")]
    public async Task NegativeOrderTotal_IsRejected()
    {
        var order = TestData.NewOrder();
        order.TotalAmount = -1;

        var error = await factory.SaveRejectedAsync(order);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_orders_total_amount");
    }

    [Fact(DisplayName = "AC-9：明細的單價是負數時資料庫拒絕")]
    public async Task NegativeItemPrice_IsRejected()
    {
        var item = TestData.NewOrderItem();
        item.UnitPrice = -1;

        var error = await factory.SaveRejectedAsync(item);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_order_items_unit_price");
    }

    [Fact(DisplayName = "AC-9：明細的小計是負數時資料庫拒絕")]
    public async Task NegativeItemSubtotal_IsRejected()
    {
        var item = TestData.NewOrderItem();
        item.Subtotal = -1;

        var error = await factory.SaveRejectedAsync(item);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_order_items_subtotal");
    }

    [Fact(DisplayName = "AC-9：明細的數量是 0 時資料庫拒絕")]
    public async Task ZeroQuantity_IsRejected()
    {
        var item = TestData.NewOrderItem();
        item.Quantity = 0;

        var error = await factory.SaveRejectedAsync(item);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_order_items_quantity");
    }

    // 「不可小於 0」和「必須大於 0」的邊界：金額 0、數量 1 要能存
    [Fact(DisplayName = "AC-9：金額是 0、數量是 1 時可以存檔")]
    public async Task ZeroAmountsAndQuantityOne_AreAccepted()
    {
        var item = TestData.NewOrderItem();
        item.ProductVariant.UnitPrice = 0;
        item.Order.TotalAmount = 0;
        item.UnitPrice = 0;
        item.Subtotal = 0;
        item.Quantity = 1;

        await factory.SaveAsync(item);
    }

    [Fact(DisplayName = "AC-13：沒有指定角色時資料庫拒絕")]
    public async Task UserWithoutRole_IsRejected()
    {
        var user = TestData.NewUser();
        user.Role = default;

        var error = await factory.SaveRejectedAsync(user);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_users_role");
    }

    [Fact(DisplayName = "AC-13：沒有指定訂單狀態時資料庫拒絕")]
    public async Task OrderWithoutStatus_IsRejected()
    {
        var order = TestData.NewOrder();
        order.Status = default;

        var error = await factory.SaveRejectedAsync(order);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_orders_status");
    }

    [Fact(DisplayName = "AC-13：沒有指定狀態歷程的新狀態時資料庫拒絕")]
    public async Task HistoryWithoutToStatus_IsRejected()
    {
        var history = TestData.NewStatusHistory();
        history.ToStatus = default;

        var error = await factory.SaveRejectedAsync(history);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_order_status_histories_to_status");
    }

    [Fact(DisplayName = "AC-13：直接用 SQL 把角色改成沒有定義的名稱時資料庫拒絕")]
    public async Task UndefinedRole_IsRejected()
    {
        var user = TestData.NewUser();
        await factory.SaveAsync(user);

        var error = await factory.ExecuteRejectedAsync($"UPDATE users SET role = 'admin' WHERE id = {user.Id}");

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_users_role");
    }

    [Fact(DisplayName = "AC-13：直接用 SQL 把訂單狀態改成沒有定義的名稱時資料庫拒絕")]
    public async Task UndefinedOrderStatus_IsRejected()
    {
        var order = TestData.NewOrder();
        await factory.SaveAsync(order);

        var error = await factory.ExecuteRejectedAsync($"UPDATE orders SET status = 'Canceled' WHERE id = {order.Id}");

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_orders_status");
    }

    [Fact(DisplayName = "AC-13：直接用 SQL 把狀態歷程的原狀態改成沒有定義的名稱時資料庫拒絕")]
    public async Task UndefinedHistoryFromStatus_IsRejected()
    {
        var history = TestData.NewStatusHistory();
        await factory.SaveAsync(history);

        var error = await factory.ExecuteRejectedAsync(
            $"UPDATE order_status_histories SET from_status = 'pending' WHERE id = {history.Id}");

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_order_status_histories_from_status");
    }

    [Fact(DisplayName = "AC-13：直接用 SQL 把狀態歷程的新狀態改成沒有定義的名稱時資料庫拒絕")]
    public async Task UndefinedHistoryToStatus_IsRejected()
    {
        var history = TestData.NewStatusHistory();
        await factory.SaveAsync(history);

        var error = await factory.ExecuteRejectedAsync(
            $"UPDATE order_status_histories SET to_status = 'Canceled' WHERE id = {history.Id}");

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_order_status_histories_to_status");
    }

    [Fact(DisplayName = "AC-13：enum 有定義的角色和訂單狀態都可以存")]
    public async Task DefinedEnumValues_AreAccepted()
    {
        var users = Enum.GetValues<UserRole>().Select(TestData.NewUser);
        var histories = Enum.GetValues<OrderStatus>().Select(status =>
        {
            var history = TestData.NewStatusHistory();
            history.Order.Status = status;
            history.FromStatus = status;
            history.ToStatus = status;
            return history;
        });

        await factory.SaveAsync([.. users, .. histories]);
    }

    [Fact(DisplayName = "AC-14：帳號有大寫字母時資料庫拒絕")]
    public async Task UppercaseAccount_IsRejected()
    {
        var user = TestData.NewUser();
        user.Account = "Upper" + user.Account;

        var error = await factory.SaveRejectedAsync(user);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_users_account_lowercase");
    }

    [Theory(DisplayName = "AC-14：帳號是空的或前後有空白時資料庫拒絕")]
    [InlineData("")]
    [InlineData(" padded-front")]
    [InlineData("padded-back ")]
    [InlineData("padded-tab\t")]
    [InlineData("padded-ideographic　")] // 全形空白，中文輸入法常打出來

    public async Task BlankOrPaddedAccount_IsRejected(string account)
    {
        var user = TestData.NewUser();
        user.Account = account;

        var error = await factory.SaveRejectedAsync(user);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_users_account_trimmed");
    }

    [Theory(DisplayName = "AC-15：統一編號不是 8 位數字時資料庫拒絕")]
    [InlineData("")]
    [InlineData("1234567")]
    [InlineData("A2345678")]
    public async Task InvalidTaxId_IsRejected(string taxId)
    {
        var distributor = TestData.NewDistributor();
        distributor.TaxId = taxId;

        var error = await factory.SaveRejectedAsync(distributor);

        AssertRejected(error, PostgresErrorCodes.CheckViolation, "ck_distributors_tax_id");
    }

    private static void AssertRejected(PostgresException error, string sqlState, string constraint)
    {
        Assert.Equal(sqlState, error.SqlState);
        Assert.Equal(constraint, error.ConstraintName);
    }
}
