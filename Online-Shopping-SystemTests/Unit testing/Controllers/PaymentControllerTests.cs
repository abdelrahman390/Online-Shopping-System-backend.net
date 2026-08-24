
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;

namespace Online_Shopping_SystemTests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly EmailService _emailService;
        private (OnlineShoppingContext Context, SqliteConnection Connection) CreateContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseSqlite(connection)
                .Options;

            var context = new OnlineShoppingContext(options);

            context.Database.EnsureCreated();

            return (context, connection);
        }

        private (User User, PaymentController Controller)
    CreateTestUserAndController(OnlineShoppingContext context)
        {
            // Create UserType
            var userType = new UserType
            {
                Discount = 0.1
            };

            context.UserTypes.Add(userType);
            context.SaveChanges();

            // Create User
            var user = new User
            {
                UserTypeId = userType.UserTypeId,
                UserName = "TestUser",
                Email = "test@example.com",
                PasswordHashed = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>()
            };

            context.Users.Add(user);
            context.SaveChanges();

            // Create controller
            var controller = new PaymentController(
                null!,
                context,
                _emailService
            );

            // Create claims
            var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.UserId.ToString()
        ),

        new Claim(
            ClaimTypes.Role,
            "Customer"
        )
    };

            // Create authenticated identity
            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "TestAuthentication"
            );

            var principal = new ClaimsPrincipal(identity);

            // Create HttpContext
            var httpContext = new DefaultHttpContext();

            httpContext.User = principal;

            httpContext.Connection.RemoteIpAddress =
                IPAddress.Parse("127.0.0.1");

            // Attach HttpContext to controller
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return (user, controller);
        }
        //private User CreateTestUser(OnlineShoppingContext context, string role = "User")
        //{
        //    // Create UserType
        //    var userType = new UserType
        //    {
        //        Discount = 0.1
        //    };

        //    context.UserTypes.Add(userType);
        //    context.SaveChanges();

        //    // Create User
        //    var user = new User
        //    {
        //        UserTypeId = userType.UserTypeId,
        //        UserName = "TestUser",
        //        Email = "test@example.com",
        //        PasswordHashed = Array.Empty<byte>(),
        //        PasswordSalt = Array.Empty<byte>()
        //    };

        //    context.Users.Add(user);
        //    context.SaveChanges();

        //    return user;
        //}

        //private OnlineShoppingContext CreateContext()
        //{
        //    //var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
        //    //    .UseInMemoryDatabase(Guid.NewGuid().ToString())
        //    //    .Options;
        //    var connection = new SqliteConnection("DataSource=:memory:");
        //    connection.Open();

        //    var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
        //        .UseSqlite(connection)
        //        .Options;

        //    var context = new OnlineShoppingContext(options);

        //    context.Database.EnsureCreated();

        //    return context;

        //    //return new OnlineShoppingContext(options);
        //}

        private PaymentController CreateController(
            OnlineShoppingContext context,
            User user,
            string role = "Customer")
        {
            var controller = new PaymentController(
                null!,
                context,
                _emailService
            );

            var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.UserId.ToString()
        ),

        new Claim(
            ClaimTypes.Role,
            role
        )
    };

            var identity = new ClaimsIdentity(
                claims,
                "TestAuthentication"
            );

            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            controller.HttpContext.Connection.RemoteIpAddress =
                IPAddress.Parse("127.0.0.1");

            return controller;
        }


        [Fact]
        public async Task CashPay_OrderDoesNotExist_ReturnsBadRequest()
        {
            var setup = CreateContext();

            using var context = setup.Context;
            using var connection = setup.Connection;

            // Arrange
            //using var context = CreateContext();
            var controller = CreateController(context);

            // Act
            var result = await controller.CashPay();

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
        public async Task CreditCardPay_ValidCard_CreatesOrder_ReturnOk()
        {
            // Arrange
            var setup = CreateContext();

            using var context = setup.Context;
            using var connection = setup.Connection;

            // Create test user + claims
            //var user = CreateTestUser(context);
            var test = CreateTestUserAndController(context);

            var user = test.User;
            var controller = test.Controller;

            // Create cart
            var cart = new Cart
            {
                UserId = user.UserId,
                CartStatus = "Pending",
                TotalPrice = 100
            };

            context.Carts.Add(cart);
            context.SaveChanges();

            // Create order
            var order = new Order
            {
                UserId = user.UserId,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 100
            };

            context.Orders.Add(order);
            context.SaveChanges();

            // Create controller with authenticated user
            var controller = CreateController(
                context,
                user
            );

            // Act
            var result = await controller.CreditCardPay(
                "1234567890123456"
            );

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var savedOrder = context.Orders.First();

            Assert.Equal("Confirmed", savedOrder.OrderStatus);
        }

        //[Fact]
        //public async Task CashPay_ValidPayment_PayAndConfirmsOrder()
        //{
        //    // Arrange
        //    using var context = CreateContext();

        //    context.UserTypes.Add(new UserType
        //    {
        //        Discount = 0.1,
        //    });

        //    context.Users.Add(new User
        //    {
        //        UserTypeId = 1,
        //        UserName = "Abdelrahman",
        //        Email = "test@gmail.com",
        //        PasswordHashed = Array.Empty<byte>()
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
        //    var result = await controller.CashPay();

        //    // Assert
        //    var order = context.Orders.First();
        //    var cart = context.Carts.FirstOrDefault();
        //    var cashPay = context.CashPayments.FirstOrDefault();

        //    Assert.Equal("Confirmed", order.OrderStatus);
        //    Assert.Equal("Confirmed", cart.CartStatus);
        //    Assert.Equal(order.OrderId, cashPay.OrderId);
        //}


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
        //        PasswordHashed = Array.Empty<byte>()
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
                PasswordHashed = Array.Empty<byte>()
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
                "123"
            );

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Credit Card data is incorrect.",
                badRequest.Value
            );
        }


        //[Fact]
        //public async Task CreditCardPay_ValidCard_CreatesOrder_ReturnOk()
        //{
        //    // Arrange
        //    using var context = CreateContext();

        //    context.UserTypes.Add(new UserType
        //    {
        //        Discount = 0.1,

        //    });
        //    context.SaveChanges();

        //    context.Users.Add(new User
        //    {
        //        UserTypeId = context.UserTypes.First().UserTypeId,
        //        UserName = "Abdelrahman",
        //        Email = "test@gmail.com",
        //        PasswordHashed = Array.Empty<byte>(),
        //        PasswordSalt = Array.Empty<byte>()
        //    });
        //    context.SaveChanges();

        //    context.Carts.Add(new Cart
        //    {
        //        UserId = context.Users.First().UserId,
        //        CartStatus = "Pending",
        //        TotalPrice = 100
        //    });
        //    context.SaveChanges();

        //        context.Orders.Add(new Order
        //    {                                   
        //        UserId = context.Users.First().UserId,
        //        CartId = context.Carts.First().CartId,
        //        OrderStatus = "Pending",
        //        TotalCost = 100
        //    });

        //    context.SaveChanges();

        //    var controller = CreateController(context);

        //    // Act
        //    var result = await controller.CreditCardPay(
        //        "1234567890123456"
        //    );

        //    //if (result is ObjectResult objectResult)
        //    //{
        //    //    Console.WriteLine($"Status Code: {objectResult.StatusCode}");
        //    //    Console.WriteLine($"Error: {objectResult.Value}");
        //    //}

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);

        //    var order = context.Orders.FirstOrDefault();

        //    //Assert.NotNull(order.);
        //    Assert.Equal("Confirmed", order.OrderStatus);
        //}


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