namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 每張資料表都有的欄位。CreatedAt、UpdatedAt 在存檔時自動填入，程式不用自己填。
/// </summary>
public abstract class EntityBase
{
    public int Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
