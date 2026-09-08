using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Models.Payment;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using static market_watch.Controllers.RegisterController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace Online_Shopping_System.Data
{
    public class CreateTestData
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;
        public CreateTestData(IConfiguration configuration, OnlineShoppingContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public void Run()
        {
            CreateTestUserTypes();
            CreateTestUsers();
            CreateTestProducts();
            CreateTestShippingTypes();
        }

        // add products
        public void CreateTestProducts()
        {
            for (int i = 0; i < 20; i++)
            {
                Product newElectronic = new Electronics
                {
                    Name = "Electronics-" + i,
                    Price = 250,
                    Quantity = 10000,
                    Type = "Electronics",
                    Warranty = 24
                };
                _dbContext.Products.Add(newElectronic);
            }

            for (int i = 0; i < 20; i++)
            {
                Product newBook = new Books
                {
                    Name = "Book-" + i,
                    Price = 50,
                    Quantity = 10000,
                    Type = "Books",
                    Author = "Author-" + i,
                    ISBN = "ISBN-" + i
                };

                _dbContext.Products.Add(newBook);
            }

            for (int i = 0; i < 20; i++)
            {
                Product newClothes = new Clothes
                {
                    Name = "Clothes-" + i,
                    Price = 70,
                    Quantity = 10000,
                    Type = "Clothes",
                    Size = i+1,
                    Color = "Color-" + i
                };

                _dbContext.Products.Add(newClothes);
            }

            _dbContext.SaveChanges();
            Console.WriteLine("Test Products created successfully.");
        }

        public void CreateTestUsers()
        {
            for (int i = 0; i < 10; i++)
            {
                User newUser = new User
                {
                    UserName = "User" + i.ToString(),
                    Email = "User" + i.ToString() + "@gmail.com",
                    PasswordHashed = Array.Empty<byte>(),
                    UserTypeId = _dbContext.UserTypes.First().UserTypeId,
                    PasswordSalt = Array.Empty<byte>(),
                    UserRole = "User"
                };

                _dbContext.Users.Add(newUser);
            }
            _dbContext.SaveChanges();
            Console.WriteLine("Test Users created successfully.");
        }

        public void CreateTestShippingTypes()
        {
                ShippingType Standard = new ShippingType
                {
                    ShippingName = "Standard",
                    ShippingCost = 5,
                    ShippingDuration = 5
                };

                ShippingType Express = new ShippingType
                {
                    ShippingName = "Express",
                    ShippingCost = 10,
                    ShippingDuration = 3
                };

                ShippingType SameDay = new ShippingType
                {
                    ShippingName = "SameDay",
                    ShippingCost = 15,
                    ShippingDuration = 1
                };

            _dbContext.ShippingTypes.Add(Standard);
            _dbContext.ShippingTypes.Add(Express);
            _dbContext.ShippingTypes.Add(SameDay);

            _dbContext.SaveChanges();
            Console.WriteLine("Test ShippingTypes created successfully.");
        }

        public void CreateTestUserTypes()
        {
            UserType Normal = new UserType
            {
                UserTypeName = "Normal",
                Discount = 0.0
            };

            UserType Premium = new UserType
            {
                UserTypeName = "Premium",
                Discount = 5.0,
            };

            UserType VIP = new UserType
            {
                UserTypeName = "VIP",
                Discount = 10.0,
            };

            _dbContext.UserTypes.Add(Normal);
            _dbContext.UserTypes.Add(Premium);
            _dbContext.UserTypes.Add(VIP);

            _dbContext.SaveChanges();
            Console.WriteLine("Test UserTypes created successfully.");
        }

    }
}
