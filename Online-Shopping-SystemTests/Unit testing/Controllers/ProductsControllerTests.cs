using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Online_Shopping_System.Controllers;
using Online_Shopping_System.Models.Products;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_System.Tests.Controllers
{
    // NOTE: SqliteContextFixture is created fresh per test
    // (xUnit creates a new test class instance per [Fact]),
    // so there is no cross-test state leak.
    public class ProductsControllerTests : IDisposable
    {
        private readonly SqliteContextFixture _db;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _db = new SqliteContextFixture();

            var configMock = new Mock<IConfiguration>();

            _controller = new ProductsController(
                configMock.Object,
                _db.Context
            );
        }

        public void Dispose() => _db.Dispose();


        // ============================================================
        // GetAllProducts Tests
        // ============================================================

        [Fact]
        public async Task GetAllProducts_ReturnsOk_WhenNoProductsExist()
        {
            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var products = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                okResult.Value
            );

            Assert.Empty(products.Cast<object>());
        }


        [Fact]
        public async Task GetAllProducts_ReturnsAllProducts_WhenProductsExist()
        {
            _db.Context.Products.AddRange(
                new Electronics
                {
                    Name = "Headphones",
                    Price = 100,
                    Quantity = 5,
                    Type = "Electronics",
                    Warranty = 12
                },

                new Books
                {
                    Name = "Clean Code",
                    Price = 50,
                    Quantity = 10,
                    Type = "Books",
                    Author = "Robert Martin",
                    ISBN = "9780132350884"
                },

                new Clothes
                {
                    Name = "T-Shirt",
                    Price = 25,
                    Quantity = 20,
                    Type = "Clothes",
                    Size = 42,
                    Color = "Black"
                }
            );

            await _db.Context.SaveChangesAsync();

            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var products = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                okResult.Value
            );

            Assert.Equal(3, products.Cast<object>().Count());
        }


        [Fact]
        public async Task GetAllProducts_ReturnsElectronicsData_Correctly()
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

            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var products = okResult.Value as System.Collections.IEnumerable;

            var product = products!
                .Cast<object>()
                .Single();

            var warranty = product
                .GetType()
                .GetProperty("Warranty")!
                .GetValue(product);

            Assert.Equal((double)12, warranty);
        }


        [Fact]
        public async Task GetAllProducts_ReturnsBooksData_Correctly()
        {
            _db.Context.Products.Add(new Books
            {
                Name = "Clean Code",
                Price = 50,
                Quantity = 10,
                Type = "Books",
                Author = "Robert Martin",
                ISBN = "9780132350884"
            });

            await _db.Context.SaveChangesAsync();

            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var products = okResult.Value as System.Collections.IEnumerable;

            var product = products!
                .Cast<object>()
                .Single();

            var author = product
                .GetType()
                .GetProperty("Author")!
                .GetValue(product);

            var isbn = product
                .GetType()
                .GetProperty("ISBN")!
                .GetValue(product);

            Assert.Equal("Robert Martin", author);
            Assert.Equal("9780132350884", isbn);
        }


        [Fact]
        public async Task GetAllProducts_ReturnsClothesData_Correctly()
        {
            _db.Context.Products.Add(new Clothes
            {
                Name = "T-Shirt",
                Price = 25,
                Quantity = 20,
                Type = "Clothes",
                Size = 42,
                Color = "Black"
            });

            await _db.Context.SaveChangesAsync();

            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var products = okResult.Value as System.Collections.IEnumerable;

            var product = products!
                .Cast<object>()
                .Single();

            var size = product
                .GetType()
                .GetProperty("Size")!
                .GetValue(product);

            var color = product
                .GetType()
                .GetProperty("Color")!
                .GetValue(product);

            Assert.Equal(42, size);
            Assert.Equal("Black", color);
        }


        // ============================================================
        // AddProduct Tests
        // ============================================================

        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenNameIsEmpty()
        {
            var request = new AddProductRequest
            {
                Name = "",
                Price = 100,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 12
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Product name is required.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenPriceIsNegative()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = -1,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 12
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Price cannot be negative.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenQuantityIsNegative()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = 100,
                Quantity = -1,
                Type = "Electronics",
                Warranty = 12
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Quantity cannot be negative.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenTypeIsEmpty()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 5,
                Type = "",
                Warranty = 12
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Product type is required.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenTypeIsInvalid()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 5,
                Type = "Food"
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Invalid product type. Use Electronics, Books, or Clothes.",
                badRequest.Value
            );
        }


        // ============================================================
        // Electronics Tests
        // ============================================================

        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenElectronicsWarrantyIsMissing()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 5,
                Type = "Electronics",
                Warranty = null
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Warranty is required for electronics.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenElectronicsWarrantyIsNegative()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 5,
                Type = "Electronics",
                Warranty = -1
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Warranty is required for electronics.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_CreatesElectronics_WhenValid()
        {
            var request = new AddProductRequest
            {
                Name = "Headphones",
                Price = 100,
                Quantity = 5,
                Type = "Electronics",
                Warranty = 12
            };

            var result = await _controller.AddProduct(request);

            Assert.IsType<OkObjectResult>(result);

            var product = await _db.Context.Products
                .OfType<Electronics>()
                .SingleAsync();

            Assert.Equal("Headphones", product.Name);
            Assert.Equal(100, product.Price);
            Assert.Equal(5, product.Quantity);
            Assert.Equal("Electronics", product.Type);
            Assert.Equal(12, product.Warranty);
        }


        // ============================================================
        // Books Tests
        // ============================================================

        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenBookAuthorIsMissing()
        {
            var request = new AddProductRequest
            {
                Name = "Clean Code",
                Price = 50,
                Quantity = 10,
                Type = "Books",
                Author = "",
                ISBN = "9780132350884"
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Author is required for books.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenBookISBNIsMissing()
        {
            var request = new AddProductRequest
            {
                Name = "Clean Code",
                Price = 50,
                Quantity = 10,
                Type = "Books",
                Author = "Robert Martin",
                ISBN = ""
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "ISBN is required for books.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_CreatesBook_WhenValid()
        {
            var request = new AddProductRequest
            {
                Name = "Clean Code",
                Price = 50,
                Quantity = 10,
                Type = "Books",
                Author = "Robert Martin",
                ISBN = "9780132350884"
            };

            var result = await _controller.AddProduct(request);

            Assert.IsType<OkObjectResult>(result);

            var product = await _db.Context.Products
                .OfType<Books>()
                .SingleAsync();

            Assert.Equal("Clean Code", product.Name);
            Assert.Equal(50, product.Price);
            Assert.Equal(10, product.Quantity);
            Assert.Equal("Books", product.Type);
            Assert.Equal("Robert Martin", product.Author);
            Assert.Equal("9780132350884", product.ISBN);
        }


        // ============================================================
        // Clothes Tests
        // ============================================================

        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenClothesSizeIsMissing()
        {
            var request = new AddProductRequest
            {
                Name = "T-Shirt",
                Price = 25,
                Quantity = 20,
                Type = "Clothes",
                Size = null,
                Color = "Black"
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Size is required for clothes.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenClothesColorIsMissing()
        {
            var request = new AddProductRequest
            {
                Name = "T-Shirt",
                Price = 25,
                Quantity = 20,
                Type = "Clothes",
                Size = 42,
                Color = ""
            };

            var result = await _controller.AddProduct(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Color is required for clothes.",
                badRequest.Value
            );
        }


        [Fact]
        public async Task AddProduct_CreatesClothes_WhenValid()
        {
            var request = new AddProductRequest
            {
                Name = "T-Shirt",
                Price = 25,
                Quantity = 20,
                Type = "Clothes",
                Size = 42,
                Color = "Black"
            };

            var result = await _controller.AddProduct(request);

            Assert.IsType<OkObjectResult>(result);

            var product = await _db.Context.Products
                .OfType<Clothes>()
                .SingleAsync();

            Assert.Equal("T-Shirt", product.Name);
            Assert.Equal(25, product.Price);
            Assert.Equal(20, product.Quantity);
            Assert.Equal("Clothes", product.Type);
            Assert.Equal(42, product.Size);
            Assert.Equal("Black", product.Color);
        }
    }
}