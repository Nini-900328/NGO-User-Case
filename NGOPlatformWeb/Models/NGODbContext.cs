using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.ViewModels;
namespace NGOPlatformWeb.Models.Entity
{
    public class NGODbContext : DbContext
    {
        public NGODbContext(DbContextOptions<NGODbContext> options) : base(options) { }


        public DbSet<Activity> Activities { get; set; } = default!;
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyCategory> SupplyCategories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseLogin> CaseLogins { get; set; }
        public DbSet<RegularSupplyNeeds> RegularSuppliesNeeds { get; set; }
        public DbSet<EmergencySupplyNeeds> EmergencySupplyNeeds { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        // 捐贈訂單系統 - 支援組合包分解為單項物資
        public DbSet<UserOrder> UserOrders { get; set; }
        public DbSet<UserOrderDetail> UserOrderDetails { get; set; }
        
        // 活動報名系統
        public DbSet<UserActivityRegistration> UserActivityRegistrations { get; set; }
        public DbSet<CaseActivityRegistrations> CaseActivityRegistrations { get; set; }
        
        // 緊急物資認購記錄系統
        public DbSet<EmergencyPurchaseRecord> EmergencyPurchaseRecords { get; set; }
        
        // ECPay 付款交易記錄
        public DbSet<EcpayTransaction> EcpayTransactions { get; set; }
        
        // 成就系統
        public DbSet<UserAchievement> UserAchievements { get; set; }
    }
}
