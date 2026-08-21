using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Users;

namespace Online_Shopping_System.Models.Carts
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int CartId { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }

        // Navigation property
        public Product Product { get; set; }

        // Navigation property
        public Cart Cart { get; set; }
    }
}
