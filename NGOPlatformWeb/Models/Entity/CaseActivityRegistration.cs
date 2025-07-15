using System;
using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class CaseActivityRegistration
    {
        [Key]
        public int RegistrationId { get; set; }
        
        public int CaseId { get; set; }
        
        public int ActivityId { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "registered"; // registered, cancelled
        
        public DateTime RegisterTime { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Case Case { get; set; }
        public virtual Activity Activity { get; set; }
    }
}