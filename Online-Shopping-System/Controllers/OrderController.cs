using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using System.Security.Claims;
//using System.Threading.Tasks;

namespace Online_Shopping_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;
        private readonly IMemoryCache _cache;

        public OrderController(IConfiguration configuration, OnlineShoppingContext dbContext, IMemoryCache cache)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _cache = cache;
        }

        [Authorize]
        //[EnableRateLimiting("checkout")]
        [HttpPost("confirmOrder")]
        public async Task<IActionResult> ConfirmOrder(int shippingTypeId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                //var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                //var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (userIdClaim == null)
                {
                    return Unauthorized("Missing data in the token.");
                }
                int userId = int.Parse(userIdClaim.Value);

                Cart cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.CartStatus == "Pending");

                if (cart == null)
                {
                    return BadRequest($"Cart not found. {userId} - {shippingTypeId}");
                }

                //ShippingType ShippingType = await _dbContext.ShippingTypes.FirstOrDefaultAsync(s => s.ShippingTypeId == shippingTypeId);
                var cacheKey = $"shipping-type:{shippingTypeId}";

                ShippingType? shippingType = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow =
                            TimeSpan.FromMinutes(30);

                        return await _dbContext.ShippingTypes
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                s => s.ShippingTypeId == shippingTypeId);
                    });


                if (shippingType == null)
                {
                    return BadRequest("Shipping Type not found.");
                }

                Order order = new Order
                {
                    CartId = cart.CartId,
                    UserId = userId,
                    OrderStatus = "Pending",
                    CreatedAt = DateTimeOffset.Now,
                    TotalCost = cart.TotalPrice + shippingType.ShippingCost
                };
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                ShippingRecords newShippingRecord = new ShippingRecords
                {
                    OrderId = order.OrderId,
                    ShippingTypeId = shippingType.ShippingTypeId,
                    ShippingAddress = "Test Address from payment controller"
                };

                _dbContext.ShippingRecords.Add(newShippingRecord);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok($"Order {order.OrderId} has benn confermed.");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                //Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }


        [Authorize]
        [HttpGet("getOrders")]
        public async Task<IActionResult> getOrders()
        {

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                //var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                //var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (userIdClaim == null)
                {
                    return Unauthorized("Missing data in the token.");
                }

                var orders = await _dbContext.Orders.AsNoTracking().Where(c => c.UserId == int.Parse(userIdClaim.Value)).ToListAsync();

                return Ok(orders);

            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }


        [Authorize]
        [HttpGet("getUserUnpaidOrder")]
        public async Task<IActionResult> getUserUnpaidOrder()
        {

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                //var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                //var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (userIdClaim == null)
                {
                    return Unauthorized("Missing data in the token.");
                }

                var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == int.Parse(userIdClaim.Value) && c.OrderStatus == "Pending");

                return Ok(order);

            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }

    }
}
