using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class CaseActivityRegistrations
    {
        [Key]
        public int RegistrationId { get; set; }

        public int CaseId { get; set; }
        public int ActivityId { get; set; }
        public string Status { get; set; } = "pending";
    }
}
