using Microsoft.EntityFrameworkCore;
namespace NGOPlatformWeb.Models.Entity
{
    public class NGODbContext : DbContext
    {
        public NGODbContext(DbContextOptions<NGODbContext> options) : base(options) { }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyCategory> SupplyCategories { get; set; }
        // 後續其他 DbSet 也可一起加上
    }
}
