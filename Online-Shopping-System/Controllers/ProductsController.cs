using Microsoft.AspNetCore.Mvc;
using Online_Shopping_System.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Shopping_System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.Design;
using System.Data;
using System.Security.Claims;
//using Online_Shopping_System.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Net;


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

        [HttpGet("getProducts")]
        public IActionResult getProducts()
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

                List<Product> Companies = _dbContext.Products.ToList();

                //_AuditLogsService.Log(int.Parse(userIdClaim.Value), "getCompanies", "Companies", DateTime.Now, userIpAdress);

                //Console.WriteLine($"Test from AuditLogs second: {testVal}");

                return Ok(Companies);

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
