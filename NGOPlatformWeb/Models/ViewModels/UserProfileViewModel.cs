using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string? Password { get; set; }
        
        // 活動報名總覽
        public int TotalActivitiesRegistered { get; set; }
        public int ActiveRegistrations { get; set; }
        public List<ActivitySummary> RecentActivities { get; set; } = new List<ActivitySummary>();
        
        // 認購總覽
        public int TotalPurchaseOrders { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public List<PurchaseSummary> RecentPurchases { get; set; } = new List<PurchaseSummary>();
    }
    
    public class ActivitySummary
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
    
    public class PurchaseSummary
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
    }
}
