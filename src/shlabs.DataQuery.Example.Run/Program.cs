// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using QueryInfo.Test.Infrastructure.Model;
using shabs.DataQuery.Abstractions;
using shlabs.DataQuery.Example.Infrastructure;

Console.WriteLine("Hello, World!");

var dbPath = "E:\\db\\QueryBuilder.db";

var db = new AppDbContext(dbPath);

Directory.CreateDirectory("E:\\db");

// db.Database.EnsureCreated();
db.Database.Migrate();

var queryBuilder = new QueryBuilder<School>();

queryBuilder.IncludeList(x => x.Students)
    .ThenIncludeList(x => x.Grades);


var query = db.Schools.AsQueryable();
query = queryBuilder.ToQueryable(query);

var students = query.ToList();

Console.ReadKey();
