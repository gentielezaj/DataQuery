using Microsoft.EntityFrameworkCore;
using QueryInfo.Models;
using QueryInfo.Test.Infrastructure;
using QueryInfo.Test.Infrastructure.Model;

namespace QueryInfo.Test
{
    public class TestQueryInfo : CoreUnitTest
    {
        [Fact]
        public void TestQueryInfo1()
        {
            var queryInfo = new QueryInfo<Student>()
                .SetIncludeEntity(x => x.School)
                .SetIncludeList(x => x.Grades)
                .SetWhere(x => x.Name == "Jane Roe")
                .AddOrder(x => x.Id, OrderInfoDirections.Asc);

            using var db = new AppDbContest();
            db.Database.EnsureCreated();
            db.Database.Migrate();

            var query = db.Students.AsQueryable();
            query = queryInfo.ToQueryable(query);

            var student = query.First();

            Assert.NotNull(student);
            Assert.Equal("Jane Roe", student.Name);
            Assert.NotNull(student.School);
            Assert.NotNull(student.Grades);
            Assert.True(student.Grades?.Count > 0);
        }
    }
}
