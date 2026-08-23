
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using Xunit;

namespace Online_Shopping_SystemTests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly EmailService _emailService;
        private OnlineShoppingContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OnlineShoppingContext(options);
        }

        private PaymentController CreateController(OnlineShoppingContext context)
        {
            return new PaymentController(null!, context, _emailService);
        }


        [Fact]
        public async Task CashPay_OrderDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);

            // Act
            var result = await controller.CashPay(1);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains("Order not found", badRequest.Value?.ToString());
        }


        //[Fact]
        //public async Task CashPay_ShippingTypeDoesNotExist_BadRequest()
        //{
        //    // Arrange
        //    using var context = CreateContext();

        //    context.Carts.Add(new Cart
        //    {
        //        UserId = 1,
        //        CartStatus = "Pending",
        //        TotalPrice = 100
        //    });

        //    context.SaveChanges();

        //    var controller = CreateController(context);

        //    // Act
        //    var result = await controller.CashPay(1);

        //    // Assert
        //    var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        //    Assert.Equal(
        //        "Shipping Type not found.",
        //        badRequest.Value
        //    );
        //}


        //[Fact]
        //public void CashPay_ValidCartAndShipping_CreatesOrder()
        //{
        //    // Arrange
        //    using var context = CreateContext();

        //    context.Carts.Add(new Cart
        //    {
        //        UserId = 1,
        //        CartStatus = "Pending",
        //        TotalPrice = 100
        //    });

        //    context.ShippingTypes.Add(new ShippingType
        //    {
        //        ShippingTypeId = 1,
        //        ShippingCost = 20
        //    });

        //    context.SaveChanges();

        //    var controller = CreateController(context);

        //    // Act
        //    var result = controller.CashPay(1);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);

        //    var order = context.Orders.FirstOrDefault();

        //    Assert.NotNull(order);
        //    Assert.Equal(1, order.UserId);
        //    Assert.Equal(120, order.TotalCost);
        //    Assert.Equal("InProgress", order.OrderStatus);
        //}


        [Fact]
        public async Task CashPay_ValidPayment_PayAndConfirmsOrder()
        {
            // Arrange
            using var context = CreateContext();

            context.Users.Add(new User
            {
                UserTypeId = 1,
                UserName = "Abdelrahman",
                Email = "test@gmail.com",
                PasswordHashed = "ddfgdns55dffw"
            });
            context.SaveChanges();

            context.Carts.Add(new Cart
            {
                UserId = context.Users.First().UserId,
                CartStatus = "Pending",
                TotalPrice = 100
            });
            context.SaveChanges();

            context.Orders.Add(new Order
            {
                UserId = 1,
                CartId = context.Carts.First().CartId,
                OrderStatus = "Pending",
                TotalCost = 100
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = await controller.CashPay(1);

            // Assert
            var order = context.Orders.First();
            var cart = context.Carts.FirstOrDefault();
            var cashPay = context.CashPayments.FirstOrDefault();

            Assert.Equal("Confirmed", order.OrderStatus);
            Assert.Equal("Confirmed", cart.CartStatus);
            Assert.Equal(order.OrderId, cashPay.OrderId);
        }


        //[Fact]
        //public void CashPay_ValidPayment_CreatesPayment()
        //{
        //    // Arrange
        //    using var context = CreateContext();


        //    context.Users.Add(new User
        //    {
        //        UserTypeId = 1,
        //        UserName = "Abdelrahman",
        //        Email = "test@gmail.com",
        //        PasswordHashed = "ddfgdns55dffw"
        //    });
        //    context.SaveChanges();

        //    context.Carts.Add(new Cart
        //    {
        //        UserId = context.Users.First().UserId,
        //        CartStatus = "Pending",
        //        TotalPrice = 100
        //    });
        //    context.SaveChanges();

        //    context.Orders.Add(new Order
        //    {
        //        UserId = 1,
        //        CartId = context.Carts.First().CartId,
        //        OrderStatus = "Pending",
        //        TotalCost = 100
        //    });

        //    context.SaveChanges();

        //    var controller = CreateController(context);

        //    // Act
        //    controller.CashPay(1);

        //    // Assert
        //    var payment = context.Payments.FirstOrDefault();

        //    Assert.NotNull(payment);
        //    Assert.Equal(120, payment.Amount);
        //}


        // =========================
        // CREDIT CARD PAYMENT
        // =========================

        //[Fact]
        //public async Task CreditCardPay_OrderDoesNotExist_ReturnsBadRequest()
        //{
        //    // Arrange
        //    using var context = CreateContext();
        //    var controller = CreateController(context);

        //    // Act
        //    var result = await controller.CreditCardPay(
        //        1,
        //        "1234567890123456"
        //    );

        //    // Assert
        //    var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        //    Assert.Equal("Order not found.", badRequest.Value);
        //}


        //[Fact]
        //public void CreditCardPay_ShippingTypeDoesNotExist_ReturnsBadRequest()
        //{
        //    // Arrange
        //    using var context = CreateContext();

        //    context.Carts.Add(new Cart
        //    {
        //        UserId = 1,
        //        CartStatus = "Pending",
        //        TotalPrice = 100
        //    });

        //    context.SaveChanges();

        //    var controller = CreateController(context);

        //    // Act
        //    var result = controller.CreditCardPay(
        //        1,
        //        "1234567890123456"
        //    );

        //    // Assert
        //    var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        //    Assert.Equal(
        //        "Shipping Type not found.",
        //        badRequest.Value
        //    );
        //}


        [Fact]
        public async Task CreditCardPay_InvalidCard_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();

            context.Users.Add(new User
            {
                UserTypeId = 1,
                UserName = "Abdelrahman",
                Email = "test@gmail.com",
                PasswordHashed = "ddfgdns55dffw"
            });
            context.SaveChanges();

            context.Carts.Add(new Cart
            {
                UserId = context.Users.First().UserId,
                CartStatus = "Pending",
                TotalPrice = 100
            });
            context.SaveChanges();

            context.Orders.Add(new Order
            {
                UserId = 1,
                CartId = context.Carts.First().CartId,
                OrderStatus = "Pending",
                TotalCost = 100
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = await controller.CreditCardPay(
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
        public async Task CreditCardPay_ValidCard_CreatesOrder_ReturnOk()
        {
            // Arrange
            using var context = CreateContext();

            context.Users.Add(new User
            {
                UserTypeId = 1,
                UserName = "Abdelrahman",
                Email = "test@gmail.com",
                PasswordHashed = "ddfgdns55dffw"
            });
            context.SaveChanges();

            context.Carts.Add(new Cart
            {
                UserId = context.Users.First().UserId,
                CartStatus = "Pending",
                TotalPrice = 100
            });
            context.SaveChanges();

            context.Orders.Add(new Order
            {
                UserId = 1,
                CartId = context.Carts.First().CartId,
                OrderStatus = "Pending",
                TotalCost = 100
            });

            context.SaveChanges();

            var controller = CreateController(context);

            // Act
            var result = controller.CreditCardPay(
                1,
                "1234567890123456"
            );

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var order = context.Orders.FirstOrDefault();

            //Assert.NotNull(order.);
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