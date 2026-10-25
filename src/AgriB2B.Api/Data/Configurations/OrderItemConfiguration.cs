using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_order_items_unit_price", "unit_price >= 0");
            t.HasCheckConstraint("ck_order_items_quantity", "quantity > 0");
            t.HasCheckConstraint("ck_order_items_subtotal", "subtotal >= 0");
        });

        builder.Property(i => i.ProductName).HasMaxLength(100);
        builder.Property(i => i.VariantName).HasMaxLength(50);
    }
}
