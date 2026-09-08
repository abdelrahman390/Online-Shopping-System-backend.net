using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Online_Shopping_System.Integration
{
    public class OrderTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;

    private HttpClient _client = default!;
        private int _userId;

        public OrderTests(ShoppingWebAppFactory factory)
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
        // CONFIRM ORDER
        // =========================================================

        [Fact]
        public async Task ConfirmOrder_WithValidCartAndShippingType_CreatesOrderAndShippingRecord()
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

            var shippingType = new ShippingType
            {
                ShippingName = "Standard",
                ShippingCost = 50m
            };

            db.Carts.Add(cart);
            db.ShippingTypes.Add(shippingType);

            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                $"/Order/confirmOrder?shippingTypeId={shippingType.ShippingTypeId}",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await db.Orders
                .FirstOrDefaultAsync(o =>
                    o.UserId == _userId &&
                    o.CartId == cart.CartId);

            order.Should().NotBeNull();

            order!.OrderStatus.Should().Be("Pending");
            order.TotalCost.Should().Be(550m);

            var shippingRecord = await db.ShippingRecords
                .FirstOrDefaultAsync(s =>
                    s.OrderId == order.OrderId);

            shippingRecord.Should().NotBeNull();
            shippingRecord!.ShippingTypeId
                .Should().Be(shippingType.ShippingTypeId);
        }

        [Fact]
        public async Task ConfirmOrder_WithoutCart_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var shippingType = new ShippingType
            {
                ShippingName = "Standard",
                ShippingCost = 50m
            };

            db.ShippingTypes.Add(shippingType);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                $"/Order/confirmOrder?shippingTypeId={shippingType.ShippingTypeId}",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ConfirmOrder_WithInvalidShippingType_ReturnsBadRequest()
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

            // Act
            var response = await _client.PostAsync(
                "/Order/confirmOrder?shippingTypeId=99999",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ConfirmOrder_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsync(
                "/Order/confirmOrder?shippingTypeId=1",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // GET ORDERS
        // =========================================================

        [Fact]
        public async Task GetOrders_ReturnsOnlyCurrentUserOrders()
        {
            // Arrange
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

            var otherUser = new User
            {
                UserTypeId = db.UserTypes.FirstOrDefault().UserTypeId,
                UserName = "OtherUser",
                Email = "other@example.com",
                PasswordHashed = [],
                PasswordSalt = [],
                UserRole = "User"
            };

            db.Users.Add(otherUser);
            await db.SaveChangesAsync();

            db.Carts.AddRange(
            new Cart
            {
                UserId = _userId,
                TotalPrice = 100m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            },
            new Cart
            {
                UserId = otherUser.UserId,
                TotalPrice = 200m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            });

            await db.SaveChangesAsync();

            db.Orders.AddRange(
                new Order
                {
                    UserId = _userId,
                    OrderStatus = "Pending",
                    TotalCost = 100m,
                    CreatedAt = DateTimeOffset.Now,
                    CartId = db.Carts.FirstOrDefault(c => c.UserId == _userId).CartId
                },
                new Order
                {
                    UserId = otherUser.UserId,
                    OrderStatus = "Confirmed",
                    TotalCost = 200m,
                    CreatedAt = DateTimeOffset.Now,
                    CartId = db.Carts.FirstOrDefault(c => c.UserId == otherUser.UserId).CartId
                });

            await db.SaveChangesAsync();

            var token = TestAuthHelper.GenerateToken(
            _userId.ToString(),
            "User");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                "/Order/getOrders");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Contain("100");
            body.Should().NotContain("200");
        }

        [Fact]
        public async Task GetOrders_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync(
                "/Order/getOrders");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // GET USER UNPAID ORDER
        // =========================================================

        [Fact]
        public async Task GetUserUnpaidOrder_WithPendingOrder_ReturnsOrder()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var Cart = new Cart 
            {
                UserId = _userId,
                TotalPrice = 500m,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            };
            db.Carts.Add(Cart);
            await db.SaveChangesAsync();

            var order = new Order
            {
                CartId = Cart.CartId,
                UserId = _userId,
                OrderStatus = "Pending",
                TotalCost = 500m,
                CreatedAt = DateTimeOffset.Now
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync(
                "/Order/getUserUnpaidOrder");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Contain("500");
            body.Should().Contain("Pending");
        }

        [Fact]
        public async Task GetUserUnpaidOrder_WithoutPendingOrder_ReturnsNull()
        {
            // Act
            var response = await _client.GetAsync(
                "/Order/getUserUnpaidOrder");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            //var body = await response.Content.ReadAsStringAsync();

            //body.Should().Be("null");
        }

        [Fact]
        public async Task GetUserUnpaidOrder_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync(
                "/Order/getUserUnpaidOrder");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

}
