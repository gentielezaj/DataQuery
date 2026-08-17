# shlabs.DataQuery

`shlabs.DataQuery` is a .NET query-builder library for describing an Entity
Framework Core query independently from its database source. Build filters,
ordering, paging, and navigation-property includes as a
`QueryBuilder<TEntity>`; later, apply that description to a `DbContext`,
`DbSet<TEntity>`, or existing `IQueryable<TEntity>`.

This is useful when a query is assembled outside the data-access layer—for
example from API request parameters, application rules, or reusable query
specifications—without passing a `DbContext` through that code.

## Packages

| Package/project | Use it when you need |
| --- | --- |
| `shlabs.DataQuery.Abstractions` | The provider-independent `QueryBuilder<TEntity>` model and its typed or dynamic filtering APIs. It has no EF Core dependency. |
| `shlabs.DataQuery` | EF Core translation extensions, including `ToQueryable`. This references `Abstractions` and Microsoft.EntityFrameworkCore. |

In an EF Core application, reference `shlabs.DataQuery`. Reference only
`shlabs.DataQuery.Abstractions` in layers that must not depend on EF Core.

## Install

Add the projects as project references while developing this repository, or
install the matching NuGet packages in an application:

```powershell
dotnet add package shlabs.DataQuery
```

The projects target .NET 8. Import the namespaces below where needed:

```csharp
using shlabs.DataQuery.Abstractions;
using Microsoft.EntityFrameworkCore;
```

`ToQueryable` is exposed as an extension method in `System.Linq`, which is
normally available through implicit usings in a modern .NET project.

## The query lifecycle

```text
Build QueryBuilder<TEntity>  →  apply it to a query source  →  execute with EF Core
     no database needed           ToQueryable(...)               ToListAsync(), CountAsync(), ...
```

`QueryBuilder<TEntity>` stores expressions and metadata; it is not an
`IQueryable<TEntity>` and it never executes a database query. `ToQueryable`
returns an `IQueryable<TEntity>`, so SQL is still deferred until the caller
uses an EF Core terminal operation.

## Quick start

Create a typed query description first:

```csharp
var builder = new QueryBuilder<School>()
    .AppendFilter(s => s.Name.StartsWith("North"))
    .AddOrderBy(s => s.Name, QueryOrderDirections.Asc)
    .SetSkip(20)
    .SetTake(10)
    .AddInclude(s => s.SchoolClasses);
```

Then apply it when a database source is available:

```csharp
IQueryable<School> query = builder.ToQueryable(dbContext);
List<School> schools = await query.ToListAsync();
```

The translator applies root-level operations in this sequence:

1. filter (`Where`)
2. ordering (`OrderBy` and `ThenBy`)
3. skip
4. take
5. includes

For stable paging, always supply a deterministic order before calling
`SetSkip` or `SetTake`.

## Applying a builder to a source

There are three overloads. Choose the one that matches where your query starts:

```csharp
// Starts from dbContext.Set<School>().
var fromContext = builder.ToQueryable(dbContext);

// Starts from a particular DbSet.
var fromSet = builder.ToQueryable(dbContext.Schools);

// Preserves restrictions already placed on an existing query.
var namedSchools = dbContext.Schools.Where(s => s.Name.StartsWith("North"));
var composed = builder.ToQueryable(namedSchools);
```

Use the existing-`IQueryable` overload when the application has already added
tenant scoping, soft-delete restrictions, authorization rules, or another
provider-translatable query constraint. `ToQueryable` composes onto that query;
it does not replace it.

## Filters

### Set one filter

`SetFilter` replaces the builder's current filter:

```csharp
var builder = new QueryBuilder<Student>()
    .SetFilter(student => student.SchoolId == 7 && student.Name.StartsWith("A"));
```

### Append filters

Use `AppendFilter` when separate components contribute restrictions. The
default condition is `And`:

```csharp
using static shlabs.DataQuery.Abstractions.Dynamic.QueryBuilderFilter;

var builder = new QueryBuilder<Student>()
    .AppendFilter(student => student.SchoolId == 7)
    .AppendFilter(student => student.Name.StartsWith("A"));
```

The result is equivalent to:

```csharp
student => student.SchoolId == 7 && student.Name.StartsWith("A")
```

Pass `Conditions.Or` to append an alternative:

```csharp
builder.AppendFilter(student => student.Name.StartsWith("B"), Conditions.Or);
```

That produces `(schoolIdIs7 AND nameStartsWithA) OR nameStartsWithB`. If the intended
logic needs more complex grouping, express the group in one lambda before
appending it:

```csharp
builder.AppendFilter(student =>
    student.Name.StartsWith("B") || student.Name.EndsWith("son"));
```

Use normal expression-tree-friendly C# in filters. Avoid calling arbitrary
local methods or compiling expressions; EF Core must be able to translate the
final expression to the database provider.

## Ordering

`SetOrderBy` establishes (or replaces) the root ordering. `AddOrderBy` adds a
secondary key:

```csharp
var builder = new QueryBuilder<Student>()
    .AddOrderBy(student => student.Name, QueryOrderDirections.Asc)
    .AddOrderBy(student => student.SchoolId, QueryOrderDirections.Asc)
    .AddOrderBy(student => student.Id, QueryOrderDirections.Asc);
```

`QueryOrderDirections` supports `Asc` and `Desc`.

If you are creating order definitions separately, pass them to `SetOrder`:

```csharp
var byName = new QueryOrder<Student, string>(
    student => student.Name, QueryOrderDirections.Asc);
var bySchool = new QueryOrder<Student, int>(
    student => student.SchoolId, QueryOrderDirections.Asc);

var builder = new QueryBuilder<Student>().SetOrder(byName, bySchool);
```

The order of the definitions is preserved: the first becomes `OrderBy` and
later definitions become `ThenBy` calls.

## Paging

Use `SetSkip` and `SetTake` for fluent paging:

```csharp
var pageNumber = 3; // zero-based in this example
var pageSize = 25;

var builder = new QueryBuilder<Student>()
    .AddOrderBy(student => student.Id, QueryOrderDirections.Asc)
    .SetSkip(pageNumber * pageSize)
    .SetTake(pageSize);
```

`Skip` and `Take` are nullable properties inherited from `CoreQueryBuilder`,
so either may be omitted. They can also be assigned during initialization:

```csharp
var builder = new QueryBuilder<Student> { Skip = 50, Take = 25 };
```

## Includes and nested includes

Includes describe EF Core navigation loading. Use `IncludeEntity` for a
reference navigation and `IncludeList` for a collection navigation. `Include`
selects the matching form from the expression type.

### Reference navigation

```csharp
var builder = new QueryBuilder<Teacher>();
builder.IncludeEntity(teacher => teacher.School);
```

For a single fluent root-builder statement, use `AddInclude`:

```csharp
var builder = new QueryBuilder<Teacher>()
    .AddInclude(teacher => teacher.School);
```

### Collection navigation

```csharp
var builder = new QueryBuilder<School>();
builder.IncludeList(school => school.Students);
```

### Nested navigation

The `Include*` methods return an include descriptor on which you can add a
`ThenInclude` chain:

```csharp
var builder = new QueryBuilder<Teacher>();

builder.IncludeEntity(teacher => teacher.School)
       .ThenIncludeList(school => school.Students);
```

The next example loads a school, its students, and each student's grades:

```csharp
var builder = new QueryBuilder<School>();

builder.IncludeList(school => school.Students)
       .ThenIncludeList(student => student.Grades);
```

### Filtered collection include

Pass a predicate as the second argument to include only matching collection
items:

```csharp
var builder = new QueryBuilder<School>();

builder.IncludeList(
    school => school.Students,
    student => student.Name.StartsWith("A"));
```

For a nested collection, pass the filter to `ThenIncludeList`:

```csharp
builder.IncludeEntity(teacher => teacher.School)
       .ThenIncludeList(
           school => school.Students,
           student => student.Name.StartsWith("A"));
```

Collection include descriptors also expose `Order`, `Skip`, and `Take`. Set
them before turning the root builder into an `IQueryable`:

```csharp
var builder = new QueryBuilder<School>();
var students = builder.IncludeList(school => school.Students, student => student.Name.StartsWith("A"));

students.Order = new QueryOrder<Student, string>(
    student => student.Name,
    QueryOrderDirections.Asc);
students.Skip = 0;
students.Take = 10;
```

These operations stay within the EF Core `Include` expression; they do not
filter, order, or page the root `School` query.

## Dynamic filters and ordering

The dynamic model is useful for APIs that receive field names, operators, and
string values. It is separate from the strongly typed `QueryBuilder<TEntity>`:

```csharp
using DynamicQueryBuilder = shlabs.DataQuery.Abstractions.Dynamic.QueryBuilder;
using shlabs.DataQuery.Abstractions.Dynamic;

var requestQuery = new DynamicQueryBuilder
{
    Filter = QueryBuilderFilter.And(
        new QueryBuilderFilterCriteria(
            "Name",
            QueryBuilderCriteriaConditions.Contains,
            "Smith"),
        new QueryBuilderFilterCriteria(
            "SchoolId",
            QueryBuilderCriteriaConditions.Equal,
            "7")),
    Skip = 0,
    Take = 20
};

requestQuery.AddOrder("Name", QueryOrderDirections.Asc);
requestQuery.AddOrder("Id", QueryOrderDirections.Asc);

QueryBuilder<Student> builder = requestQuery.ToQueryBuilder<Student>();
var students = await builder.ToQueryable(dbContext).ToListAsync();
```

The supported criteria are `Equal`, `NotEqual`, `Greater`, `Less`,
`GreaterOrEqual`, `LessOrEqual`, `Contains`, `NotContains`, `Null`, and
`NotNull`. `In` and `NotIn` are declared but are not implemented.

Dynamic field paths may be nested (for example, `"School.Name"`). Treat field
names and values received from clients as untrusted input: validate that fields
are allowed for the request type and return a clear validation error when
conversion or property lookup fails. Use `QueryBuilderFilterRuleConvertorOptions`
when dynamic date/time conversion needs a specific `DateTimeKind`.

## Clone a query definition

Use `Clone()` to reuse a base definition while changing its top-level state:

```csharp
var activeStudents = new QueryBuilder<Student>()
    .AppendFilter(student => student.SchoolId == 7)
    .AddOrderBy(student => student.Name, QueryOrderDirections.Asc);

var firstPage = activeStudents.Clone().SetTake(25);
var secondPage = activeStudents.Clone().SetSkip(25).SetTake(25);
```

The clone copies the include list but does not deep-clone nested include and
order objects. Treat those shared nested descriptors as immutable after cloning
unless sharing is intentional.

## Execute the query

`ToQueryable` only builds the query. Execute it using normal EF Core APIs:

```csharp
var query = builder.ToQueryable(dbContext);

var items = await query.ToListAsync();
var count = await query.CountAsync();
var first = await query.FirstOrDefaultAsync();
```

The query builder intentionally does not own materialization, tracking, or
projection choices. Add those to the existing source query or after
`ToQueryable`, according to your application's data-access conventions.

## Build, test, and run the example

The repository uses .NET 8 (see `global.json`). From `src/`:

```powershell
dotnet restore shlabs.DataQuery.sln
dotnet build shlabs.DataQuery.sln
dotnet test shlabs.DataQuery.sln
dotnet run --project shlabs.DataQuery.Example.Run\shlabs.DataQuery.Example.Run.csproj
```

The example project uses the school domain and demonstrates an include chain.
Tests in `shlabs.DataQuery.Test` cover translation behavior against the sample
EF Core model.

## Contributing

Keep the abstraction project free of EF Core types. Provider-specific
translation belongs in `shlabs.DataQuery`. When changing expression translation
or include behavior, add a test that verifies the observable result through EF
Core rather than only inspecting expression-tree internals.
