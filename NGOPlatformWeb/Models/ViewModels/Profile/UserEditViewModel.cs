using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Profile
{
    public class UserEditViewModel : BaseProfileViewModel
    {
        // 編輯時的當前密碼
        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string? Password { get; set; }
        
        // 繼承自 BaseProfileViewModel 的屬性：
        // Name, Email, Phone, IdentityNumber, ProfileImage
        // NewPassword, ConfirmPassword (已定義於基類)
        // TotalActivitiesRegistered, ActiveRegistrations
    }
}
