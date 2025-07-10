using System.Collections.Generic;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class SupplyRecordViewModel
    {
        public List<SupplyRecordItem> UnreceivedSupplies { get; set; }
        public List<SupplyRecordItem> ReceivedSupplies { get; set; }
        public List<SupplyRecordItem> EmergencySupplies { get; set; }

    }
}