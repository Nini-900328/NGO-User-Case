namespace NGOPlatformWeb.Models.ViewModels
{
    public class CaseActivityRegistrationsViewModel
    {
        public string CaseName { get; set; } = string.Empty;
        public int TotalRegistrations { get; set; }
        public int ActiveRegistrations { get; set; }
        public List<CaseActivityRegistrationItem> Registrations { get; set; } = new List<CaseActivityRegistrationItem>();
    }
    
    public class CaseActivityRegistrationItem
    {
        public int RegistrationId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegisterTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty;
        
        // 計算屬性
        public bool IsUpcoming => StartDate > DateTime.Now;
        public bool IsCompleted => EndDate < DateTime.Now;
        public bool IsActive => Status == "registered";
        public string StatusDisplay => Status == "registered" ? "已報名" : "已取消";
        public string CategoryDisplay => Category switch
        {
            "生活" => "生活技能",
            "心靈" => "心靈成長", 
            "運動" => "運動健康",
            _ => Category
        };
    }
}