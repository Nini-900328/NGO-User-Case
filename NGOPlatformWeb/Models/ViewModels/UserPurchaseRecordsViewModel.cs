using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels
{
    /// <summary>
    /// 使用者認購紀錄的主要ViewModel
    /// </summary>
    public class UserPurchaseRecordsViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<OrderRecordViewModel> Orders { get; set; } = new List<OrderRecordViewModel>();
        public int TotalOrders => Orders.Count;
        public decimal TotalDonated => Orders.Sum(o => o.TotalPrice);
    }

    /// <summary>
    /// 單筆訂單記錄的ViewModel
    /// </summary>
    public class OrderRecordViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        
        // 輔助屬性
        public string FormattedOrderDate => OrderDate.ToString("yyyy/MM/dd HH:mm");
        public string FormattedTotalPrice => $"NT$ {TotalPrice:N0}";
        public int TotalItems => Items.Sum(i => i.Quantity);
        public bool IsEmergencyOrder => Items.Any(i => i.IsEmergency);
    }

    /// <summary>
    /// 訂單明細項目的ViewModel
    /// </summary>
    public class OrderItemViewModel
    {
        public string SupplyName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsEmergency { get; set; } = false;
        public int? CaseId { get; set; }

        // 輔助屬性
        public string FormattedUnitPrice => $"NT$ {UnitPrice:N0}";
        public string FormattedTotalPrice => $"NT$ {TotalPrice:N0}";
        public string QuantityText => $"{Quantity} 份";
    }
}