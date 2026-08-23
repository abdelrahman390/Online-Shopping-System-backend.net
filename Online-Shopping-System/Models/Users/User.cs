using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;

namespace Online_Shopping_System.Models.Users
{
    public class User
    {
        public int UserId { get; set; }
        public int UserTypeId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHashed { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string UserRole { get; set; } = "User";

        // One User can have many Carts
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();

        // One User can have many Orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();


        // Navigation property
        public UserType UserType { get; set; }

    }
}
