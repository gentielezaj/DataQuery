
using Microsoft.EntityFrameworkCore;
using QueryInfo.Test.Infrastructure;

Console.WriteLine("Hello world");

using var db = new AppDbContest();
//db.Database.Migrate();


var students = db.Students.ToList();

Console.WriteLine(students.Count);

Console.WriteLine("Press any key to close");
Console.ReadKey();