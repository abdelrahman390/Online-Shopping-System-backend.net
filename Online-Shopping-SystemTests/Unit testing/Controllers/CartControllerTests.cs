
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Shipping;
using Xunit;

namespace Online_Shopping_SystemTests.Controllers
{
    public class CartControllerTests
    {
        private OnlineShoppingContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OnlineShoppingContext(options);
        }

        private CartController CreateController(OnlineShoppingContext context)
        {
            return new CartController(null!, context);
        }


        // =========================
        // Add to cart
        // =========================

        [Fact]
        public async Task addToCart_ProductNotFound_ReturnsNotFound()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var result = await controller.addToCart(1, 1, 1);

            //var objectResult = Assert.IsType<ObjectResult>(result);

            //Console.WriteLine($"Status: {objectResult.StatusCode}");
            //Console.WriteLine($"Error: {objectResult.Value}");

            var NotFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Contains(
                "Product not found.",
                NotFound.Value?.ToString());
        }

        [Fact]
        public async Task addToCart_QuantityMustBeGreaterThanZero_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);

            context.Products.Add(new Product
            {
                //ProductId = 1,
                Name = "Test Product",
                Price = 10,
                Quantity = 10,
                Type = "Test"
            });
            context.SaveChanges();

            var product = context.Products.FirstOrDefault(p => p.ProductId == 1);

            Assert.NotNull(product);
            Assert.Equal(1, product.ProductId);

            // Act
            var result = await controller.addToCart(1, 0, 1);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Console.WriteLine($"Test: {result}");
            Console.WriteLine($"Test: {badRequest}");

            Assert.Contains("Quantity must be greater than 0", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task addToCart_NotEnoughProductsInStock_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);


            context.Products.Add(new Product
            {
                Name = "Test Product",
                Price = 10,
                Quantity = 3,
                Type = "Test"
            });
            context.SaveChanges();

            // Act
            var result = await controller.addToCart(1, 4, 1);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains("Not enough products in stock", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task addToCart_AddAllStockToCart_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);


            context.Products.Add(new Product
            {
                Name = "Test Product",
                Price = 10,
                Quantity = 3,
                Type = "Test"
            });
            context.SaveChanges();

            // Act
            var result = await controller.addToCart(1, 1, 1);

            // Assert
            var goodRequest = Assert.IsType<OkObjectResult>(result);

            Assert.Contains("is now added to cart", goodRequest.Value?.ToString());
        }

        [Fact]
        public async Task addToCart_AddLessThanQuntityThanAllStockToCart_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);


            context.Products.Add(new Product
            {
                Name = "Test Product",
                Price = 10,
                Quantity = 3,
                Type = "Test"
            });
            context.SaveChanges();

            // Act
            var result = await controller.addToCart(1, 1, 1);

            // Assert
            var goodRequest = Assert.IsType<OkObjectResult>(result);

            Assert.Contains("is now added to cart", goodRequest.Value?.ToString());
        }

        [Fact]
        public async Task previewCart_getEmptyUserCart_ReturnsOk()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);

            // Act
            var result =  controller.previewCart(1);

            // Assert
            var goodRequest = Assert.IsType<OkObjectResult>(result);

            Assert.Contains([], goodRequest.Value?.ToString());
        }

        [Fact]
        public async Task previewCart_getUserCart_ReturnsOk()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context);

            context.Carts.Add(new Cart
            {
                //CartId = 1,
                UserId = 1,
                TotalPrice = 100,
                CreatedAt = DateTimeOffset.Now,
                CartStatus = "Pending"
            });
            context.SaveChanges();

            var product = new Product
            {
                Name = "Test Product",
                Price = 10,
                Quantity = 10,
                Type = "Test"
            };

            context.Products.Add(product);
            context.SaveChanges();

            context.CartItems.Add(new CartItem
            {
                CartId = 1,
                ProductId = 1,
                Quantity = 2,
                TotalPrice = 20
            });
            context.SaveChanges();

            //var cartItems = context.CartItems.ToList();
            //foreach (var cartItem in cartItems)
            //{
            //    Console.WriteLine($"CartItemId: {cartItem.CartItemId}, ProductId: {cartItem.ProductId}, Quantity: {cartItem.Quantity}, TotalPrice: {cartItem.TotalPrice}");
            //}

            //var carts = context.Carts.ToList();
            //foreach (var cart in carts)
            //{
            //    Console.WriteLine($"UserId: {cart.UserId}, UserId: {cart.UserId}, CartStatus: {cart.CartStatus}.");
            //}
            //Console.WriteLine($"previewCart_getUserCart_ReturnsOk: {cartItems.Count()}");

            // Act
            var result = controller.previewCart(1);

            // Assert
            var goodRequest = Assert.IsType<OkObjectResult>(result);

            var items = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                goodRequest.Value);

            Assert.Single(items.Cast<object>());
        }



    }
}