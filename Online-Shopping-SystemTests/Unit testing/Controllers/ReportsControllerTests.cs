using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_System.Tests.Controllers
{
    public class ReportsControllerTests : IDisposable
    {
        private readonly SqliteContextFixture _db;
        private readonly ReportService _reportService;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _db = new SqliteContextFixture();

            _reportService = new ReportService(_db.Context);

            _controller = new ReportsController(_reportService);
        }

        private static async Task<OnlineShoppingContext> CreateContextAsync()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            var context = new OnlineShoppingContext(options);

            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        public void Dispose() => _db.Dispose();


        private static async Task SeedOrderAsync(OnlineShoppingContext context)
        {
            var userType = new UserType
            {
                UserTypeName = "User"
            };

            context.UserTypes.Add(userType);
            await context.SaveChangesAsync();

            var user = new User
            {
                UserName = "testuser",
                UserTypeId = userType.UserTypeId,
                Email = "abdelrahmanbo390@gmail.com",
                PasswordHashed = new byte[] { 0x00 },
                PasswordSalt = new byte[] { 0x00 },
                UserRole = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.UserId,
                TotalPrice = 100,
                CartStatus = "Pending",
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var shippingType = new ShippingType
            {
                ShippingName = "Express Shipping"
            };

            context.ShippingTypes.Add(shippingType);
            await context.SaveChangesAsync();

            var order = new Order
            {
                CartId = cart.CartId,
                UserId = user.UserId,
                TotalCost = 100,
                PaymentTypeName = "Credit Card",
                CreatedAt = DateTimeOffset.UtcNow,
                OrderStatus = "Confirmed"
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var shippingRecord = new ShippingRecords
            {
                OrderId = order.OrderId,
                ShippingTypeId = shippingType.ShippingTypeId
            };

            context.ShippingRecords.Add(shippingRecord);

            await context.SaveChangesAsync();
        }


        // ============================================================
        // OrdersExcel Tests
        // ============================================================

        [Fact]
        public async Task OrdersExcel_ReturnsFile_WithExcelContentType()
        {
            var result = await _controller.OrdersExcel();

            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileResult.ContentType);

            Assert.NotNull(fileResult.FileContents);
            Assert.NotEmpty(fileResult.FileContents);

            Assert.StartsWith(
                "Orders-",
                fileResult.FileDownloadName);
        }


        // ============================================================
        // OrdersPdf Tests
        // ============================================================

        [Fact]
        public async Task OrdersPdf_ReturnsFile_WithPdfContentType()
        {
            // Arrange
            await using var context = await CreateContextAsync();

            await SeedOrderAsync(context);

            var result = await _controller.OrdersPdf();

            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal(
                "application/pdf",
                fileResult.ContentType);

            Assert.NotNull(fileResult.FileContents);
            Assert.NotEmpty(fileResult.FileContents);
        }
    }
}