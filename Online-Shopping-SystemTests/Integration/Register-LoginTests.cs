using FluentAssertions;
using market_watch.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Users;
using System.Net;
using Xunit;

namespace Online_Shopping_SystemTests.Integration
{
    public class RegisterTests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;

    private HttpClient _client = default!;

        public RegisterTests(ShoppingWebAppFactory factory)
        {
            _factory = factory;
            _dbReset = new DatabaseResetFixture(factory);
        }

        public async Task InitializeAsync()
        {
            await _dbReset.InitializeAsync();
            await _dbReset.ResetAsync();

            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var userType = new UserType
            {
                UserTypeName = "Normal",
                Discount = 0
            };

            db.UserTypes.Add(userType);

            await db.SaveChangesAsync();
        }

        public Task DisposeAsync()
            => _dbReset.DisposeAsync();

        // =========================================================
        // REGISTER
        // =========================================================

        [Fact]
        public async Task Register_WithValidData_CreatesUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            var userType = new UserType
            {
                UserTypeName = "User",
                Discount = 0
            };

            db.UserTypes.Add(userType);

            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Register/register?UserName=testuser&email=test@example.com&Password=Password123&userRole=User",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            //using var scope = _factory.Services.CreateScope();

            //var db = scope.ServiceProvider
            //    .GetRequiredService<OnlineShoppingContext>();

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.UserName == "testuser");

            user.Should().NotBeNull();
            user!.Email.Should().Be("test@example.com");
            user.UserRole.Should().Be("User");

            user.PasswordHashed.Should().NotBeEmpty();
            user.PasswordSalt.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync(
                "/Register/register?UserName=testuser&email=test@example.com&Password=Password123&userRole=InvalidRole",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync(
                "/Register/register?UserName=testuser&email=invalid-email&Password=Password123&userRole=User",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<OnlineShoppingContext>();

            db.UserTypes.Add(new UserType
            {
                UserTypeName = "User",
                Discount = 0
            });

            db.Users.Add(new User
            {
                UserTypeId = db.UserTypes.First().UserTypeId,
                UserName = "existinguser",
                Email = "existing@example.com",
                PasswordHashed = [],
                PasswordSalt = [],
                UserRole = "User"
            });

            await db.SaveChangesAsync();

            // Act
            var response = await _client.PostAsync(
                "/Register/register?UserName=existinguser&email=new@example.com&Password=Password123&userRole=User",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // LOGIN
        // =========================================================

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOkAndToken()
        {
            // Arrange
            var password = "Password123";

            var passwordResult =
                await RegisterController.HashPassword(password);


            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<OnlineShoppingContext>();

                var userType = new UserType
                {
                    UserTypeName = "Normal",
                    Discount = 0
                };

                db.UserTypes.Add(userType);
                await db.SaveChangesAsync();

                db.Users.Add(new User
                {
                    UserTypeId = db.UserTypes.First().UserTypeId,
                    UserName = "loginuser",
                    Email = "login@example.com",
                    PasswordHashed = passwordResult.Hash,
                    PasswordSalt = passwordResult.Salt,
                    UserRole = "User"
                });

                await db.SaveChangesAsync();
            }

            // Act
            var response = await _client.PostAsync(
                "/Register/login?UserName=loginuser&Password=Password123",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Contain("Logged in successfully");
            body.Should().Contain("token");
        }

        [Fact]
        public async Task Login_WithNonExistingUser_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PostAsync(
                "/Register/login?UserName=doesnotexist&Password=Password123",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var passwordResult =
                await RegisterController.HashPassword("CorrectPassword");

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<OnlineShoppingContext>();

                var userType = new UserType
                {
                    UserTypeName = "User",
                    Discount = 0
                };

                db.UserTypes.Add(userType);
                await db.SaveChangesAsync();

                db.Users.Add(new User
                {
                    UserTypeId = userType.UserTypeId,
                    UserName = "loginuser",
                    Email = "login@example.com",
                    PasswordHashed = passwordResult.Hash,
                    PasswordSalt = passwordResult.Salt,
                    UserRole = "User"
                });

                await db.SaveChangesAsync();
            }

            // Act
            var response = await _client.PostAsync(
                "/Register/login?UserName=loginuser&Password=WrongPassword",
                null);

            // Assert
            response.StatusCode.Should()
                .Be(HttpStatusCode.Unauthorized);
        }
    }

}
