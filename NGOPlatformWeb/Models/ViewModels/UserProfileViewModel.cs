using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class UserProfileViewModel : BaseProfileViewModel
    {
        public string? Password { get; set; }
        
        // 活動報名總覽
        public List<ActivitySummary> RecentActivities { get; set; } = new List<ActivitySummary>();
        
        // 認購總覽（用戶專有）
        public int TotalPurchaseOrders { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public List<PurchaseSummary> RecentPurchases { get; set; } = new List<PurchaseSummary>();
    }
}
