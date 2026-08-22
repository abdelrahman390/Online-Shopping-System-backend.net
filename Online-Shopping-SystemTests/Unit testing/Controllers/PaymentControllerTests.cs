
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Shipping;
using Xunit;

namespace Online_Shopping_SystemTests.Controllers
{
    public class PaymentControllerTests
    {
        private OnlineShoppingContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OnlineShoppingContext(options);
        }

        private PaymentController CreateController(OnlineShoppingContext context)
        {
            return new PaymentController(null!, context);
        }


        [Fact]
        public void CashPay_CartDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);

            // Act
            var result = controller.cashPay(1, 1);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains("Cart not found", badRequest.Value?.ToString());
        }


        [Fact]
        public void CashPay_ShippingTypeDoesNotExist_BadRequest()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.cashPay(1, 999);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Shipping Type not found.",
                badRequest.Value
            );
        }


        [Fact]
        public void CashPay_ValidCartAndShipping_CreatesOrder()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.cashPay(1, 1);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var order = context.Orders.FirstOrDefault();

            Assert.NotNull(order);
            Assert.Equal(1, order.UserId);
            Assert.Equal(120, order.TotalCost);
            Assert.Equal("InProgress", order.OrderStatus);
        }


        [Fact]
        public void CashPay_ValidPayment_ConfirmsCart()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            controller.cashPay(1, 1);

            // Assert
            var cart = context.Carts.First();

            Assert.Equal("Confirmed", cart.CartStatus);
        }


        [Fact]
        public void CashPay_ValidPayment_CreatesPayment()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            controller.cashPay(1, 1);

            // Assert
            var payment = context.Payments.FirstOrDefault();

            Assert.NotNull(payment);
            Assert.Equal(120, payment.Amount);
        }


        // =========================
        // CREDIT CARD PAYMENT
        // =========================

        [Fact]
        public void CreditCardPay_CartDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);

            // Act
            var result = controller.CreditCardPay(
                1,
                1,
                "1234567890123456"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Cart not found.", badRequest.Value);
        }


        [Fact]
        public void CreditCardPay_ShippingTypeDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.CreditCardPay(
                1,
                999,
                "1234567890123456"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Shipping Type not found.",
                badRequest.Value
            );
        }


        [Fact]
        public void CreditCardPay_InvalidCard_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.CreditCardPay(
                1,
                1,
                "123"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Credit Card data is incorrect.",
                badRequest.Value
            );
        }


        [Fact]
        public void CreditCardPay_ValidCard_CreatesOrder()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.CreditCardPay(
                1,
                1,
                "1234567890123456"
            );

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var order = context.Orders.FirstOrDefault();

            Assert.NotNull(order);
            Assert.Equal(120, order.TotalCost);
        }


        // =========================
        // WALLET PAYMENT
        // =========================

        [Fact]
        public void WalletPay_CartDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();

            var controller = CreateController(context);

            // Act
            var result = controller.WalletPay(
                1,
                1,
                "12345678901",
                "Vodafone"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("Cart not found.", badRequest.Value);
        }


        [Fact]
        public void WalletPay_ShippingTypeDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.WalletPay(
                1,
                999,
                "12345678901",
                "Vodafone"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Shipping Type not found.",
                badRequest.Value
            );
        }


        [Fact]
        public void WalletPay_InvalidWallet_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.WalletPay(
                1,
                1,
                "123",
                "Vodafone"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Wallet data is incorrect.",
                badRequest.Value
            );
        }


        [Fact]
        public void WalletPay_ValidWallet_CreatesOrder()
        {
            // Arrange
            using var context = CreateContext();

            context.Carts.Add(new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            });

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.WalletPay(
                1,
                1,
                "012345678901",
                "Vodafone"
            );

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var order = context.Orders.FirstOrDefault();

            Assert.NotNull(order);
            Assert.Equal(120, order.TotalCost);
        }
    }
}