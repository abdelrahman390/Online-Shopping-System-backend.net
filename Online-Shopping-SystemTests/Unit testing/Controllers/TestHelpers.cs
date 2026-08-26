using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Data;

namespace Online_Shopping_System.Tests
{
    /// <summary>
    /// Wraps a SQLite in-memory connection + DbContext.
    /// Keep the connection open for the lifetime of the test, then Dispose
    /// this object to tear the in-memory database down.
    /// </summary>
    public sealed class SqliteContextFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        public OnlineShoppingContext Context { get; }

        public SqliteContextFixture()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<OnlineShoppingContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new OnlineShoppingContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }

    public static class ControllerTestHelpers
    {
        /// <summary>
        /// Attaches a fake authenticated HttpContext (claims + remote IP)
        /// to a controller so `User.FindFirst(...)` and
        /// `HttpContext.Connection.RemoteIpAddress` work in the action.
        /// </summary>
        public static void AuthenticateAs(this ControllerBase controller, int userId, string role)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };
            httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Simulates a request with no auth claims at all, to hit the
        /// "Missing data in the token." branches.
        /// </summary>
        public static void WithNoAuthClaims(this ControllerBase controller)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
            };
            httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
