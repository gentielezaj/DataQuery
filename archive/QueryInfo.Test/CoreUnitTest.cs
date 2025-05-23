using Microsoft.EntityFrameworkCore;
using QueryInfo.Test.Infrastructure;

namespace QueryInfo.Test
{
    public class CoreUnitTest
    {
        // Use a SQLite connection string pointing to a file in the project directory
        private const string DB_PATH = @"E:\github\QueryInfo\QueryInfo.db";

        protected AppDbContest Db()
        {
            var connactionString = $"Data Source={DB_PATH};";
            var dbContext = new AppDbContest();
            dbContext.Database.Migrate();
            return dbContext;
        }
    }
}
