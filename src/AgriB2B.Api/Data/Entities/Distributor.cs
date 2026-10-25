namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 經銷商資料。是否停用看對應帳號的 <see cref="User.IsActive"/>。
/// </summary>
public class Distributor : EntityBase
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public required string CompanyName { get; set; }

    // 選填：有些經銷商沒有統一編號。有填的不能重複
    public string? TaxId { get; set; }

    public required string ContactName { get; set; }

    public required string Phone { get; set; }

    public string? Email { get; set; }

    public required string Address { get; set; }

    public string? Note { get; set; }
}
