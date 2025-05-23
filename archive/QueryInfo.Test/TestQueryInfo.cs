using Microsoft.EntityFrameworkCore;
using QueryInfo.Models;
using QueryInfo.Test.Infrastructure;
using QueryInfo.Test.Infrastructure.Model;

namespace QueryInfo.Test
{
    public class TestQueryInfo : CoreUnitTest
    {

        [Fact]
        public void DynamicOrderToTypedOrder()
        {
            var order = new OrderInfo(nameof(Student.Name), OrderInfoDirections.Desc);
            var typedOrder = order.ToOrderInfo<Student>();

            Assert.NotNull(typedOrder);
        }
        
        [Fact]
        public void TestOrderGradId()
        {
            var queryInfo = new QueryInfo<Student>()
                .SetIncludeEntity(x => x.School)
                .SetIncludeList(x => x.Grades, x => x.SetFilter(f => f.Value == "B")
                    .SetOrder(g => g.Value, OrderInfoDirections.Desc))
                .SetWhere(x => x.Name == "Jane Roe");
                // .AddOrder(x => x.School!.Name, OrderInfoDirections.Asc);

            using var db = new AppDbContest();
            db.Database.EnsureCreated();
            db.Database.Migrate();


            var students = db.Students
                .Include(x => x.Grades.OrderByDescending(g => g.Value))
                .ToList();

            var query = db.Students.AsQueryable();
            query = queryInfo.ToQueryable(query);

            var student = query.First();

            Assert.NotNull(student);
            Assert.Equal("Jane Roe", student.Name);
            Assert.NotNull(student.School);
            Assert.NotNull(student.Grades);
            Assert.True(student.Grades?.Count > 0);
        }
        
        [Fact]
        public void IncludeTenIncludeEntity()
        {
            var queryInfo = new QueryInfo<Student>();

            queryInfo.IncludeList(x => x.Grades)
                .ThenIncludeEntity(x => x.SchoolClass);
                

            using var db = new AppDbContest();
            db.Database.EnsureCreated();
            db.Database.Migrate();

            var query = db.Students.AsQueryable();
            query = queryInfo.ToQueryable(query);

            var student = query.First();

            Assert.NotNull(student?.Grades?.FirstOrDefault()?.SchoolClass);
        }
        
        public void IncludeTenIncludeList()
        {
            var queryInfo = new QueryInfo<School>();

            queryInfo.IncludeList(x => x.Students)
                .ThenIncludeList(x => x.Grades);
                

            using var db = new AppDbContest();
            db.Database.EnsureCreated();
            db.Database.Migrate();

            var query = db.Schools.AsQueryable();
            query = queryInfo.ToQueryable(query);

            var school = query.First();

            Assert.NotNull(school?.Students?.FirstOrDefault()?.Grades);
        }
    }
}
