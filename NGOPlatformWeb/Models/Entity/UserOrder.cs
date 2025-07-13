using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    // 使用者訂單主檔 - 儲存捐贈訂單基本資訊
    public class UserOrder
    {
        [Key]
        public int UserOrderId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = "";

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string PaymentStatus { get; set; } = "已確認"; // 跳過付款流程，直接確認訂單

        // 導覽屬性
        public virtual User? User { get; set; }
        public virtual ICollection<UserOrderDetail> OrderDetails { get; set; } = new List<UserOrderDetail>();
    }
}