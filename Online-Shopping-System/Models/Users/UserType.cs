using Online_Shopping_System.Models.Carts;

namespace Online_Shopping_System.Models.Users
{
    public class UserType
    {
        public int UserTypeId { get; set; }
        // UserTypes: Normal - Premium - VIP
        public string UserTypeName { get; set; } = "Normal";
        public double Discount { get; set; } = 0.0; // in percentage

        // One UserType has many Users
        public ICollection<User> Users { get; set; } = new List<User>();

        public decimal CalculatTotalPriceAfterDiscount(decimal totalPrice)
        {
            return totalPrice - (totalPrice * (decimal)(Discount / 100.0));
        }
    }
}
