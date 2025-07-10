namespace NGOPlatformWeb.Models.ViewModels
{
    public class CaseProfileViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; } // 來自 CaseLogins 表
        public string Phone { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string Address { get; set; }

        // 密碼編輯專用欄位（不做顯示）
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
