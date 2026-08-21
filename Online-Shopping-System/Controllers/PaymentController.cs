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

        public PaymentController(IConfiguration configuration, OnlineShoppingContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet("getPaymentTypes")]
        public IActionResult GetPaymentTypesPayOrder()
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

                //var paymentType = _dbContext.PaymentTypes.ToList();

                return Ok("test");
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


        [HttpPost("cashPay")]
        public IActionResult cashPay(int userId, int ShippingTypeId)
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

                if (cart == null)
                {
                    return BadRequest("Cart not found.");
                }

                ShippingType ShippingType = _dbContext.ShippingTypes.FirstOrDefault(s => s.ShippingTypeId == ShippingTypeId);

                if(ShippingType == null)
                {
                    return BadRequest("Shipping Type not found.");
                }

                Order order = new Order
                {
                    CartId = cart.CartId,
                    UserId = userId,
                    OrderStatus = "InProgress",
                    TotalCost = cart.TotalPrice + ShippingType.ShippingCost
                };
                _dbContext.Orders.Add(order);

                cart.CartStatus = "Confirmed";

                _dbContext.SaveChanges();

                Payment payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalCost,
                    Date = DateTimeOffset.Now,
                    PaymentTypeName = "Cash"
                };
                _dbContext.Payments.Add(payment);

                ShippingRecords newShippingRecord = new ShippingRecords
                {
                    OrderId = order.OrderId,
                    ShippingTypeId = ShippingType.ShippingTypeId,
                    ShippingAddress = "Test Address from payment controller"
                };

                _dbContext.ShippingRecords.Add(newShippingRecord);

                _dbContext.SaveChanges();

                return Ok(
                    $"OrderId: {order.OrderId} is now Confirmed; CartId: {cart.CartId}, with ShippingRecordId: {newShippingRecord} is now added."
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
        public IActionResult CreditCardPay(int userId, int ShippingTypeId, string cardNumber)
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

                if (cart == null)
                {
                    return BadRequest("Cart not found.");
                }

                ShippingType ShippingType = _dbContext.ShippingTypes.FirstOrDefault(s => s.ShippingTypeId == ShippingTypeId);

                if (ShippingType == null)
                {
                    return BadRequest("Shipping Type not found.");
                }

                CreditCardPayment creditCardPayment = new CreditCardPayment
                {
                    CardNumber = cardNumber
                };

                if (creditCardPayment.CollectMoney() == false)
                {
                    return BadRequest("Credit Card data is incorrect.");
                }

                Order order = new Order
                {
                    CartId = cart.CartId,
                    UserId = userId,
                    OrderStatus = "InProgress",
                    TotalCost = cart.TotalPrice + ShippingType.ShippingCost
                };
                _dbContext.Orders.Add(order);

                cart.CartStatus = "Confirmed";

                _dbContext.SaveChanges();

                Payment payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalCost,
                    Date = DateTimeOffset.Now,
                    PaymentTypeName = "CreditCard"
                };
                _dbContext.Payments.Add(payment);
                _dbContext.SaveChanges();

                creditCardPayment.PaymentId = payment.PaymentId;

                _dbContext.CreditCardPayments.Add(creditCardPayment);

                ShippingRecords newShippingRecord = new ShippingRecords
                {
                    OrderId = order.OrderId,
                    ShippingTypeId = ShippingType.ShippingTypeId,
                    ShippingAddress = "Test Address from payment controller"
                };
                _dbContext.ShippingRecords.Add(newShippingRecord);

                _dbContext.SaveChanges();

                return Ok(
                    $"OrderId: {order.OrderId} is now Confirmed; CartId: {cart.CartId}, with ShippingRecordId: {newShippingRecord} is now added."
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
        public IActionResult WalletPay(int userId, int ShippingTypeId, string WalletNumber, string WalletName)
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

                if (cart == null)
                {
                    return BadRequest("Cart not found.");
                }

                ShippingType ShippingType = _dbContext.ShippingTypes.FirstOrDefault(s => s.ShippingTypeId == ShippingTypeId);

                if (ShippingType == null)
                {
                    return BadRequest("Shipping Type not found.");
                }

                WalletPayment walletPayment = new WalletPayment
                {
                    WalletName = WalletName,
                    WalletNumber = WalletNumber
                };

                if (walletPayment.CollectMoney() == false)
                {
                    return BadRequest("Wallet data is incorrect.");
                }

                Order order = new Order
                {
                    CartId = cart.CartId,
                    UserId = userId,
                    OrderStatus = "InProgress",
                    TotalCost = cart.TotalPrice + ShippingType.ShippingCost
                };
                _dbContext.Orders.Add(order);

                cart.CartStatus = "Confirmed";

                _dbContext.SaveChanges();

                Payment payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalCost,
                    Date = DateTimeOffset.Now,
                    PaymentTypeName = "Wallet"
                };
                _dbContext.Payments.Add(payment);
                _dbContext.SaveChanges();

                walletPayment.PaymentId = payment.PaymentId;

                _dbContext.WalletPayments.Add(walletPayment);

                ShippingRecords newShippingRecord = new ShippingRecords
                {
                    OrderId = order.OrderId,
                    ShippingTypeId = ShippingType.ShippingTypeId,
                    ShippingAddress = "Test Address from payment controller"
                };
                _dbContext.ShippingRecords.Add(newShippingRecord);

                _dbContext.SaveChanges();

                return Ok(
                    $"OrderId: {order.OrderId} is now Confirmed; CartId: {cart.CartId}, with ShippingRecordId: {newShippingRecord} is now added."
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
