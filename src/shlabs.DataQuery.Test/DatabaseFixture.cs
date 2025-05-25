using shlabs.DataQuery.Example.Infrastructure;

namespace shlabs.DataQuery.Test;

public class DatabaseFixture : IDisposable
{
    public AppDbContext DbContext { get; }

    public DatabaseFixture()
    {
        SetUp.SetupDatabase();
        DbContext = SetUp.GetDbContext();
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}