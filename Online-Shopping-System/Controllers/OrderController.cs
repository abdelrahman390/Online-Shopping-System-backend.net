using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Shipping;
using System.Security.Claims;

namespace Online_Shopping_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;

        public OrderController(IConfiguration configuration, OnlineShoppingContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }


        [Authorize]
        [HttpPost("confirmOrder")]
        public async Task<IActionResult> ConfirmOrder(int shippingTypeId)
        {
            /*
             info: Online-Shopping-System[0]
              POST /Order/confirmOrder responded 200 in 171 ms
             --
            info: Online-Shopping-System[0]
            POST /Order/confirmOrder responded 500 in 392 ms

            ------- after --------
            info: Online-Shopping-System[0]
              POST /Order/confirmOrder responded 500 in 541 ms

             */
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


                ShippingType ShippingType = await _dbContext.ShippingTypes.FirstOrDefaultAsync(s => s.ShippingTypeId == shippingTypeId);

                if (ShippingType == null)
                {
                    return BadRequest("Shipping Type not found.");
                }

                Order order = new Order
                {
                    CartId = cart.CartId,
                    UserId = userId,
                    OrderStatus = "Pending",
                    CreatedAt = DateTimeOffset.Now,
                    TotalCost = cart.TotalPrice + ShippingType.ShippingCost
                };
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                ShippingRecords newShippingRecord = new ShippingRecords
                {
                    OrderId = order.OrderId,
                    ShippingTypeId = ShippingType.ShippingTypeId,
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

                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(
                    500,
                    $"Error: {ex.Message}"
                );
            }
        }


        [Authorize]
        [HttpGet("getOrders")]
        public IActionResult getOrders()
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

                var orders = _dbContext.Orders.Where(c => c.UserId == int.Parse(userIdClaim.Value)).ToList();

                return Ok(orders);

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


        [Authorize]
        [HttpGet("getUserUnpaidOrder")]
        public IActionResult getUserUnpaidOrder()
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

                var order = _dbContext.Orders.FirstOrDefault(c => c.UserId == int.Parse(userIdClaim.Value) && c.OrderStatus == "Pending");

                return Ok(order);

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
