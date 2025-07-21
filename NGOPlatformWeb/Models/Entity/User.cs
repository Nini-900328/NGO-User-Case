using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string ?IdentityNumber { get; set; }

        [Required]
        [EmailAddress]
        public string ?Email { get; set; }

        [Required]
        [StringLength(60)]
        public string ?Password { get; set; }

        [StringLength(20)]
        public string ?Phone { get; set; }

        [StringLength(50)]
        public string ?Name { get; set; }
        
        [StringLength(500)]
        public string ?ProfileImage { get; set; }
    }
}
