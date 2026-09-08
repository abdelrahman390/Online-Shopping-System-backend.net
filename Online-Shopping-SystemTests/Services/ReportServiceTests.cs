
using ClosedXML.Excel;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using Online_Shopping_System.Services;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_System.Tests.Services
{
    public class ReportServiceTests
    {
        public ReportServiceTests()
        {
            QuestPDF.Settings.License = LicenseType.Evaluation;
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

        private static async Task SeedOrderAsync(
            OnlineShoppingContext context)
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

        [Fact]
        public async Task GenerateOrdersExcelAsync_WithOrders_ReturnsValidExcelFile()
        {
            // Arrange
            await using var context = await CreateContextAsync();

            await SeedOrderAsync(context);

            var service = new ReportService(context);

            // Act
            var result = await service.GenerateOrdersExcelAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            using var stream = new MemoryStream(result);
            using var workbook = new XLWorkbook(stream);

            workbook.Worksheets.Should().ContainSingle();

            var worksheet = workbook.Worksheet("Orders");

            worksheet.Cell(1, 1).GetString()
                .Should().Be("Order ID");

            worksheet.Cell(1, 2).GetString()
                .Should().Be("Cart ID");

            worksheet.Cell(1, 3).GetString()
                .Should().Be("User Name");

            worksheet.Cell(1, 4).GetString()
                .Should().Be("Total Price");

            worksheet.Cell(1, 5).GetString()
                .Should().Be("Status");

            worksheet.Cell(1, 6).GetString()
                .Should().Be("Order Date");

            worksheet.Cell(1, 7).GetString()
                .Should().Be("Payment Type");

            worksheet.Cell(1, 8).GetString()
                .Should().Be("Cart Status");

            worksheet.Cell(1, 9).GetString()
                .Should().Be("Cart Date");

            worksheet.Cell(1, 10).GetString()
                .Should().Be("Shipping Type");

            // Verify order data
            worksheet.Cell(2, 3).GetString()
                .Should().Be("testuser");

            worksheet.Cell(2, 4).GetValue<decimal>()
                .Should().Be(100);

            worksheet.Cell(2, 7).GetString()
                .Should().Be("Credit Card");

            worksheet.Cell(2, 10).GetString()
                .Should().Be("Express Shipping");
        }

        [Fact]
        public async Task GenerateOrdersPdfAsync_WithOrders_ReturnsValidPdfFile()
        {
            // Arrange
            await using var context = await CreateContextAsync();

            await SeedOrderAsync(context);

            var service = new ReportService(context);

            // Act
            var result = await service.GenerateOrdersPdfAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            // PDF files start with %PDF
            var header = System.Text.Encoding.ASCII
                .GetString(result.Take(4).ToArray());

            header.Should().Be("%PDF");
        }
    }
}
