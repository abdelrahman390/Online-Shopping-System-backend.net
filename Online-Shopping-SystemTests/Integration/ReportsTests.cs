using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Online_Shopping_System.Integration
{
    public class ReportsTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;

    private HttpClient _client = default!;

        private int _userId;
        private int _adminId;

        public ReportsTests(ShoppingWebAppFactory factory)
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
                Email = "user@example.com",
                PasswordHashed = [],
                PasswordSalt = [],
                UserRole = "User"
            };

            var admin = new User
            {
                UserTypeId = userType.UserTypeId,
                UserName = "TestAdmin",
                Email = "admin@example.com",
                PasswordHashed = [],
                PasswordSalt = [],
                UserRole = "Admin"
            };

            db.Users.AddRange(user, admin);
            await db.SaveChangesAsync();

            _userId = user.UserId;
            _adminId = admin.UserId;
        }

        public Task DisposeAsync()
            => _dbReset.DisposeAsync();

        // =========================================================
        // EXCEL
        // =========================================================

        [Fact]
        public async Task OrdersExcel_AsAdmin_ReturnsExcelFile()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                "/Reports/orders/excel");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType!
                .MediaType
                .Should()
                .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            var file = await response.Content.ReadAsByteArrayAsync();

            file.Should().NotBeEmpty();
        }

        [Fact]
        public async Task OrdersExcel_AsUser_ReturnsForbidden()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _userId.ToString(),
                "User");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                "/Reports/orders/excel");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task OrdersExcel_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync(
                "/Reports/orders/excel");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // PDF
        // =========================================================

        [Fact]
        public async Task OrdersPdf_AsAdmin_ReturnsPdfFile()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                "/Reports/orders/pdf");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Content.Headers.ContentType!
                .MediaType
                .Should()
                .Be("application/pdf");

            var file = await response.Content.ReadAsByteArrayAsync();

            file.Should().NotBeEmpty();
        }

        [Fact]
        public async Task OrdersPdf_AsUser_ReturnsForbidden()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _userId.ToString(),
                "User");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync(
                "/Reports/orders/pdf");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task OrdersPdf_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync(
                "/Reports/orders/pdf");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

}
