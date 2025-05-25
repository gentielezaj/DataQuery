using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Example.Infrastructure.Models;
using shlabs.DataQuery.Example.Infrastructure.Config;

namespace shlabs.DataQuery.Example.Infrastructure;

public class AppDbContext(string dbPath) : DbContext
{
    public DbSet<School> Schools { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<SchoolClass> SchoolClasses { get; set; }
    public DbSet<Grade> Grades { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = $"Data Source={dbPath}";
        // Use SQLite for testing
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AddDataToTable.AddDataToTableD(modelBuilder);
    }
}