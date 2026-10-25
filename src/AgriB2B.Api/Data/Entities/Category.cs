namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 商品分類。
/// </summary>
public class Category : EntityBase
{
    public required string Name { get; set; }

    public int SortOrder { get; set; }

    // 上架。新增時預設下架，管理員確認資料後再上架
    public bool IsActive { get; set; }
}
