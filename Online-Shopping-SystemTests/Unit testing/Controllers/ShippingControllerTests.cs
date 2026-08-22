using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_SystemTests.Controllers
{
    public class ShippingControllerTests
    {
        private OnlineShoppingContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OnlineShoppingContext(options);
        }

        private ShippingController CreateController(OnlineShoppingContext context)
        {
            return new ShippingController(null!, context);
        }

        [Fact]
        public async Task GetShippingTypes_getEmptyShippingTypes_ReturnsOk()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.GetShippingTypes();

            var goodResult = Assert.IsType<OkObjectResult>(result);

            Assert.Contains(
                [],
                goodResult.Value?.ToString());
        }

        [Fact]
        public async Task GetShippingTypes_getAllProducts_ReturnsOk()
        {
            using var context = CreateContext();
            var controller = CreateController(context);

            context.ShippingTypes.Add(new ShippingType
            {
                ShippingName = "Standard Shipping",
                ShippingCost = 5,
                ShippingDuration = 2
            });
            context.SaveChanges();

            var result = controller.GetShippingTypes();

            // Assert
            var goodRequest = Assert.IsType<OkObjectResult>(result);

            var items = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                goodRequest.Value);

            Assert.Single(items.Cast<object>());
        }

    }
}
