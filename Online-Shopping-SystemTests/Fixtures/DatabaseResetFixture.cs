// Fixtures/DatabaseResetFixture.cs
using Microsoft.Data.SqlClient;
using Respawn;
using Xunit;

public class DatabaseResetFixture : IAsyncLifetime
{
    private readonly ShoppingWebAppFactory _factory;
    private Respawner _respawner = default!;
    private SqlConnection _connection = default!;

    public DatabaseResetFixture(ShoppingWebAppFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _connection = new SqlConnection(_factory.ConnectionString);
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new[] { new Respawn.Graph.Table("__EFMigrationsHistory") }
        });
    }

    public async Task ResetAsync() => await _respawner.ResetAsync(_connection);

    public Task DisposeAsync() => _connection.DisposeAsync().AsTask();
}