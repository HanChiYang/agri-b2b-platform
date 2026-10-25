namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 訂單明細。商品名稱、規格、單價都存下單時的值，之後商品改名或改價，舊訂單不受影響。
/// </summary>
public class OrderItem : EntityBase
{
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public Guid ProductVariantId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public required string ProductName { get; set; }

    public required string VariantName { get; set; }

    public long UnitPrice { get; set; }

    public int Quantity { get; set; }

    public long Subtotal { get; set; }
}
