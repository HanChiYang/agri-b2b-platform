using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        // from_status 是空值時，CHECK 的結果不是 false，所以第一筆沒有原狀態也能存
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_order_status_histories_from_status", CheckConstraintSql.EnumNames<OrderStatus>("from_status"));
            t.HasCheckConstraint("ck_order_status_histories_to_status", CheckConstraintSql.EnumNames<OrderStatus>("to_status"));
        });
        builder.HasOne(h => h.ChangedByUser).WithMany().HasForeignKey(h => h.ChangedBy);

        builder.Property(h => h.Note).HasMaxLength(500);
    }
}
