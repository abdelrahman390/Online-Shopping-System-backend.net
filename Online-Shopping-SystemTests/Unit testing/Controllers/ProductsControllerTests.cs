using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_SystemTests.Controllers
{
    public class ProductsControllerTests
    {
        private OnlineShoppingContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OnlineShoppingContext(options);
        }

        private ProductsController CreateController(OnlineShoppingContext context)
        {
            return new ProductsController(null!, context);
        }

        [Fact]
        public async Task getProducts_getEmptyProducts_ReturnsOk()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.getProducts();

            var goodResult = Assert.IsType<OkObjectResult>(result);

            Assert.Contains(
                [],
                goodResult.Value?.ToString());
        }

        [Fact]
        public async Task getProducts_getAllProducts_ReturnsOk()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            context.Products.Add(new Product
            {
                Name = "Test Product",
                Price = 10.99m,
                Quantity = 5,
                Type = "Electronics"
            });
            context.SaveChanges();

            var result = controller.getProducts();

            // Assert
            var goodRequest = Assert.IsType<OkObjectResult>(result);

            var items = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                goodRequest.Value);

            Assert.Single(items.Cast<object>());
        }
    }
}
