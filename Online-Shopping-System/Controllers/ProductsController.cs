using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Products;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using System.ComponentModel.Design;
//using System.Data;
//using System.Security.Claims;
//using Online_Shopping_System.Services;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using System.Net;


namespace Online_Shopping_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;

        public ProductsController(IConfiguration configuration, OnlineShoppingContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        //[Authorize]
        //[HttpGet("getProducts")]
        //public IActionResult getProducts()
        //{

        //    try
        //    {
        //        //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //        //var userRoleClaim = User.FindFirst(ClaimTypes.Role);
        //        //var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

        //        //if (userIdClaim == null || userRoleClaim == null || userIpAdress == null)
        //        //{
        //        //    return Unauthorized("Missing data in the token.");
        //        //}

        //        List<Product> Products = _dbContext.Products.ToList();

        //        //_AuditLogsService.Log(int.Parse(userIdClaim.Value), "getCompanies", "Companies", DateTime.Now, userIpAdress);

        //        //Console.WriteLine($"Test from AuditLogs second: {testVal}");

        //        return Ok(Products);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return StatusCode(
        //            500,
        //            $"Error: {ex.Message}"
        //        );
        //    }
        //}


        [Authorize(Roles = "Admin")]
        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct(
            [FromBody] AddProductRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest("Product name is required.");

                if (request.Price < 0)
                    return BadRequest("Price cannot be negative.");

                if (request.Quantity < 0)
                    return BadRequest("Quantity cannot be negative.");

                if (string.IsNullOrWhiteSpace(request.Type))
                    return BadRequest("Product type is required.");

                Product newProduct;

                switch (request.Type)
                {
                    case "Electronics":

                        if (request.Warranty == null || request.Warranty < 0)
                            return BadRequest(
                                "Warranty is required for electronics."
                            );

                        newProduct = new Electronics
                        {
                            Name = request.Name,
                            Price = request.Price,
                            Quantity = request.Quantity,
                            Type = "Electronics",
                            Warranty = request.Warranty.Value
                        };

                        break;

                    case "Books":

                        if (string.IsNullOrWhiteSpace(request.Author))
                            return BadRequest(
                                "Author is required for books."
                            );

                        if (string.IsNullOrWhiteSpace(request.ISBN))
                            return BadRequest(
                                "ISBN is required for books."
                            );

                        newProduct = new Books
                        {
                            Name = request.Name,
                            Price = request.Price,
                            Quantity = request.Quantity,
                            Type = "Books",
                            Author = request.Author,
                            ISBN = request.ISBN
                        };

                        break;

                    case "Clothes":

                        if (request.Size == null)
                            return BadRequest(
                                "Size is required for clothes."
                            );

                        if (string.IsNullOrWhiteSpace(request.Color))
                            return BadRequest(
                                "Color is required for clothes."
                            );

                        newProduct = new Clothes
                        {
                            Name = request.Name,
                            Price = request.Price,
                            Quantity = request.Quantity,
                            Type = "Clothes",
                            Size = request.Size.Value,
                            Color = request.Color
                        };

                        break;

                    default:
                        return BadRequest(
                            "Invalid product type. Use Electronics, Books, or Clothes."
                        );
                }

                _dbContext.Products.Add(newProduct);

                _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Product added successfully.",
                    productId = newProduct.ProductId,
                    product = newProduct
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    $"An error occurred while adding the product: {ex.Message}"
                );
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products =  _dbContext.Products
                .ToList();

            var result = products.Select(product => new
            {
                product.ProductId,
                product.Name,
                product.Price,
                product.Quantity,
                product.Type,

                // Electronics data
                Warranty = product is Electronics electronics
                    ? electronics.Warranty
                    : (double?)null,

                // Books data
                Author = product is Books book
                    ? book.Author
                    : null,

                ISBN = product is Books book2
                    ? book2.ISBN
                    : null,

                // Clothes data
                Size = product is Clothes clothes
                    ? clothes.Size
                    : (int?)null,

                Color = product is Clothes clothes2
                    ? clothes2.Color
                    : null
            });

            return Ok(result);
        }

    }
}
