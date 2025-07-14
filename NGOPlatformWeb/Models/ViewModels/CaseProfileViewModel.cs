using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class CaseProfileViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [StringLength(50, ErrorMessage = "姓名長度不能超過50個字")]
        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        [Display(Name = "電子信箱")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入電話號碼")]
        [StringLength(20, ErrorMessage = "電話號碼長度不能超過20個字")]
        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        [Display(Name = "電話號碼")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入身份證字號")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "身份證字號必須為10個字")]
        [Display(Name = "身份證字號")]
        public string IdentityNumber { get; set; } = string.Empty;

        [Display(Name = "生日")]
        public DateTime? Birthday { get; set; }
        
        [Required(ErrorMessage = "請輸入地址")]
        [StringLength(200, ErrorMessage = "地址長度不能超過200個字")]
        [Display(Name = "地址")]
        public string Address { get; set; } = string.Empty;

        // 密碼編輯專用欄位（不做顯示）
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        public string? NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
        public string? ConfirmPassword { get; set; }
    }
}
