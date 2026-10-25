namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 訂單。
/// </summary>
public class Order : EntityBase
{
    public required string OrderNo { get; set; }

    public int DistributorId { get; set; }

    public Distributor Distributor { get; set; } = null!;

    // 沒有預設值：忘了指定時編譯就會失敗
    public required OrderStatus Status { get; set; }

    public required string RecipientName { get; set; }

    public required string RecipientPhone { get; set; }

    public required string ShippingAddress { get; set; }

    public string? Note { get; set; }

    public long TotalAmount { get; set; }
}
