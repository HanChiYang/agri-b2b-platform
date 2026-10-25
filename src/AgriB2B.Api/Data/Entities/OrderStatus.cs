namespace AgriB2B.Api.Data.Entities;

// 刻意不用 0：enum 沒有設定時是 0，忘了指定狀態的訂單存檔時會被資料庫拒絕，不會變成待確認
public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5,
}
