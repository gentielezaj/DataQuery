using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Example.Infrastructure;

namespace shlabs.DataQuery.Test;

public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public DatabaseFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateDbContext();
        context.Database.EnsureCreated();
    }

    // Each test can create a fresh context while sharing the same open in-memory database.
    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose()
    {
        _connection.Dispose();
    }
}
