namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 每張資料表都有的欄位，程式不用自己填：Id 在加進 DbContext 時產生，CreatedAt、UpdatedAt 在存檔時填入。
/// </summary>
public abstract class EntityBase
{
    // UUIDv7，見 ADR-0018。Guid 主鍵沒有指定值時，由 Npgsql 產生，不由資料庫產生
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
