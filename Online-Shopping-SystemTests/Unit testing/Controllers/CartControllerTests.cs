using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_System.Tests.Controllers
{
    // NOTE: SqliteContextFixture is created fresh per test (xUnit makes a new
    // test class instance per [Fact]), so there's no cross-test state leak.
    public class CartControllerTests : IDisposable
    {
        private readonly SqliteContextFixture _db;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _db = new SqliteContextFixture();

            var configMock = new Mock<IConfiguration>();
            _controller = new CartController(configMock.Object, _db.Context);
        }

        public void Dispose() => _db.Dispose();

        private async Task<int> SeedUserAsync(int userId, string role)
        {
            var userType = new UserType {}; // adjust to your real UserType shape
            _db.Context.UserTypes.Add(userType);
            await _db.Context.SaveChangesAsync();

            var user = new User
            {
                UserId = userId,
                UserName = $"testuser{userId}",
                Email = $"testuser{userId}@example.com",
                PasswordHashed = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 },
                UserRole = role,
                UserTypeId = userType.UserTypeId
            };
            _db.Context.Users.Add(user);
            await _db.Context.SaveChangesAsync();

            return user.UserId;
        }


        [Fact]
        public async Task AddToCart_ReturnsUnauthorized_WhenClaimsMissing()
        {
            _controller.WithNoAuthClaims();

            var result = await _controller.AddToCart(productId: 1, quantity: 1);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Missing data in the token.", unauthorized.Value);
        }

        [Fact]
        public async Task AddToCart_ReturnsBadRequest_WhenQuantityIsZeroOrLess()
        {
            _controller.AuthenticateAs(userId: 1, role: "User");

            var result = await _controller.AddToCart(productId: 1, quantity: 0);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Quantity must be greater than 0.", badRequest.Value);
        }

        [Fact]
        public async Task AddToCart_ReturnsBadRequest_WhenStockIsInsufficient()
        {
            // Adjust these property names to match your real Product subclass.
            _db.Context.Products.Add(new Electronics
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 2,
                Type = "Electronics",
                Warranty = 12
            });
            await _db.Context.SaveChangesAsync();
            var productId = _db.Context.Products.First().ProductId;

            _controller.AuthenticateAs(userId: 1, role: "User");

            var result = await _controller.AddToCart(productId, quantity: 5);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Insufficient product quantity.", badRequest.Value);
        }

        [Fact]
        public async Task AddToCart_CreatesCartAndItem_AndDecrementsStock_WhenValid()
        {
            _db.Context.Products.Add(new Electronics
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 12
            });
            await _db.Context.SaveChangesAsync();
            var productId = _db.Context.Products.First().ProductId;

            await SeedUserAsync(userId: 1, role: "User");

            _controller.AuthenticateAs(userId: 1, role: "User");

            var result = await _controller.AddToCart(productId, quantity: 2);

            if (result is ObjectResult obj && obj.StatusCode == 500)
            {
                Assert.Fail($"Controller returned 500: {obj.Value}");
            }

            Assert.IsType<OkObjectResult>(result);

            //var product = _db.Context.Products.First(p => p.ProductId == productId);
            var product = await _db.Context.Products
                .AsNoTracking()
                .FirstAsync(p => p.ProductId == productId);

            Assert.Equal(3, product.Quantity); // 5 - 2

            var cart = _db.Context.Carts.Single(c => c.UserId == 1 && c.CartStatus == "Pending");
            Assert.Equal(200, cart.TotalPrice); // 100 * 2

            var item = _db.Context.CartItems.Single(i => i.CartId == cart.CartId);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public async Task AddToCart_IncrementsExistingItem_WhenCalledTwiceForSameProduct()
        {
            _db.Context.Products.Add(new Electronics
            {
                Name = "Headphones",
                Price = 50,
                Quantity = 10,
                Type = "Electronics",
                Warranty = 12
            });
            await _db.Context.SaveChangesAsync();
            var productId = _db.Context.Products.First().ProductId;

            await SeedUserAsync(userId: 1, role: "User");

            _controller.AuthenticateAs(userId: 1, role: "User");

            await _controller.AddToCart(productId, quantity: 1);
            await _controller.AddToCart(productId, quantity: 2);

            var cart = _db.Context.Carts.Single(c => c.UserId == 1 && c.CartStatus == "Pending");
            var item = _db.Context.CartItems.Single(i => i.CartId == cart.CartId);

            Assert.Equal(3, item.Quantity);
        }
    }
}
