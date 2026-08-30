// Fixtures/ShoppingWebAppFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Online_Shopping_System.Data;
using Testcontainers.MsSql;
using Xunit;

public class ShoppingWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithPassword("YourStrong!Passw0rd")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // override appsettings connection string + JWT settings for test consistency
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OnlineShoppingContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<OnlineShoppingContext>(options =>
                options.UseSqlServer(ConnectionString));

            // Example: swap EmailService for a fake so tests don't hit Mailtrap
            var emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(EmailService));
            if (emailDescriptor != null) services.Remove(emailDescriptor);
            services.AddHttpClient<EmailService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OnlineShoppingContext>();
        await db.Database.MigrateAsync(); // applies your EF Core migrations
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}