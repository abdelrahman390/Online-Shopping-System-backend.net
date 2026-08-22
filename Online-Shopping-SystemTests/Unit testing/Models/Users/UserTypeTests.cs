using Xunit;

using Online_Shopping_System.Models.Users;

namespace Online_Shopping_System.Tests.Unit_testing.Models.Users
//namespace Online_Shopping_System.Unit_testing.Models.Payments
{
    public class UserTypeTests
    {
        [Fact]
        public void CalculateTotalPriceAfterDiscount_NormalUser_ReturnsOriginalPrice()
        {
            // Arrange
            var userType = new UserType
            {
                UserTypeName = "Normal",
                Discount = 0
            };

            decimal totalPrice = 1000m;

            // Act
            var result = userType.CalculatTotalPriceAfterDiscount(totalPrice);

            // Assert
            Assert.Equal(1000m, result);
        }


        [Fact]
        public void CalculateTotalPriceAfterDiscount_PremiumUser_AppliesDiscount()
        {
            // Arrange
            var userType = new UserType
            {
                UserTypeName = "Premium",
                Discount = 10
            };

            decimal totalPrice = 1000m;

            // Act
            var result = userType.CalculatTotalPriceAfterDiscount(totalPrice);

            // Assert
            Assert.Equal(900m, result);
        }


        [Fact]
        public void CalculateTotalPriceAfterDiscount_VipUser_AppliesDiscount()
        {
            // Arrange
            var userType = new UserType
            {
                UserTypeName = "VIP",
                Discount = 20
            };

            decimal totalPrice = 1000m;

            // Act
            var result = userType.CalculatTotalPriceAfterDiscount(totalPrice);

            // Assert
            Assert.Equal(800m, result);
        }


        [Fact]
        public void CalculateTotalPriceAfterDiscount_ZeroPrice_ReturnsZero()
        {
            // Arrange
            var userType = new UserType
            {
                Discount = 20
            };

            decimal totalPrice = 0m;

            // Act
            var result = userType.CalculatTotalPriceAfterDiscount(totalPrice);

            // Assert
            Assert.Equal(0m, result);
        }


        [Fact]
        public void CalculateTotalPriceAfterDiscount_50PercentDiscount_ReturnsHalfPrice()
        {
            // Arrange
            var userType = new UserType
            {
                Discount = 50
            };

            decimal totalPrice = 500m;

            // Act
            var result = userType.CalculatTotalPriceAfterDiscount(totalPrice);

            // Assert
            Assert.Equal(250m, result);
        }
    }
}

