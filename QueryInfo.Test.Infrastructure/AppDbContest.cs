using Microsoft.EntityFrameworkCore;
using QueryInfo.Test.Infrastructure.Config;
using QueryInfo.Test.Infrastructure.Model;

namespace QueryInfo.Test.Infrastructure
{
    public class AppDbContest : DbContext
    {
        public DbSet<School> Schools { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<SchoolClass> SchoolClasses { get; set; }
        public DbSet<Grade> Grades { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Data Source=E:\github\QueryInfo\QueryInfo.db";
            // Use SQLite for testing
            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddDataToTable.AddDataToTableD(modelBuilder);
        }
    }
}