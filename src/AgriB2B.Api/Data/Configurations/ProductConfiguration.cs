using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);
        builder.Property(p => p.LicenseNo).HasMaxLength(50);
        builder.Property(p => p.ActiveIngredient).HasMaxLength(200);
        builder.Property(p => p.Formulation).HasMaxLength(50);
        builder.Property(p => p.Manufacturer).HasMaxLength(100);
        builder.Property(p => p.ImagePath).HasMaxLength(300);
    }
}
