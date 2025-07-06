using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class Case
    {
        [Key]
        public int CaseId { get; set; }
        public string ?Name { get; set; }
        public string ?Phone { get; set; }
        public string ?IdentityNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string ?Address { get; set; }
        public int WorkerId { get; set; }   // 若不用，可設成 nullable
        public string ?Description { get; set; }
        public DateTime ?CreatedAt { get; set; }
        public string ?Status { get; set; }

        // 導航屬性
        public virtual CaseLogin ?CaseLogin { get; set; }
    }
}
