using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriB2B.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_users_role", CheckConstraintSql.EnumNames<UserRole>("role"));
            // 帳號不分大小寫：API 存檔前先轉小寫、去掉前後的空白，資料庫只接受處理過的帳號，唯一索引也就不分大小寫
            t.HasCheckConstraint("ck_users_account_lowercase", "account = lower(account)");
            t.HasCheckConstraint("ck_users_account_trimmed", CheckConstraintSql.Trimmed("account"));
        });
        builder.HasIndex(u => u.Account).IsUnique();

        builder.Property(u => u.Account).HasMaxLength(50);
        builder.Property(u => u.DisplayName).HasMaxLength(100);
        builder.Property(u => u.PasswordHash).HasMaxLength(200);
        builder.Property(u => u.SecurityStamp).HasMaxLength(64);
    }
}
