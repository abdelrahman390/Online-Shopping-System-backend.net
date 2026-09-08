using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Payment;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Online_Shopping_System.Integration
{
    public class PaymentTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;

    private HttpClient _client = default!;
        private int _userId;

        public PaymentTests(ShoppingWebAppFactory factory)
        {
            _factory = factory;
            _dbReset = new DatabaseResetFixture(factory);
        }

        public async Task InitializeAsync()
        {
            await _dbReset.InitializeAsync();
            await _dbReset.ResetAsync();

            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var userType = new UserType
            {
                UserTypeName = "Normal",
                Discount = 0
            };

            db.UserTypes.Add(userType);
            await db.SaveChangesAsync();

            var user = new User
            {
                UserTypeId = userType.UserTypeId,
                UserName = "TestUser",
                Email = "test@example.com",
                PasswordHashed = [],
                PasswordSalt = [],
                UserRole = "User"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            _userId = user.UserId;

            var token = TestAuthHelper.GenerateToken(
                _userId.ToString(),
                "User");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public Task DisposeAsync()
            => _dbReset.DisposeAsync();

        // =========================================================
        // CASH PAYMENT
        // =========================================================

        [Fact]
        public async Task CashPay_WithPendingOrder_ConfirmsOrderAndCreatesPayment()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var cart = new Cart
            {
                UserId = _userId,
                TotalPrice = 500m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            };

            db.Carts.Add(cart);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = _userId,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 500m,
                CreatedAt = DateTimeOffset.Now
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Payment/cashPay",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedOrder = await db.Orders
                .AsNoTracking()
                .FirstAsync(o => o.OrderId == order.OrderId);

            updatedOrder.OrderStatus.Should().Be("Confirmed");
            updatedOrder.PaymentTypeName.Should().Be("Cash");

            var updatedCart = await db.Carts
                .AsNoTracking()
                .FirstAsync(c => c.CartId == cart.CartId);

            updatedCart.CartStatus.Should().Be("Confirmed");

            var payment = await db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.OrderId);

            payment.Should().NotBeNull();
            payment!.Amount.Should().Be(500m);
        }

        [Fact]
        public async Task CashPay_WithoutPendingOrder_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync(
                "/Payment/cashPay",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CashPay_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsync(
                "/Payment/cashPay",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // CREDIT CARD PAYMENT
        // =========================================================

        [Fact]
        public async Task CreditCardPay_WithValidCard_ConfirmsOrderAndCreatesPayment()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            // Ensure user is authenticated (add this if missing)
            var token = TestAuthHelper.GenerateToken(_userId.ToString(), "User");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var cart = new Cart
            {
                UserId = _userId,
                TotalPrice = 500m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            };

            db.Carts.Add(cart);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = _userId,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 500m,
                CreatedAt = DateTimeOffset.Now
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Payment/creditCardPay?cardNumber=1234567890123456",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedOrder = await db.Orders
                .AsNoTracking()
                .FirstAsync(o => o.OrderId == order.OrderId);

            updatedOrder.OrderStatus.Should().Be("Confirmed");
            updatedOrder.PaymentTypeName.Should().Be("CreditCard");

            var payment = await db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.OrderId);

            payment.Should().NotBeNull();

            var creditCardPayment = payment
                .Should()
                .BeOfType<CreditCardPayment>()
                .Subject;

            creditCardPayment.CardNumber
                .Should().Be("1234567890123456");
        }

        [Fact]
        public async Task CreditCardPay_WithInvalidCard_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var cart = new Cart
            {
                UserId = _userId,
                TotalPrice = 500m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            };

            db.Carts.Add(cart);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = _userId,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 500m,
                CreatedAt = DateTimeOffset.Now
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Payment/creditCardPay?cardNumber=123",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.BadRequest);

            var unchangedOrder = await db.Orders
                .AsNoTracking()
                .FirstAsync(o => o.OrderId == order.OrderId);

            unchangedOrder.OrderStatus.Should().Be("Pending");
        }

        // =========================================================
        // WALLET PAYMENT
        // =========================================================

        [Fact]
        public async Task WalletPay_WithValidWallet_ConfirmsOrderAndCreatesPayment()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var cart = new Cart
            {
                UserId = _userId,
                TotalPrice = 500m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            };

            db.Carts.Add(cart);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = _userId,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 500m,
                CreatedAt = DateTimeOffset.Now
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Payment/WalletPay?WalletNumber=01012345678&WalletProviderName=Vodafone",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedOrder = await db.Orders
                .AsNoTracking()
                .FirstAsync(o => o.OrderId == order.OrderId);

            updatedOrder.OrderStatus.Should().Be("Confirmed");
            updatedOrder.PaymentTypeName.Should().Be("Wallet");

            var payment = await db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.OrderId);

            payment.Should().NotBeNull();

            var walletPayment = payment
                .Should()
                .BeOfType<WalletPayment>()
                .Subject;

            walletPayment.WalletNumber
                .Should().Be("01012345678");

            walletPayment.WalletProviderName
                .Should().Be("Vodafone");
        }

        [Fact]
        public async Task WalletPay_WithInvalidWallet_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var cart = new Cart
            {
                UserId = _userId,
                TotalPrice = 500m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            };

            db.Carts.Add(cart);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = _userId,
                CartId = cart.CartId,
                OrderStatus = "Pending",
                TotalCost = 500m,
                CreatedAt = DateTimeOffset.Now
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Payment/WalletPay?WalletNumber=123&WalletProviderName=Unknown",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.BadRequest);

            var unchangedOrder = await db.Orders
                .AsNoTracking()
                .FirstAsync(o => o.OrderId == order.OrderId);

            unchangedOrder.OrderStatus.Should().Be("Pending");
        }

        [Fact]
        public async Task WalletPay_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsync(
                "/Payment/WalletPay?WalletNumber=01012345678&WalletProviderName=Vodafone",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

}
