using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class EmergencySupplyNeeds
    {
        [Key] // 這行很重要
        public int EmergencyNeedId { get; set; }
        public int CaseId { get; set; }
        public int SupplyId { get; set; }
        public int WorkerId { get; set; }
        public int Quantity { get; set; }
        public DateTime VisitDate { get; set; }
        public string Status { get; set; }
        public DateTime? PickupDate { get; set; }
        public virtual Supply Supply { get; set; }  // 連結 Supply 表
    }
}
