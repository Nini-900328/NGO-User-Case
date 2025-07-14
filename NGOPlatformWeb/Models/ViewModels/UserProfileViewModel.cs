namespace NGOPlatformWeb.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
