using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class DistributorConfiguration : IEntityTypeConfiguration<Distributor>
{
    public void Configure(EntityTypeBuilder<Distributor> builder)
    {
        // 一家經銷商一個帳號。設成一對一，EF 會在 user_id 建立唯一索引
        builder.HasOne(d => d.User).WithOne().HasForeignKey<Distributor>(d => d.UserId);
        builder.HasIndex(d => d.TaxId).IsUnique();
        // 統一編號是 8 位數字，空字串也不行：沒有統一編號就存空值。空值時 CHECK 的結果不是 false，所以可以存
        builder.ToTable(t => t.HasCheckConstraint("ck_distributors_tax_id", "tax_id ~ '^[0-9]{8}$'"));

        builder.Property(d => d.CompanyName).HasMaxLength(100);
        builder.Property(d => d.TaxId).HasMaxLength(8);
        builder.Property(d => d.ContactName).HasMaxLength(50);
        builder.Property(d => d.Phone).HasMaxLength(30);
        builder.Property(d => d.Email).HasMaxLength(254);
        builder.Property(d => d.Address).HasMaxLength(200);
        builder.Property(d => d.Note).HasMaxLength(500);
    }
}
