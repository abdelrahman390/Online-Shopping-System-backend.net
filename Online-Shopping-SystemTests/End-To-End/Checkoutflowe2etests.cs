using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Online_Shopping_System.Integration
{
    /// <summary>
    /// True end-to-end tests: every step goes through the real HTTP pipeline
    /// (register -> login -> add product -> add to cart -> confirm order -> pay),
    /// instead of seeding Cart/Order rows directly via DbContext like the
    /// narrower PaymentTests do. This exercises controller wiring, auth,
    /// validation and cross-controller state exactly as a real client would.
    ///
    /// ASSUMPTIONS (adjust if your models differ):
    /// - ShippingType has ShippingTypeId, ShippingTypeName, ShippingCost.
    /// - ASP.NET Core's default System.Text.Json camelCase policy is in effect,
    ///   so JSON property names below (e.g. "orderStatus") are camelCase.
    /// - Register/Login require an existing UserType row whose UserTypeName
    ///   matches the requested role ("Admin" / "User").
    /// </summary>
    public class CheckoutFlowE2ETests : IClassFixture<ShoppingWebAppFactory>, IAsyncLifetime
    {
        private readonly ShoppingWebAppFactory _factory;
        private readonly DatabaseResetFixture _dbReset;
        private HttpClient _client = default!;

        public CheckoutFlowE2ETests(ShoppingWebAppFactory factory)
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
            var db = scope.ServiceProvider.GetRequiredService<OnlineShoppingContext>();

            // Reference data that must exist before Register/checkout can work.
            db.UserTypes.AddRange(
                new UserType { UserTypeName = "Admin", Discount = 0 },
                new UserType { UserTypeName = "User", Discount = 0 }
            );

            db.ShippingTypes.Add(new ShippingType
            {
                ShippingName = "Standard",
                ShippingCost = 50m
            });

            await db.SaveChangesAsync();
        }

        public Task DisposeAsync() => _dbReset.DisposeAsync();

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private async Task<string> RegisterAndLoginAsync(string userName, string role)
        {
            var registerResponse = await _client.PostAsync(
                $"/Register/register?UserName={userName}&email={userName}@example.com" +
                $"&Password=Password123!&userRole={role}",
                null);

            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResponse = await _client.PostAsync(
                $"/Register/login?UserName={userName}&Password=Password123!",
                null);

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            return loginBody.GetProperty("token").GetString()!;
        }

        private void AuthenticateAs(string token) =>
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        // ---------------------------------------------------------------
        // Happy path: the full customer journey
        // ---------------------------------------------------------------

        [Fact]
        public async Task FullCheckoutJourney_RegisterThroughPayment_Succeeds()
        {
            // ---- Admin registers, logs in, creates a product ----
            var adminToken = await RegisterAndLoginAsync("AdminUser", "Admin");
            AuthenticateAs(adminToken);

            var addProductResponse = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                new
                {
                    Name = "Test Laptop",
                    Price = 500m,
                    Quantity = 10,
                    Type = "Electronics",
                    Warranty = 12
                });

            addProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var addProductBody = await addProductResponse.Content.ReadFromJsonAsync<JsonElement>();
            var productId = addProductBody.GetProperty("productId").GetInt32();

            // ---- Customer registers and logs in ----
            var userToken = await RegisterAndLoginAsync("ShopperUser", "User");
            AuthenticateAs(userToken);

            // ---- Customer adds the product to cart ----
            var addToCartResponse = await _client.PostAsync(
                $"/Cart/addToCart?productId={productId}&quantity=2",
                null);

            addToCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // ---- Customer previews the cart ----
            var previewResponse = await _client.GetAsync("/Cart/previewCart");
            previewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var previewItems = await previewResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
            previewItems.Should().ContainSingle();
            previewItems![0].GetProperty("quantity").GetInt32().Should().Be(2);
            previewItems[0].GetProperty("totalPrice").GetDecimal().Should().Be(1000m);

            // ---- Customer fetches shipping options ----
            var shippingTypesResponse = await _client.GetAsync("/Shipping/getShippingTypes");
            shippingTypesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var shippingTypes = await shippingTypesResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
            shippingTypes.Should().ContainSingle();
            var shippingTypeId = shippingTypes![0].GetProperty("shippingTypeId").GetInt32();

            // ---- Customer confirms the order ----
            var confirmOrderResponse = await _client.PostAsync(
                $"/Order/confirmOrder?shippingTypeId={shippingTypeId}",
                null);

            confirmOrderResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Sanity check: an unpaid order now exists for this user
            var unpaidOrderResponse = await _client.GetAsync("/Order/getUserUnpaidOrder");
            unpaidOrderResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var unpaidOrder = await unpaidOrderResponse.Content.ReadFromJsonAsync<JsonElement>();
            unpaidOrder.GetProperty("totalCost").GetDecimal().Should().Be(1050m); // 1000 cart + 50 shipping

            // ---- Customer pays by cash ----
            var payResponse = await _client.PostAsync("/Payment/cashPay", null);
            payResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // ---- Verify the confirmed order via the API ----
            var ordersResponse = await _client.GetAsync("/Order/getOrders");
            ordersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await ordersResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
            orders.Should().ContainSingle();
            orders![0].GetProperty("orderStatus").GetString().Should().Be("Confirmed");
            orders[0].GetProperty("paymentTypeName").GetString().Should().Be("Cash");
            orders[0].GetProperty("totalCost").GetDecimal().Should().Be(1050m);

            // ---- Verify side effects that only the DB can confirm ----
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<OnlineShoppingContext>();

            var product = await verifyDb.Products.AsNoTracking()
                .FirstAsync(p => p.ProductId == productId);
            product.Quantity.Should().Be(8); // 10 - 2 reserved at add-to-cart time

            var payment = await verifyDb.Payments.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Amount == 1050m);
            payment.Should().NotBeNull();
        }

        // ---------------------------------------------------------------
        // Negative end-to-end paths
        // ---------------------------------------------------------------

        [Fact]
        public async Task CheckoutJourney_WithoutAuthentication_StopsAtFirstAuthorizedStep()
        {
            // No login at all: every protected step in the journey must reject the caller.
            var addToCart = await _client.PostAsync("/Cart/addToCart?productId=1&quantity=1", null);
            addToCart.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var confirmOrder = await _client.PostAsync("/Order/confirmOrder?shippingTypeId=1", null);
            confirmOrder.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var pay = await _client.PostAsync("/Payment/cashPay", null);
            pay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task NonAdminUser_CannotAddProduct_JourneyBlockedAtCatalogStep()
        {
            var userToken = await RegisterAndLoginAsync("RegularShopper", "User");
            AuthenticateAs(userToken);

            var addProductResponse = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                new
                {
                    Name = "Unauthorized Product",
                    Price = 100m,
                    Quantity = 5,
                    Type = "Books",
                    Author = "Someone",
                    ISBN = "1234567890"
                });

            addProductResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CheckoutJourney_AddingMoreThanAvailableStock_FailsBeforeCheckout()
        {
            var adminToken = await RegisterAndLoginAsync("AdminUser2", "Admin");
            AuthenticateAs(adminToken);

            var addProductResponse = await _client.PostAsJsonAsync(
                "/Products/addProduct",
                new
                {
                    Name = "Limited Stock Item",
                    Price = 20m,
                    Quantity = 1,
                    Type = "Books",
                    Author = "Some Author",
                    ISBN = "0987654321"
                });

            addProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var addProductBody = await addProductResponse.Content.ReadFromJsonAsync<JsonElement>();
            var productId = addProductBody.GetProperty("productId").GetInt32();

            var userToken = await RegisterAndLoginAsync("StockTestShopper", "User");
            AuthenticateAs(userToken);

            var addToCartResponse = await _client.PostAsync(
                $"/Cart/addToCart?productId={productId}&quantity=5", // only 1 in stock
                null);

            addToCartResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // No cart/order should exist, so checkout has nothing to confirm.
            var confirmOrderResponse = await _client.PostAsync(
                "/Order/confirmOrder?shippingTypeId=1",
                null);

            confirmOrderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}