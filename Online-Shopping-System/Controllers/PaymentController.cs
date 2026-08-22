using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Payment;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Shipping;
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


        [HttpPost("cashPay")]
        public async Task<IActionResult> CashPay(int userId)
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

                Order order = _dbContext.Orders.FirstOrDefault(o => o.UserId == userId && o.OrderStatus == "Pending");

                if (order == null)
                {
                    return BadRequest($"Order not found. {userId}");
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

                Cart cart = _dbContext.Carts.FirstOrDefault(c => c.CartId == order.CartId && c.CartStatus == "Pending");

                if(cart == null)
                {
                    return BadRequest("Cart not found.");
                }

                order.OrderStatus = "Confirmed";
                cart.CartStatus = "Confirmed";

                _dbContext.Payments.Add(cashpayment);

                _dbContext.SaveChanges();

                await _emailService.SendEmailAsync(
                        "abdelrahmanbo390@gmail.com",
                        "Order Confirmation.",
                        "Hello from Online-Shopping-System, your order has benn confirmed."
                    );

                return Ok(
                    $"OrderId: {order.OrderId} is now Paid and confirmed."
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

        [HttpPost("creditCardPay")]
        public IActionResult CreditCardPay(int userId, string cardNumber)
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

                Order order = _dbContext.Orders.FirstOrDefault(o => o.UserId == userId && o.OrderStatus == "Pending");

                if (order == null)
                {
                    return BadRequest("Order not found.");
                }

                Payment creditCardPayment = new CreditCardPayment
                {
                    PaymentType = "Visa",
                    CardNumber = cardNumber,
                };

                if (creditCardPayment.CollectMoney() == false)
                {
                    return BadRequest("Credit Card data is incorrect.");
                }

                creditCardPayment.OrderId = order.OrderId;
                creditCardPayment.Amount = order.TotalCost;
                creditCardPayment.Date = DateTimeOffset.Now;

                order.OrderStatus = "Confirmed";

                _dbContext.Payments.Add(creditCardPayment);

                _dbContext.SaveChanges();

                return Ok(
                    $"OrderId: {order.OrderId} is now Paid and confirmed."
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

        [HttpPost("WalletPay")]
        public IActionResult WalletPay(int userId, string WalletNumber, string WalletProviderName)
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

                Order order = _dbContext.Orders.FirstOrDefault(o => o.UserId == userId && o.OrderStatus == "Pending");

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

                _dbContext.SaveChanges();

                return Ok(
                    $"OrderId: {order.OrderId} is now Paid and confirmed."
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
