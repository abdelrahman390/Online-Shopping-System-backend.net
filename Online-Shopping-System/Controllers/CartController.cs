using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Data;
using System.Security.Claims;
using System.Data;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using System.ComponentModel.Design;
//using System.Linq;
//using System.Net;
//using Microsoft.Data.SqlClient;
//using Online_Shopping_System.Services;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


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

        [Authorize]
        [HttpPost("addToCart")]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            /*
             info: Online-Shopping-System[0]
             POST /Cart/addToCart responded 200 in 728 ms
            --
            info: Online-Shopping-System[0]
            POST /Cart/addToCart responded 200 in 203 ms
            ---
            info: Online-Shopping-System[0]
            POST /Cart/addToCart responded 200 in 82 ms
             */
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (userIdClaim == null || userRoleClaim == null || userIpAdress == null)
                {
                    return Unauthorized("Missing data in the token.");
                }

                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0.");
                }

                // --------- FOR PRODUCTION ---------------
                var affectedRows = _dbContext.Products
                    .Where(p => p.ProductId == productId && p.Quantity >= quantity)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(p => p.Quantity, p => p.Quantity - quantity));

                if (affectedRows == 0)
                {
                    // Product doesn't exist OR insufficient quantity
                    return BadRequest("Insufficient product quantity.");
                }

                Product product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);

                Cart cart = _dbContext.Carts.FirstOrDefault(c => c.UserId == int.Parse(userIdClaim.Value) && c.CartStatus == "Pending");

                //if (product == null)
                //{
                //    return NotFound("Product not found.");
                //}

                //if (quantity > product.Quantity)
                //{
                //    Console.WriteLine($"quantity: {quantity}  |  product.Quantity: {product.Quantity}");
                //    return BadRequest("Not enough products in stock.");
                //}
                //product.Quantity -= quantity;

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = int.Parse(userIdClaim.Value),
                        TotalPrice = 0,
                        CreatedAt = DateTimeOffset.Now,
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

                transaction.CommitAsync();

                return Ok(
                    $"Product: {product.ProductId} is now added to cart: {cart.CartId} as item: {item.CartItemId}"
                );
            }
            catch (Exception ex)
            {
               transaction.RollbackAsync();

                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }


        [Authorize]
        [HttpGet("previewCart")]
        public IActionResult PreviewCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (userIdClaim == null || userRoleClaim == null || userIpAdress == null)
                {
                    return Unauthorized("Missing data in the token.");
                }

                Cart cart = _dbContext.Carts.FirstOrDefault(c => c.UserId == int.Parse(userIdClaim.Value) && c.CartStatus == "Pending");

                if(cart == null)
                {
                    return Ok(new List<object>());
                }

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
