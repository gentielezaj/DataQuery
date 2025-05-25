using Microsoft.EntityFrameworkCore;

namespace shlabs.DataQuery.Example.Infrastructure;

public class SetUp
{
    public const string DbPath = "E:\\db\\QueryBuilder.db";

    public static void SetupDatabase()
    {
        if (!File.Exists(DbPath))
        {
            File.Create(DbPath).Dispose();
            
            var directory = Path.GetDirectoryName(DbPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        var db = new AppDbContext(DbPath);
        // db.Database.EnsureCreated();
        db.Database.Migrate();
    }

    public static AppDbContext GetDbContext()
    {
        return new AppDbContext(DbPath);
    }
}