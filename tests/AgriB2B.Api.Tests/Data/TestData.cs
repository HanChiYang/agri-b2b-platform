using System.Globalization;
using AgriB2B.Api.Data.Entities;

namespace AgriB2B.Api.Tests.Data;

/// <summary>
/// 產生可以直接存檔的測試資料。測試共用同一個資料庫，唯一欄位每次都用不同的值，測試之間才不會互相影響。
/// 參照的資料預設一起新建；要參照已經存在的資料時，傳入它的 id。
/// 有預設值的欄位（例如 IsActive）都不指定，AC-11 要檢查預設值。
/// </summary>
internal static class TestData
{
    private static int _sequence;

    private static int Next() => Interlocked.Increment(ref _sequence);

    public static User NewUser(UserRole role = UserRole.Distributor) => new()
    {
        Account = $"user{Next()}",
        DisplayName = "測試帳號",
        PasswordHash = "not-a-real-hash",
        Role = role,
        SecurityStamp = "test-stamp",
    };

    public static Distributor NewDistributor(int? userId = null)
    {
        var distributor = new Distributor
        {
            CompanyName = "測試農藥行",
            TaxId = Next().ToString("D8", CultureInfo.InvariantCulture),
            ContactName = "王小明",
            Phone = "02-1234-5678",
            Address = "台北市中正區測試路 1 號",
        };

        if (userId is { } id)
        {
            distributor.UserId = id;
        }
        else
        {
            distributor.User = NewUser();
        }

        return distributor;
    }

    public static Category NewCategory() => new()
    {
        Name = "殺蟲劑",
        SortOrder = 1,
    };

    public static Product NewProduct(int? categoryId = null)
    {
        var product = new Product
        {
            Name = "測試殺蟲劑",
            LicenseNo = "農藥製字第 00001 號",
            ActiveIngredient = "賽洛寧 2.8%",
            Formulation = "乳劑",
            Manufacturer = "測試農化股份有限公司",
        };

        if (categoryId is { } id)
        {
            product.CategoryId = id;
        }
        else
        {
            product.Category = NewCategory();
        }

        return product;
    }

    public static ProductVariant NewVariant(int? productId = null)
    {
        var variant = new ProductVariant
        {
            Name = "1L",
            Sku = $"SKU-{Next()}",
            UnitPrice = 450,
            SortOrder = 1,
        };

        if (productId is { } id)
        {
            variant.ProductId = id;
        }
        else
        {
            variant.Product = NewProduct();
        }

        return variant;
    }

    public static Order NewOrder(int? distributorId = null)
    {
        var order = new Order
        {
            OrderNo = $"20260101-{Next():D4}",
            Status = OrderStatus.Pending,
            RecipientName = "王小明",
            RecipientPhone = "0912-345-678",
            ShippingAddress = "台北市中正區測試路 1 號",
            TotalAmount = 900,
        };

        if (distributorId is { } id)
        {
            order.DistributorId = id;
        }
        else
        {
            order.Distributor = NewDistributor();
        }

        return order;
    }

    public static OrderItem NewOrderItem() => new()
    {
        Order = NewOrder(),
        ProductVariant = NewVariant(),
        ProductName = "測試殺蟲劑",
        VariantName = "1L",
        UnitPrice = 450,
        Quantity = 2,
        Subtotal = 900,
    };

    public static OrderStatusHistory NewStatusHistory() => new()
    {
        Order = NewOrder(),
        ToStatus = OrderStatus.Pending,
        ChangedByUser = NewUser(UserRole.Admin),
    };
}
