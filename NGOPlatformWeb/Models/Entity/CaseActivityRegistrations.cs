using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class CaseActivityRegistration
    {
        [Key]
        public int RegistrationId { get; set; }
        public int CaseId { get; set; }
        public int ActivityId { get; set; }
        public string Status { get; set; } = "registered";

        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int ParticipantCount { get; set; }
    }
}