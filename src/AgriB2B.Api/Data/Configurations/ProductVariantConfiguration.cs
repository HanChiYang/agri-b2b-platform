using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("ck_product_variants_unit_price", "unit_price >= 0"));
        builder.HasIndex(v => v.Sku).IsUnique();

        builder.Property(v => v.Name).HasMaxLength(50);
        builder.Property(v => v.Sku).HasMaxLength(50);
    }
}
