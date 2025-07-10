using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.ViewModels;
namespace NGOPlatformWeb.Models.Entity
{
    public class NGODbContext : DbContext
    {
        public NGODbContext(DbContextOptions<NGODbContext> options) : base(options) { }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyCategory> SupplyCategories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseLogin> CaseLogins { get; set; }
        // 後續其他 DbSet 也可一起加上
        public DbSet<RegularSupplyNeeds> RegularSuppliesNeeds { get; set; }
        public DbSet<EmergencySupplyNeeds> EmergencySupplyNeeds { get; set; }

    }
}
