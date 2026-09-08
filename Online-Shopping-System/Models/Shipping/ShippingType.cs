namespace Online_Shopping_System.Models.Shipping
{
    public class ShippingType
    {
        public int ShippingTypeId { get; set; }
        // ShippingTypes: Standard - Express - SameDay
        public string ShippingName { get; set; } = "Standard";
        public decimal ShippingCost { get; set; }
        public int ShippingDuration { get; set; } // in days

        public ICollection<ShippingRecords> ShippingRecords { get; set; }
    = new List<ShippingRecords>();

    }
}
