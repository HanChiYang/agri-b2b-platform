using AgriB2B.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AgriB2B.Api.Tests.Data;

/// <summary>
/// 測試直接存取資料庫用。每次都用新的 DbContext，和 API 處理每個請求時一樣；
/// 讀出來的也一定是資料庫裡的值，而不是 DbContext 記住的物件。
/// </summary>
internal static class DatabaseExtensions
{
    public static async Task SaveAsync(this WebApplicationFactory<Program> app, params object[] entities)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AddRange(entities);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 存檔並確認被資料庫拒絕，回傳資料庫的錯誤，讓測試檢查是哪一種錯誤、哪一張資料表。
    /// </summary>
    public static async Task<PostgresException> SaveRejectedAsync(this WebApplicationFactory<Program> app, params object[] entities)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AddRange(entities);
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        return Assert.IsType<PostgresException>(exception.InnerException);
    }

    /// <summary>
    /// 直接執行 SQL 並確認被資料庫拒絕。用來測試不經過 EF Core 寫入時，資料庫的限制還是有效。
    /// </summary>
    public static async Task<PostgresException> ExecuteRejectedAsync(this WebApplicationFactory<Program> app, FormattableString sql)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlAsync(sql));
    }

    public static async Task<T> QueryAsync<T>(this WebApplicationFactory<Program> app, Func<AppDbContext, Task<T>> query)
    {
        await using var scope = app.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
