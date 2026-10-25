namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 商品。同一種商品的不同包裝放在 <see cref="ProductVariant"/>，見 ADR-0007。
/// </summary>
public class Product : EntityBase
{
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public required string Name { get; set; }

    public required string LicenseNo { get; set; }

    public required string ActiveIngredient { get; set; }

    public required string Formulation { get; set; }

    public required string Manufacturer { get; set; }

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    // 上架。新增時預設下架，管理員確認資料後再上架
    public bool IsActive { get; set; }
}
