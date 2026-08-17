using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Example.Infrastructure.Config;
using shlabs.DataQuery.Example.Infrastructure.Models;

namespace shlabs.DataQuery.Example.Infrastructure;

public class AppDbContext : DbContext
{
    private readonly string? _dbPath;

    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<School> Schools { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<SchoolClass> SchoolClasses { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Grade> Grades { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AddDataToTable.AddDataToTableD(modelBuilder);
    }
}
