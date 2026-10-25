namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 訂單狀態歷程。<see cref="EntityBase.CreatedAt"/> 就是變更時間。
/// </summary>
public class OrderStatusHistory : EntityBase
{
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    // 下單時的第一筆沒有原狀態
    public OrderStatus? FromStatus { get; set; }

    // 沒有預設值：忘了指定時編譯就會失敗
    public required OrderStatus ToStatus { get; set; }

    public Guid ChangedBy { get; set; }

    public User ChangedByUser { get; set; } = null!;

    public string? Note { get; set; }
}
