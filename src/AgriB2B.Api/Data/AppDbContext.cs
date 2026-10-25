using AgriB2B.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriB2B.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Distributor> Distributors => Set<Distributor>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 角色和訂單狀態存成文字（例如 Admin、Pending），直接查資料庫也看得懂
        configurationBuilder.Properties<UserRole>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<OrderStatus>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // 資料只停用、不刪除。外鍵一律設成還有資料參照時不能刪除，防止誤刪。
        // 這段在套用設定類別之後執行，設定類別裡寫的 OnDelete 也會被這裡蓋掉
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(type => type.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
