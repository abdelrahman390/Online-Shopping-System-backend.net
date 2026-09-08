using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Online_Shopping_System.Integration
{
    public class ShippingTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;

    private HttpClient _client = default!;
        private int _userId;

        public ShippingTests(ShoppingWebAppFactory factory)
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
        // GET SHIPPING TYPES
        // =========================================================

        [Fact]
        public async Task GetShippingTypes_WithAuthentication_ReturnsShippingTypes()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            db.ShippingTypes.AddRange(
                new ShippingType
                {
                    ShippingName = "Standard",
                    ShippingCost = 50m
                },
                new ShippingType
                {
                    ShippingName = "Express",
                    ShippingCost = 100m
                });

            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync(
                "/Shipping/getShippingTypes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Contain("Standard");
            body.Should().Contain("Express");
        }

        [Fact]
        public async Task GetShippingTypes_WhenNoShippingTypes_ReturnsEmptyList()
        {
            // Act
            var response = await _client.GetAsync(
                "/Shipping/getShippingTypes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Be("[]");
        }

        [Fact]
        public async Task GetShippingTypes_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync(
                "/Shipping/getShippingTypes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
