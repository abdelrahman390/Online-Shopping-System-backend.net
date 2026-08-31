using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Online_Shopping_System.Integration
{
    public class CartTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;
        private HttpClient _client = default!;

        public CartTests(ShoppingWebAppFactory factory)
        {
            _factory = factory;
            _dbReset = new DatabaseResetFixture(factory);
        }

        //public async Task InitializeAsync()
        //{
        //    await _dbReset.InitializeAsync();
        //    await _dbReset.ResetAsync();

        //    _client = _factory.CreateClient();

        //    var token = TestAuthHelper.GenerateToken(userId: "1");

        //    _client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", token);
        //}
        public async Task InitializeAsync()
        {
            await _dbReset.InitializeAsync();
            await _dbReset.ResetAsync();

            _client = _factory.CreateClient();

            // Seed the user that the JWT claims to represent
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OnlineShoppingContext>();

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
                    UserName = "Test User",
                    Email = "test@example.com",
                    PasswordHashed = [],
                    PasswordSalt = [],
                    UserRole = "User"
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

            }

            var token = TestAuthHelper.GenerateToken(userId: "1");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public Task DisposeAsync()
            => _dbReset.DisposeAsync();

        [Fact]
        public async Task AddToCart_WithValidProduct_AddsProductToCartAndDecreasesStock()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var product = new Electronics
            {
                Name = "Test Widget",
                Price = 9.99m,
                Quantity = 10,
                Type = "Electronics",
            };

            db.Products.Add(product);

            await db.SaveChangesAsync();

            Console.WriteLine($"########### Products: {product.ProductId}");

            Console.WriteLine($"########### Users count: {db.Users.Count()}");

            Console.WriteLine($"########### User: {db.Users.First().UserId}");

            // Act
            var response = await _client.PostAsync(
                "/Cart/addToCart?productId=1&quantity=2",
                null);


            // Assert - HTTP response
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode}");
            Console.WriteLine($"Response: {responseBody}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert - database
            var cart = await db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(
                    c => c.UserId == 1 &&
                         c.CartStatus == "Pending");

            cart.Should().NotBeNull();

            cart!.CartItems.Should()
                .ContainSingle(item =>
                    item.ProductId == product.ProductId &&
                    item.Quantity == 2 &&
                    item.TotalPrice == 19.98m);

            cart.TotalPrice.Should().Be(19.98m);

            // Product stock should decrease from 10 to 8
            var updatedProduct = await db.Products
                .AsNoTracking()
                .FirstAsync(p => p.ProductId == 1);

            updatedProduct.Quantity.Should().Be(8);
        }

        //public async Task PreviewCart_WithValidCridentials()
        //{
        //    // Arrange
        //    using var scope = _factory.Services.CreateScope();

        //    var db = scope.ServiceProvider
        //        .GetRequiredService<OnlineShoppingContext>();

        //    var Cart = new Cart
        //    {
        //        UserId = 1,
        //        TotalPrice = 0,
        //        CreatedAt = DateTimeOffset.Now,
        //        CartStatus = "Pending"
        //    };

        //    db.Carts.Add(Cart);

        //    await db.SaveChangesAsync();

        //    //Console.WriteLine($"########### Products: {product.ProductId}");

        //    //Console.WriteLine($"########### Users count: {db.Users.Count()}");

        //    //Console.WriteLine($"########### User: {db.Users.First().UserId}");

        //    // Act
        //    var response = await _client.PostAsync(
        //        "/Cart/previewCart",
        //        null);


        //    // Assert - HTTP response
        //    var responseBody = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine($"Status: {(int)response.StatusCode}");
        //    Console.WriteLine($"Response: {responseBody}");

        //    response.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Assert - database
        //    var cart = await db.Carts
        //        .Include(c => c.CartItems)
        //        .FirstOrDefaultAsync(
        //            c => c.UserId == 1 &&
        //                 c.CartStatus == "Pending");

        //    cart.Should().NotBeNull();

        //    cart!.CartItems.Should()
        //        .ContainSingle(item =>
        //            item.ProductId == product.ProductId &&
        //            item.Quantity == 2 &&
        //            item.TotalPrice == 19.98m);

        //    cart.TotalPrice.Should().Be(19.98m);

        //    // Product stock should decrease from 10 to 8
        //    var updatedProduct = await db.Products
        //        .AsNoTracking()
        //        .FirstAsync(p => p.ProductId == 1);

        //    updatedProduct.Quantity.Should().Be(8);
        //}
    }
}