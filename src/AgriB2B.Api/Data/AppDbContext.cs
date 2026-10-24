using Microsoft.EntityFrameworkCore;

namespace AgriB2B.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
