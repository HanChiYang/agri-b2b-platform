using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AgriB2B.Api.Data;

/// <summary>
/// 存檔前自動填入 CreatedAt、UpdatedAt，並把所有時間截到微秒。時間從 TimeProvider 取得，測試可以換成固定的時間。
/// </summary>
public class TimestampInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetTimestamps(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetTimestamps(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void SetTimestamps(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        // 同一次存檔只取一次時間，新增的資料 CreatedAt 和 UpdatedAt 才會相同
        var now = TruncateToMicroseconds(timeProvider.GetUtcNow());
        foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            // 透過 entry 設定值，EF 才知道欄位改了；直接改物件的屬性，關掉自動偵測變更時不會存進資料庫
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTimeOffset value && value != TruncateToMicroseconds(value))
                {
                    property.CurrentValue = TruncateToMicroseconds(value);
                }
            }

            entry.Property(e => e.UpdatedAt).CurrentValue = now;
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
            }
            else
            {
                // 就算程式改了 CreatedAt，也不會存進資料庫
                entry.Property(e => e.CreatedAt).IsModified = false;
            }
        }
    }

    // PostgreSQL 只存到微秒，Npgsql 寫入時直接捨去微秒以下。先在這裡截掉，存檔後物件上的時間才會和資料庫裡的一樣
    private static DateTimeOffset TruncateToMicroseconds(DateTimeOffset value) =>
        value.AddTicks(-(value.Ticks % TimeSpan.TicksPerMicrosecond));
}
