using shlabs.DataQuery.Example.Infrastructure;

namespace shlabs.DataQuery.Test;

public abstract class CoreUnitTest : IClassFixture<DatabaseFixture>
{
    public AppDbContext DbContext { get; }
    protected CoreUnitTest(DatabaseFixture fixture)
    {
        DbContext = fixture.DbContext;
    }
}