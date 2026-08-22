using Online_Shopping_System.Models.Users;
using Online_Shopping_System.Models.Orders;


namespace Online_Shopping_System.Models.Carts
{
    public class Cart
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        // status: Pending - Confirmed - Closed
        public string CartStatus { get; set; }
        //public List<CartItem> CartItems { get; set; }

        // Navigation property
        public User User { get; set; }

        // Navigation property
        public Order Order { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}
