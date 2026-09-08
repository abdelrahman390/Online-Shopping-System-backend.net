using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Online_Shopping_System.Integration
{
    public class ProductsTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;

    private HttpClient _client = default!;

        private int _userId;
        private int _adminId;

        public ProductsTests(ShoppingWebAppFactory factory)
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
                Email = "user@test.com",
                PasswordHashed = [],
                PasswordSalt = [],
                UserRole = "User"
            };

            var admin = new User
            {
                UserTypeId = userType.UserTypeId,
                UserName = "TestAdmin",
                Email = "admin@test.com",
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
        // ADD PRODUCT
        // =========================================================

        [Fact]
        public async Task AddProduct_AsAdmin_WithValidElectronics_AddsProduct()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Laptop",
                Price = 15000m,
                Quantity = 10,
                Type = "Electronics",
                Warranty = 2
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var product = await db.Products
                .OfType<Electronics>()
                .FirstOrDefaultAsync(p => p.Name == "Laptop");

            product.Should().NotBeNull();
            product!.Price.Should().Be(15000m);
            product.Quantity.Should().Be(10);
            product.Type.Should().Be("Electronics");
            product.Warranty.Should().Be(2);
        }

        [Fact]
        public async Task AddProduct_AsAdmin_WithValidBook_AddsBook()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Clean Code",
                Price = 500m,
                Quantity = 20,
                Type = "Books",
                Author = "Robert Martin",
                ISBN = "9780132350884"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var product = await db.Products
                .OfType<Books>()
                .FirstOrDefaultAsync(p => p.Name == "Clean Code");

            product.Should().NotBeNull();
            product!.Author.Should().Be("Robert Martin");
            product.ISBN.Should().Be("9780132350884");
        }

        [Fact]
        public async Task AddProduct_AsAdmin_WithValidClothes_AddsClothes()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "T-Shirt",
                Price = 300m,
                Quantity = 15,
                Type = "Clothes",
                Size = 42,
                Color = "Black"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var product = await db.Products
                .OfType<Clothes>()
                .FirstOrDefaultAsync(p => p.Name == "T-Shirt");

            product.Should().NotBeNull();
            product!.Size.Should().Be(42);
            product.Color.Should().Be("Black");
        }

        [Fact]
        public async Task AddProduct_AsUser_ReturnsForbidden()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _userId.ToString(),
                "User");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Laptop",
                Price = 1000m,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AddProduct_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            var request = new
            {
                Name = "Laptop",
                Price = 1000m,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddProduct_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "",
                Price = 100m,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddProduct_WithNegativePrice_ReturnsBadRequest()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Laptop",
                Price = -1m,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddProduct_WithNegativeQuantity_ReturnsBadRequest()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Laptop",
                Price = 1000m,
                Quantity = -5,
                Type = "Electronics",
                Warranty = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddProduct_WithInvalidType_ReturnsBadRequest()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Unknown",
                Price = 100m,
                Quantity = 5,
                Type = "UnknownType"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddProduct_ElectronicsWithoutWarranty_ReturnsBadRequest()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _adminId.ToString(),
                "Admin");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Laptop",
                Price = 1000m,
                Quantity = 5,
                Type = "Electronics"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // GET ALL PRODUCTS
        // =========================================================

        [Fact]
        public async Task GetAllProducts_WithAuthentication_ReturnsProducts()
        {
            // Arrange
            var token = TestAuthHelper.GenerateToken(
                _userId.ToString(),
                "User");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            db.Products.AddRange(
                new Electronics
                {
                    Name = "Laptop",
                    Price = 15000m,
                    Quantity = 5,
                    Type = "Electronics",
                    Warranty = 2
                },
                new Books
                {
                    Name = "Clean Code",
                    Price = 500m,
                    Quantity = 10,
                    Type = "Books",
                    Author = "Robert Martin",
                    ISBN = "123456"
                },
                new Clothes
                {
                    Name = "T-Shirt",
                    Price = 300m,
                    Quantity = 20,
                    Type = "Clothes",
                    Size = 42,
                    Color = "Black"
                });

            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/Products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Contain("Laptop");
            body.Should().Contain("Clean Code");
            body.Should().Contain("T-Shirt");
        }

        [Fact]
        public async Task GetAllProducts_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/Products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

}
