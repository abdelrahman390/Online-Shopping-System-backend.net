using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class ShippingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;

        public ShippingController(IConfiguration configuration, OnlineShoppingContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }


        [HttpGet("getShippingTypes")]
        public IActionResult GetShippingTypes()
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

                var cart = _dbContext.ShippingTypes.ToList();

                return Ok(
                    cart
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
