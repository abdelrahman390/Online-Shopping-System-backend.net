using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Data;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
//using Online_Shopping_System.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace Online_Shopping_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;

        public CartController(IConfiguration configuration, OnlineShoppingContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("addToCart")]
        public async Task<IActionResult> addToCart(int productId, int quantity, int userId)
        {
            try
            {
                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0.");
                }

                //var affectedRows = await _dbContext.Products
                //    .Where(p => p.ProductId == productId && p.Quantity >= quantity)
                //    .ExecuteUpdateAsync(setters => setters
                //        .SetProperty(p => p.Quantity, p => p.Quantity - quantity));

                //if (affectedRows == 0)
                //{
                //    // Product doesn't exist OR insufficient quantity
                //    return BadRequest("Insufficient product quantity.");
                //}

                Product product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);

                Cart cart = _dbContext.Carts.FirstOrDefault(c => c.UserId == userId && c.CartStatus == "Pending");

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (quantity >= product.Quantity)
                {
                    return BadRequest("Not enough products in stock.");
                }
                product.Quantity -= quantity;

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        TotalPrice = 0,
                        CreatedAt = DateTime.UtcNow,
                        CartStatus = "Pending"
                    };

                    _dbContext.Carts.Add(cart);
                    _dbContext.SaveChanges();
                }

                CartItem item = _dbContext.CartItems.FirstOrDefault(i => i.ProductId == productId && i.CartId == cart.CartId);

                if (item == null)
                {
                    item = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = quantity,
                        TotalPrice = product.Price * quantity
                    };
                    _dbContext.CartItems.Add(item);
                    _dbContext.SaveChanges();
                }
                else
                {
                    item.Quantity += quantity;
                    item.TotalPrice += quantity * product.Price;
                }

                cart.TotalPrice += product.Price * quantity;

                _dbContext.SaveChanges();

                return Ok(
                    $"Product: {product.ProductId} is now added to cart: {cart.CartId} as item: {item.CartItemId}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }


        [HttpGet("previewCart")]
        public IActionResult previewCart(int userId)
        {
            try
            {
                //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                //var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                //var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                //if (userIdClaim == null || userRoleClaim == null || userIpAdress == null)
                //{
                //    return Unauthorized("Missing data in the token.");
                //}

                Cart cart = _dbContext.Carts.FirstOrDefault(c => c.UserId == userId && c.CartStatus == "Pending");

                //Product product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);

                if(cart == null)
                {
                    return Ok(new List<object>());
                }

                //List<CartItem> cartItems = _dbContext.CartItem.Where(i => i.CartId == cart.CartId).ToList();
                var cartItems = _dbContext.CartItems
                    .Where(i => i.CartId == cart.CartId)
                    .Select(i => new
                    {
                        i.CartItemId,
                        ProductName = i.Product.Name,
                        i.Quantity,
                        i.TotalPrice,
                        ItemPrice = i.Product.Price
                    })
                    .ToList();

                //foreach (var item in cartItems)
                //{
                //    Product product = _dbContext.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                //    item.TotalPrice = item.Quantity * 100;
                //    item.
                //}
                //CartItem item = _dbContext.CartItem.FirstOrDefault(i => i.CartId == cart.UserId);

                return Ok(
                    cartItems
                );

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }


    }
}
