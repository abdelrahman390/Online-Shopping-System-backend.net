using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;

namespace Online_Shopping_System.Models.Shipping
{
    public class ShippingRecords
    {
        public int ShippingRecordsId { get; set; }
        public int OrderId { get; set; }
        public int ShippingTypeId { get; set; }
        public string ShippingAddress { get; set; } = "Test Adress";
        public DateTimeOffset DeleveredTime { get; set; }

        //public double Weight { get; set; }

        public Order Order { get; set; }
        public ShippingType ShippingType { get; set; }
    }


}
