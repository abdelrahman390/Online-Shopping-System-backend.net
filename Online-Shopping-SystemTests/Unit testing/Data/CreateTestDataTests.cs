using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using Xunit;

namespace Online_Shopping_System.Tests.Data
{
    public class CreateTestDataTests
    {
        private async Task<OnlineShoppingContext> CreateContextAsync()
        {
            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            var context = new OnlineShoppingContext(options);

            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        private IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
        }

        [Fact]
        public async Task CreateTestUserTypes_CreatesThreeUserTypes()
        {
            await using var context = await CreateContextAsync();

            var createTestData = new CreateTestData(
                CreateConfiguration(),
                context);

            createTestData.CreateTestUserTypes();

            var userTypes = await context.UserTypes
                .OrderBy(x => x.UserTypeId)
                .ToListAsync();

            Assert.Equal(3, userTypes.Count);

            Assert.Contains(userTypes, x =>
                x.UserTypeName == "Normal" &&
                x.Discount == 0.0);

            Assert.Contains(userTypes, x =>
                x.UserTypeName == "Premium" &&
                x.Discount == 5.0);

            Assert.Contains(userTypes, x =>
                x.UserTypeName == "VIP" &&
                x.Discount == 10.0);
        }

        [Fact]
        public async Task CreateTestUsers_CreatesTenUsers()
        {
            await using var context = await CreateContextAsync();

            var normalUserType = new UserType
            {
                UserTypeName = "Normal",
                Discount = 0.0
            };

            context.UserTypes.Add(normalUserType);
            await context.SaveChangesAsync();

            var createTestData = new CreateTestData(
                CreateConfiguration(),
                context);

            createTestData.CreateTestUsers();

            var users = await context.Users
                .OrderBy(x => x.UserId)
                .ToListAsync();

            Assert.Equal(10, users.Count);

            Assert.All(users, user =>
            {
                Assert.Equal("User", user.UserRole);
                Assert.Equal(normalUserType.UserTypeId, user.UserTypeId);
                Assert.NotNull(user.Email);
                Assert.NotNull(user.UserName);
            });
        }

        [Fact]
        public async Task CreateTestProducts_CreatesSixtyProducts()
        {
            await using var context = await CreateContextAsync();

            var createTestData = new CreateTestData(
                CreateConfiguration(),
                context);

            createTestData.CreateTestProducts();

            var products = await context.Products.ToListAsync();

            Assert.Equal(60, products.Count);

            Assert.Equal(
                20,
                products.Count(x => x.Type == "Electronics"));

            Assert.Equal(
                20,
                products.Count(x => x.Type == "Books"));

            Assert.Equal(
                20,
                products.Count(x => x.Type == "Clothes"));
        }

        [Fact]
        public async Task CreateTestProducts_CreatesProductsWithExpectedValues()
        {
            await using var context = await CreateContextAsync();

            var createTestData = new CreateTestData(
                CreateConfiguration(),
                context);

            createTestData.CreateTestProducts();

            var electronics = await context.Products
                .OfType<Electronics>()
                .ToListAsync();

            var books = await context.Products
                .OfType<Books>()
                .ToListAsync();

            var clothes = await context.Products
                .OfType<Clothes>()
                .ToListAsync();

            Assert.Equal(20, electronics.Count);
            Assert.Equal(20, books.Count);
            Assert.Equal(20, clothes.Count);

            Assert.All(electronics, product =>
            {
                Assert.Equal(250, product.Price);
                Assert.Equal(10000, product.Quantity);
                Assert.Equal(24, product.Warranty);
            });

            Assert.All(books, product =>
            {
                Assert.Equal(50, product.Price);
                Assert.Equal(10000, product.Quantity);
            });

            Assert.All(clothes, product =>
            {
                Assert.Equal(70, product.Price);
                Assert.Equal(10000, product.Quantity);
            });
        }

        [Fact]
        public async Task CreateTestShippingTypes_CreatesThreeShippingTypes()
        {
            await using var context = await CreateContextAsync();

            var createTestData = new CreateTestData(
                CreateConfiguration(),
                context);

            createTestData.CreateTestShippingTypes();

            var shippingTypes = await context.ShippingTypes
                .OrderBy(x => x.ShippingName)
                .ToListAsync();

            Assert.Equal(3, shippingTypes.Count);

            Assert.Contains(shippingTypes, x =>
                x.ShippingName == "Standard" &&
                x.ShippingCost == 5 &&
                x.ShippingDuration == 5);

            Assert.Contains(shippingTypes, x =>
                x.ShippingName == "Express" &&
                x.ShippingCost == 10 &&
                x.ShippingDuration == 3);

            Assert.Contains(shippingTypes, x =>
                x.ShippingName == "SameDay" &&
                x.ShippingCost == 15 &&
                x.ShippingDuration == 1);
        }

        [Fact]
        public async Task Run_CreatesAllTestData()
        {
            await using var context = await CreateContextAsync();

            var createTestData = new CreateTestData(
                CreateConfiguration(),
                context);

            createTestData.Run();

            Assert.Equal(3, await context.UserTypes.CountAsync());
            Assert.Equal(10, await context.Users.CountAsync());
            Assert.Equal(60, await context.Products.CountAsync());
            Assert.Equal(3, await context.ShippingTypes.CountAsync());
        }
    }
}