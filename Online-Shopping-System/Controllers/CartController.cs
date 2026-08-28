using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Data;
using System.Security.Claims;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                //var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                //var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                //var sw = Stopwatch.StartNew();

                var userId = int.Parse(userIdClaim.Value);

                //Console.WriteLine($"user Id change: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                if (userIdClaim == null)
                {
                    return Unauthorized("Missing data in the token.");
                }

                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0.");
                }

                // --------- FOR PRODUCTION ---------------
                var affectedRows = await _dbContext.Products
                    .Where(p => p.ProductId == productId && p.Quantity >= quantity)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.Quantity, p => p.Quantity - quantity));

                //Console.WriteLine($"AffectedRows DB query: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                if (affectedRows == 0)
                {
                    // Product doesn't exist OR insufficient quantity
                    return BadRequest("Insufficient product quantity.");
                }

                //Product product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                var price = await _dbContext.Products
                    .Where(p => p.ProductId == productId)
                    .Select(p => p.Price)
                    .SingleAsync();

                //Console.WriteLine($"product price DB query: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                Cart cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "Pending");

                //Console.WriteLine($"cart DB query: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        TotalPrice = 0,
                        CreatedAt = DateTimeOffset.Now,
                        CartStatus = "Pending"
                    };

                    _dbContext.Carts.Add(cart);
                    await _dbContext.SaveChangesAsync();

                    //Console.WriteLine($"create cart DB query: {sw.ElapsedMilliseconds} ms");
                    //sw.Restart();
                }



                CartItem item = await _dbContext.CartItems.FirstOrDefaultAsync(i => i.ProductId == productId && i.CartId == cart.CartId);

                //Console.WriteLine($"cart item DB query: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                if (item == null)
                {
                    item = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = quantity,
                        TotalPrice = price * quantity
                    };
                    _dbContext.CartItems.Add(item);
                    await _dbContext.SaveChangesAsync();
                    //Console.WriteLine($"create cart item DB query: {sw.ElapsedMilliseconds} ms");
                    //sw.Restart();
                }
                else
                {
                    item.Quantity += quantity;
                    item.TotalPrice += quantity * price;
                }

                cart.TotalPrice += price * quantity;

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                //Console.WriteLine($"End of api: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                return Ok(
                    $"Product: {productId} is now added to cart: {cart.CartId} as item: {item.CartItemId}"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

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
                var userId = int.Parse(userIdClaim.Value);

                if (userIdClaim == null || userRoleClaim == null || userIpAdress == null)
                {
                    return Unauthorized("Missing data in the token.");
                }

                Cart cart = _dbContext.Carts.FirstOrDefault(c => c.UserId == userId && c.CartStatus == "Pending");

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
