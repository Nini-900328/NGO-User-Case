using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    public class Supply
    {
        [Key]
        public int SupplyId { get; set; }

        [Required]
        public string? SupplyName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SupplyPrice { get; set; }

        [ForeignKey("MaterialCategory")]
        public int MaterialCategoryId { get; set; }

        public int SupplyQuantity { get; set; }

        public string? SupplyType { get; set; }

        public string? ImageUrl { get; set; }

        public string? SupplyDescription { get; set; }

        // 導覽屬性（可選）
        public virtual MaterialCategory? MaterialCategory { get; set; }
    }
}
