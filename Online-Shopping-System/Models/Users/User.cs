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
        public string PasswordHashed { get; set; }

        // Navigation property
        public Cart Cart { get; set; }

        // Navigation property
        public UserType UserType { get; set; }

        public ICollection<Order> Order { get; set; } = new List<Order>();
    }
}
