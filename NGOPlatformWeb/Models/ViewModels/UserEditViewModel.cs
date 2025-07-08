using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class UserEditViewModel
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string? Phone { get; set; }

        [Required]
        public string? IdentityNumber { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
