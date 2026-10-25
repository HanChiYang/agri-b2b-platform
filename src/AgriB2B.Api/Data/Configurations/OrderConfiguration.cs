using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_orders_status", CheckConstraintSql.EnumNames<OrderStatus>("status"));
            t.HasCheckConstraint("ck_orders_total_amount", "total_amount >= 0");
        });
        builder.HasIndex(o => o.OrderNo).IsUnique();

        builder.Property(o => o.OrderNo).HasMaxLength(20);
        builder.Property(o => o.RecipientName).HasMaxLength(50);
        builder.Property(o => o.RecipientPhone).HasMaxLength(30);
        builder.Property(o => o.ShippingAddress).HasMaxLength(200);
        builder.Property(o => o.Note).HasMaxLength(500);
    }
}
