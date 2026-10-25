namespace AgriB2B.Api.Data.Configurations;

internal static class CheckConstraintSql
{
    // .NET 的 string.Trim() 會去掉的字元（char.IsWhiteSpace），包含 tab、換行、全形空白。
    // 寫成 \uXXXX 讓資料庫的規則和 API 一致，也不受資料庫語系設定的影響
    private static readonly string WhiteSpace = string.Concat(
        Enumerable.Range(char.MinValue, char.MaxValue + 1)
            .Where(c => char.IsWhiteSpace((char)c))
            .Select(c => $"\\u{c:x4}"));

    /// <summary>
    /// 欄位只能存 enum 有定義的名稱，例如 <c>role IN ('Admin', 'Distributor')</c>。
    /// 名稱從 enum 產生，enum 改了 model 就會跟著變，忘了建立 migration 時 AC-2 會擋下。
    /// </summary>
    public static string EnumNames<TEnum>(string column) where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"))})";

    /// <summary>
    /// 欄位不能是空的，前後也不能有空白：第一個和最後一個字元都不是空白（只有一個字元時是同一個）。
    /// </summary>
    public static string Trimmed(string column) =>
        $"{column} ~ '^[^{WhiteSpace}](.*[^{WhiteSpace}])?$'";
}
