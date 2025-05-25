// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using shabs.DataQuery.Abstractions;
using shlabs.DataQuery.Example.Infrastructure;
using shlabs.DataQuery.Example.Infrastructure.Models;

Console.WriteLine("Hello, World!");

SetUp.SetupDatabase();
using var db = new AppDbContext(SetUp.DbPath);

// db.Database.EnsureCreated();
db.Database.Migrate();

var queryBuilder = new QueryBuilder<School>();

queryBuilder.IncludeList(x => x.Students)
    .ThenIncludeList(x => x.Grades);


var query = db.Schools.AsQueryable();
query = queryBuilder.ToQueryable(query);

var students = query.ToList();

Console.ReadKey();
