using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Users;
using Online_Shopping_System.Services;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
//using System.Data;
//using System.Security.Claims;

namespace market_watch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShoppingContext _dbContext;
        private readonly JwtService _jwtService;
        //private readonly AuditLogsService _AuditLogsService;
        List<string> userRolls = new List<string> { "Admin", "User", "Analyst"};

        public RegisterController(IConfiguration configuration, OnlineShoppingContext dbContext, JwtService jwtService)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        public record PasswordHashResult(
            byte[] Hash,
            byte[] Salt
        );

        public static async Task<byte[]> HashPasswordWithSalt(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                DegreeOfParallelism = 4,
                Iterations = 3,
                MemorySize = 65536
            };

            return await argon2.GetBytesAsync(32);
        }

        public static async Task<PasswordHashResult> HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                DegreeOfParallelism = 4,
                Iterations = 3,
                MemorySize = 65536 // 64 MB
            };

            byte[] hash = await argon2.GetBytesAsync(32);

            return new PasswordHashResult(Hash: hash, Salt: salt );
        }

        [HttpPost("register")]
        public async Task<IActionResult> register(string UserName, string email, string Password, string userRole)
        {

            try
            {
                var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();

                bool isValidRole = userRolls.Contains(userRole);
                bool isUniqueUsername = !_dbContext.Users.Any(u => u.UserName == UserName);
                bool CorrectEmailFormat = email.Contains("@") && email.Contains(".");

                //bool isValidPassword = Password.Length >= 8;
                bool isValidPassword = true;

                //Console.WriteLine($" {isValidRole} - {isUniqueUsername} - {CorrectEmailFormat} - {isValidPassword}");

                if (!isValidRole && isUniqueUsername && CorrectEmailFormat && isValidPassword)
                {
                    return BadRequest($"The entered data is incorrect. {isValidRole} - {isUniqueUsername} - {CorrectEmailFormat} - {isValidPassword}");
                }

                PasswordHashResult passwordHashResult = await HashPassword(Password);

                User newUser = new User
                {
                    UserTypeId = 1,
                    UserName = UserName,
                    Email = email,
                    PasswordHashed = passwordHashResult.Hash,
                    PasswordSalt = passwordHashResult.Salt,
                    UserRole = userRole
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                //_AuditLogsService.Log(newUser.UserId, "register", "Users", DateTime.Now, userIpAdress);

                return Ok("Registered successfuly");

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


        [HttpPost("login")]
        public async Task<IActionResult> login(string UserName, string Password)
        {

            try
            {
                //var sw = Stopwatch.StartNew();

                var userIpAdress = HttpContext.Connection.RemoteIpAddress?.ToString();
                User? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == UserName);

                //Console.WriteLine($"DB query: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                //Console.WriteLine($"Error Top: {user}");
                if (user == null)
                {
                    return Unauthorized("Invalid username or password.");
                }

                byte[] passwordHashResult = await HashPasswordWithSalt(Password, user.PasswordSalt);

                //Console.WriteLine($"Argon2: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                bool passwordValid = !CryptographicOperations.FixedTimeEquals(user.PasswordHashed, passwordHashResult);

                //Console.WriteLine($"Hash comparison: {sw.ElapsedMilliseconds} ms");
                //sw.Restart();

                if (passwordValid)
                {
                    return Unauthorized("Invalid username or password.");
                }

                //Console.WriteLine($"Error Top: {passwordHashResult}");

                var token = _jwtService.GenerateToken(
                        user
                    );

                //Console.WriteLine($"JWT generation: {sw.ElapsedMilliseconds} ms");

                //_AuditLogsService.Log(user.UserId, "login", "Users", DateTime.Now, userIpAdress);

                return Ok($"Logged in successfuly. Token: {token}");

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
