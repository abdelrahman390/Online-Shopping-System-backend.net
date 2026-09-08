using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Payment;
using Online_Shopping_System.Models.Users;
using System.Security.Claims;
//using System.ComponentModel.Design;
//using System.Data;
//using System.Linq;
//using System.Net;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using Online_Shopping_System.Services;


namespace Online_Shopping_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;
        private readonly EmailService _emailService;

        public PaymentController(IConfiguration configuration, OnlineShoppingContext dbContext, EmailService emailService)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        [Authorize]
        [HttpPost("cashPay")]
        public async Task<IActionResult> CashPay()
        {
            /*
             info: Online-Shopping-System[0]
             POST /Order/confirmOrder responded 200 in 171 ms
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

                Order order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == int.Parse(userIdClaim.Value) && o.OrderStatus == "Pending");

                if (order == null)
                {
                    return BadRequest($"Order not found. {int.Parse(userIdClaim.Value)}");
                }

                Payment cashpayment = new CashPayment
                {
                    PaymentType = "CashPayment",
                    OrderId = order.OrderId,
                    Amount = order.TotalCost,
                    Date = DateTimeOffset.Now
                };

                if (cashpayment.CollectMoney() == false)
                {
                    return BadRequest("Cash Payment data is incorrect.");
                }

                Cart cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == order.CartId && c.CartStatus == "Pending");

                if(cart == null)
                {
                    return BadRequest("Cart not found.");
                }

                order.OrderStatus = "Confirmed";
                order.PaymentTypeName = "Cash";
                cart.CartStatus = "Confirmed";

                _dbContext.Payments.Add(cashpayment);

                await _dbContext.SaveChangesAsync();

                //User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == int.Parse(userIdClaim.Value));

                //await _emailService.SendEmailAsync(
                //        "abdelrahmanbo390@gmail.com",
                //        "Order Confirmation.",
                //        "Hello from Online-Shopping-System, your order has benn confirmed."
                //    );

                await transaction.CommitAsync();
                return Ok(
                    $"OrderId: {order.OrderId} is now Paid and confirmed."
                );
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
        [HttpPost("creditCardPay")]
        public async Task<IActionResult> CreditCardPay(string cardNumber)
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

                Order order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == int.Parse(userIdClaim.Value) && o.OrderStatus == "Pending");

                if (order == null)
                {
                    return BadRequest("Order not found.");
                }

                Payment creditCardPayment = new CreditCardPayment
                {
                    PaymentType = "CreditCard",
                    CardNumber = cardNumber,
                };

                if (creditCardPayment.CollectMoney() == false)
                {
                    return BadRequest("Credit Card data is incorrect.");
                }

                creditCardPayment.OrderId = order.OrderId;
                creditCardPayment.Amount = order.TotalCost;
                creditCardPayment.Date = DateTimeOffset.Now;

                Cart cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == order.CartId && c.CartStatus == "Pending");

                order.OrderStatus = "Confirmed";
                order.PaymentTypeName = "CreditCard";
                cart.CartStatus = "Confirmed";

                _dbContext.Payments.Add(creditCardPayment);

                await _dbContext.SaveChangesAsync();

                User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == int.Parse(userIdClaim.Value));

                //Console.WriteLine($"Test: {cardNumber}");

                //Console.WriteLine("Before sending email");

                //await _emailService.SendEmailAsync(
                //    "abdelrahmanbo390@gmail.com",
                //    "Order Confirmation.",
                //    "Hello from Online-Shopping-System, your order has benn confirmed."
                //);

                //Console.WriteLine("After sending email");

                await transaction.CommitAsync();

                return Ok(
                    $"OrderId: {order.OrderId} is now Paid and confirmed."
                );
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
        [HttpPost("WalletPay")]
        public async Task<IActionResult> WalletPay(string WalletNumber, string WalletProviderName)
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

                Order order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.UserId == int.Parse(userIdClaim.Value) && o.OrderStatus == "Pending");

                if (order == null)
                {
                    return BadRequest("Order not found.");
                }

                Payment walletPayment = new WalletPayment
                {
                    PaymentType = "WalletPayment",
                    WalletProviderName = WalletProviderName,
                    WalletNumber = WalletNumber
                };

                if (walletPayment.CollectMoney() == false)
                {
                    return BadRequest("Wallet data is incorrect.");
                }


                order.OrderStatus = "Confirmed";

                walletPayment.OrderId = order.OrderId;
                walletPayment.Amount = order.TotalCost;
                walletPayment.Date = DateTimeOffset.Now;

                _dbContext.Payments.Add(walletPayment);

                Cart cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == order.CartId && c.CartStatus == "Pending");

                order.OrderStatus = "Confirmed";
                order.PaymentTypeName = "Wallet";
                cart.CartStatus = "Confirmed";

                await _dbContext.SaveChangesAsync();

                User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == int.Parse(userIdClaim.Value));

                //await _emailService.SendEmailAsync(
                //    "abdelrahmanbo390@gmail.com",
                //    "Order Confirmation.",
                //    "Hello from Online-Shopping-System, your order has benn confirmed."
                //);

                await transaction.CommitAsync();

                return Ok(
                    $"OrderId: {order.OrderId} is now Paid and confirmed."
                );
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

    }
}
