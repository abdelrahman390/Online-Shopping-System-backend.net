using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Products;
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

        public async Task InitializeAsync()
        {
            await _dbReset.InitializeAsync();
            await _dbReset.ResetAsync();

            _client = _factory.CreateClient();

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

            var product = new Product
            {
                ProductId = 1,
                Name = "Test Widget",
                Price = 9.99m,
                Quantity = 10
            };

            db.Products.Add(product);

            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Cart/addToCart?productId=1&quantity=2",
                null);

            // Assert - HTTP response
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();

            responseBody.Should().Contain("Product: 1 is now added to cart");

            // Assert - database
            var cart = await db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(
                    c => c.UserId == 1 &&
                         c.CartStatus == "Pending");

            cart.Should().NotBeNull();

            cart!.CartItems.Should()
                .ContainSingle(item =>
                    item.ProductId == 1 &&
                    item.Quantity == 2 &&
                    item.TotalPrice == 19.98m);

            cart.TotalPrice.Should().Be(19.98m);

            // Product stock should decrease from 10 to 8
            var updatedProduct = await db.Products
                .FirstAsync(p => p.ProductId == 1);

            updatedProduct.Quantity.Should().Be(8);
        }
    }
}