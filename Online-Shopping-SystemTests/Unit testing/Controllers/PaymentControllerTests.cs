
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Configuration;
using Online_Shopping_System.Models.Products;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Online_Shopping_System.Tests
{
    public class PaymentControllerTests : IDisposable
    {
        private readonly SqliteContextFixture _db;
        private readonly PaymentController _controller;
        private readonly Mock<EmailService> _emailServiceMock;

        public PaymentControllerTests()
        {
            _db = new SqliteContextFixture(); var httpClient = new HttpClient(); 
            
            var configMock = new Mock<IConfiguration>(); 
            
            _emailServiceMock = new Mock<EmailService>(httpClient, configMock.Object); 
            
            // Prevent the test from actually sending an email.
            _emailServiceMock .Setup(x => x.SendEmailAsync( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())) .Returns(Task.CompletedTask); 
            
            _controller = new PaymentController( null!, _db.Context, _emailServiceMock.Object );
        }

        public void Dispose() => _db.Dispose();

        private async Task<int> SeedUserAsync(
            int userId = 1,
            string role = "Customer")
        {
            var userType = new UserType
            {
                Discount = 0.1
            };

            _db.Context.UserTypes.Add(userType);
            await _db.Context.SaveChangesAsync();

            var user = new User
            {
                UserId = userId,
                UserName = $"testuser{userId}",
                Email = $"testuser{userId}@example.com",
                PasswordHashed = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>(),
                UserRole = role,
                UserTypeId = userType.UserTypeId
            };

            _db.Context.Users.Add(user);
            await _db.Context.SaveChangesAsync();

            return user.UserId;
        }

        // =========================================================
        // CASH PAYMENT
        // =========================================================

        [Fact]
        public async Task CashPay_ReturnsBadRequest_WhenOrderDoesNotExist()
        {
            await SeedUserAsync(userId: 1, role: "Customer");

            _controller.AuthenticateAs(
                userId: 1,
                role: "Customer"
            );

            var result = await _controller.CashPay();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains(
                "Order not found",
                badRequest.Value?.ToString()
            );
        }

        // =========================================================
        // CREDIT CARD PAYMENT
        // =========================================================

        [Fact]
        public async Task CreditCardPay_ReturnsBadRequest_WhenCardIsInvalid()
        {
            await SeedUserAsync(userId: 1, role: "Customer");

            var cart = new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            };

            _db.Context.Carts.Add(cart);
            await _db.Context.SaveChangesAsync();

            var order = new Order
            {
                UserId = 1,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 100
            };

            _db.Context.Orders.Add(order);
            await _db.Context.SaveChangesAsync();

            _controller.AuthenticateAs(
                userId: 1,
                role: "Customer"
            );

            var result = await _controller.CreditCardPay("123");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Credit Card data is incorrect.",
                badRequest.Value
            );
        }

        [Fact]
        public async Task CreditCardPay_ReturnsOk_WhenCardIsValid()
        {
            await SeedUserAsync(userId: 1, role: "User");

            _controller.AuthenticateAs(userId: 1, role: "User");

            var cart = new Cart
            {
                UserId = 1,
                CartStatus = "Pending",
                TotalPrice = 100
            };

            _db.Context.Carts.Add(cart);
            await _db.Context.SaveChangesAsync();

            var order = new Order
            {
                UserId = 1,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 100
            };

            _db.Context.Orders.Add(order);
            await _db.Context.SaveChangesAsync();

            //_controller.AuthenticateAs(
            //    userId: 1,
            //    role: "User"
            //);

            var result = await _controller.CreditCardPay(
                "1234567890123456"
            );

            if (result is ObjectResult obj && obj.StatusCode == 500)
            {
                Assert.Fail(
                    $"Controller returned 500: {obj.Value}"
                );
            }

            Assert.IsType<OkObjectResult>(result);

            var savedOrder = await _db.Context.Orders
                .AsNoTracking()
                .FirstAsync(o => o.OrderId == order.OrderId);

            Assert.Equal(
                "Confirmed",
                savedOrder.OrderStatus
            );
        }

        // =========================================================
        // WALLET PAYMENT
        // =========================================================

        [Fact]
        public async Task WalletPay_ReturnsBadRequest_WhenCartDoesNotExist()
        {
            await SeedUserAsync(userId: 1, role: "Customer");

            _controller.AuthenticateAs(
                userId: 1,
                role: "Customer"
            );

            var result = await _controller.WalletPay(
                "12345678901",
                "Vodafone"
            );

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Order not found.",
                badRequest.Value
            );
        }

        //[Fact]
        //public async Task WalletPay_ReturnsBadRequest_WhenShippingTypeDoesNotExist()
        //{
        //    await SeedUserAsync(userId: 1, role: "Customer");

        //    _db.Context.Carts.Add(new Cart
        //    {
        //        UserId = 1,
        //        CartStatus = "Pending",
        //        TotalPrice = 100
        //    });

        //    await _db.Context.SaveChangesAsync();

        //    _controller.AuthenticateAs(
        //        userId: 1,
        //        role: "Customer"
        //    );

        //    var result = await _controller.WalletPay(
        //        "12345678901",
        //        "Vodafone"
        //    );

        //    var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        //    Assert.Equal(
        //        "Shipping Type not found.",
        //        badRequest.Value
        //    );
        //}

        [Fact]
        public async Task WalletPay_ReturnsBadRequest_WhenWalletIsInvalid()
        {
            await SeedUserAsync(userId: 1, role: "Customer");

            var cart = new Cart { UserId = 1, CartStatus = "Pending", TotalPrice = 100 }; 
            _db.Context.Carts.Add(cart); 
            await _db.Context.SaveChangesAsync();

            var order = new Order { UserId = 1, CartId = cart.CartId, OrderStatus = "Pending", TotalCost = 100 }; 
            
            _db.Context.Orders.Add(order); await _db.Context.SaveChangesAsync();

            _db.Context.ShippingTypes.Add(new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            });

            await _db.Context.SaveChangesAsync();

            _controller.AuthenticateAs(
                userId: 1,
                role: "Customer"
            );

            var result = await _controller.WalletPay(
                "123",
                "Vodafone"
            );

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Wallet data is incorrect.",
                badRequest.Value
            );
        }

        [Fact]
        public async Task WalletPay_ReturnsOk_WhenWalletIsValid()
        {
            await SeedUserAsync(userId: 1, role: "Customer");

            var cart = new Cart { UserId = 1, CartStatus = "Pending", TotalPrice = 100 };

            _db.Context.Carts.Add(cart);
            await _db.Context.SaveChangesAsync();

            var order = new Order { UserId = 1, CartId = cart.CartId, OrderStatus = "Pending", TotalCost = 100 };

            _db.Context.Orders.Add(order); 
            
            await _db.Context.SaveChangesAsync();

            var shippingType = new ShippingType
            {
                ShippingTypeId = 1,
                ShippingCost = 20
            };

            order.TotalCost = cart.TotalPrice + shippingType.ShippingCost;

            _db.Context.ShippingTypes.Add(shippingType);
            await _db.Context.SaveChangesAsync();

            await _db.Context.SaveChangesAsync();

            _controller.AuthenticateAs(
                userId: 1,
                role: "Customer"
            );

            var result = await _controller.WalletPay(
                "01235678901",
                "Vodafone"
            );

            if (result is ObjectResult obj && obj.StatusCode == 500)
            {
                Assert.Fail(
                    $"Controller returned 500: {obj.Value}"
                );
            }

            Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(order);

            Assert.Equal(
                120,
                order.TotalCost
            );
        }
    }
}

