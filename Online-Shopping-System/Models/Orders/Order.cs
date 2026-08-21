using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Users;
using Online_Shopping_System.Models.Shipping;


namespace Online_Shopping_System.Models.Orders
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CartId { get; set; }
        public int UserId { get; set; }

        // Status: Delivered - InProgress
        public string OrderStatus { get; set; }

        // PaymentTypeNames: Cash - CreditCard - Wallet
        public string PaymentTypeName { get; set; } = "Cash";
        public double TotalCost { get; set; }

        // Navigation property
        public Cart Cart { get; set; }
        //public ICollection<Cart> Cart { get; set; } = new List<Cart>();

        // Navigation property
        public User User { get; set; }
        //public ICollection<User> Users { get; set; } = new List<User>();

        public ShippingRecords ShippingRecords { get; set; }
    }
}
