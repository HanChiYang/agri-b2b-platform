namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 包裝規格，例如同一種農藥的 250ml 和 1L。價格含稅。
/// </summary>
public class ProductVariant : EntityBase
{
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public required string Name { get; set; }

    public required string Sku { get; set; }

    public long UnitPrice { get; set; }

    public int SortOrder { get; set; }

    // 上架。新增時預設下架，管理員確認資料後再上架
    public bool IsActive { get; set; }
}
