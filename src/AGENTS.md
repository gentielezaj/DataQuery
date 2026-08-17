# DataQuery contributor guide

## Purpose

This repository provides a composable query-builder model for .NET applications.
It lets an application describe a query before it has a database source or an
`IQueryable<T>`. The description can contain filters, ordering, paging, and
Entity Framework Core-style navigation includes (including nested and filtered
collection includes).

The query builder is not itself an `IQueryable<T>` and it does not execute a
query. Construct a `QueryBuilder<T>` anywhere it is useful (for example, in an
API request handler, application service, or specification-like layer). When a
query source is available, translate the builder into an EF Core query using
the extension methods in `shlabs.DataQuery/QueryBuilderToQueryable.cs`.

The two primary library projects are:

| Project | Responsibility | Dependencies |
| --- | --- | --- |
| `shlabs.DataQuery.Abstractions` | Provider-independent query model, fluent builder API, expression composition, include metadata, ordering metadata, and dynamic-filter conversion. | No EF Core dependency. |
| `shlabs.DataQuery` | Applies an abstraction-layer builder to EF Core `IQueryable<T>` data using `Where`, `OrderBy`/`ThenBy`, `Skip`, `Take`, `Include`, and `ThenInclude`. | References `Abstractions` and EF Core. |

Keep this separation intact. Do not introduce EF Core types into
`shlabs.DataQuery.Abstractions`; add provider-specific query translation to
`shlabs.DataQuery` (or a separate provider package).

## Repository layout

- `shlabs.DataQuery.Abstractions/` — core public API.
  - `QueryBuilder<TEntity>.cs` — strongly typed builder for filters, ordering,
    paging, and includes.
  - `Include/` — metadata and fluent APIs for reference/collection includes
    and their nested `ThenInclude` chains.
  - `Order/` — typed ordering model.
  - `Dynamic/` — serializable/dynamic filter and order inputs, converted into
    a typed `QueryBuilder<TEntity>` through `ToQueryBuilder<T>()`.
  - `Utils/` — expression and dynamic-filter utilities.
- `shlabs.DataQuery/` — EF Core translation layer.
  - `QueryBuilderToQueryable.cs` — main translation entry points.
  - `QueryOrderToQueryable.cs` — translates the ordering chain.
  - `QueryIncludeToQueryable.cs` — translates includes, nested includes, and
    collection include filter/order/paging expressions.
- `shlabs.DataQuery.Test/` — xUnit integration-style tests against the sample
  EF Core model.
- `shlabs.DataQuery.Example.Infrastructure/` — sample entities, `DbContext`,
  data configuration, and migrations.
- `shlabs.DataQuery.Example.Run/` — runnable demonstration application.
- `shlabs.DataQuery.sln` — solution file; .NET SDK is pinned by `global.json`
  to .NET 8 (latest minor roll-forward permitted).

## Core usage and execution boundary

Build the query description without a `DbContext`, `DbSet<T>`, or
`IQueryable<T>`:

```csharp
var builder = new QueryBuilder<School>()
    .AppendFilter(s => s.Name.StartsWith("A"))
    .AddOrderBy(s => s.Name, QueryOrderDirections.Asc)
    .SetSkip(20)
    .SetTake(10)
    .AddInclude(s => s.SchoolClasses);
```

At the infrastructure boundary, materialize an EF Core query with one of these
overloads:

```csharp
// Starts with dbContext.Set<School>().
IQueryable<School> query = builder.ToQueryable(dbContext);

// Starts with this specific DbSet.
IQueryable<School> query = builder.ToQueryable(dbContext.Set<School>());

// Composes over an existing query, preserving earlier restrictions.
IQueryable<School> query = builder.ToQueryable(sourceQuery);
```

`ToQueryable` returns an `IQueryable<T>`; it does not execute SQL. The caller
chooses when to execute it, such as `ToListAsync`, `FirstOrDefaultAsync`, or
`CountAsync`. The translator applies the root builder in this order:

1. `Filter` (`Where`)
2. `Order` (`OrderBy`/`ThenBy`)
3. `Skip`
4. `Take`
5. `Includes`

Use the existing source-query overload when global query filters, tenancy
restrictions, projections, or other query constraints have already been added.

## Builder API conventions

### Typed `QueryBuilder<TEntity>`

- `Filter` stores an `Expression<Func<TEntity, bool>>`.
- Prefer `AppendFilter(filter, Conditions.And)` when accumulating independent
  restrictions. It combines expressions without replacing an existing filter.
- Use `SetFilter` only when replacement is intended.
- Use `SetOrderBy` to replace the root ordering and `AddOrderBy` to append a
  secondary ordering. Preserve the supplied order sequence.
- Use `SetSkip` and `SetTake` for fluent paging, or the inherited `Skip` and
  `Take` properties when object initialization is clearer.
- `Clone()` copies the builder's top-level state and include list. It does not
  deep-clone every nested include/order object; do not mutate shared nested
  objects unexpectedly after cloning.

### Includes

The typed builder exposes both fluent (`AddInclude*`) and inspectable
(`Include*`) APIs:

- `Include` / `IncludeEntity` handles a reference navigation and returns its
  include object so callers can add `ThenInclude` steps.
- `Include` / `IncludeList` handles a collection navigation. Collection
  includes may carry a filter, ordering, skip, and take configuration.
- `AddInclude`, `AddIncludeEntity`, and `AddIncludeList` add an include and
  return the root `QueryBuilder<TEntity>` to support chaining.

Example nested include:

```csharp
var builder = new QueryBuilder<Teacher>();
builder.IncludeEntity(t => t.School)
       .ThenIncludeList(s => s.Students, student => student.IsActive);
```

The EF translator constructs `Include`/`ThenInclude` expression trees for
these descriptors. Retain expression-based navigation and filter APIs so EF
Core can translate the final query. Do not compile expressions or enumerate an
`IQueryable<T>` within the translation layer.

### Dynamic input model

`shlabs.DataQuery.Abstractions.Dynamic.QueryBuilder` represents externally
supplied filter/order/paging input. It has a `QueryBuilderFilter` tree and a
list of field-name-based `QueryOrder` values. Convert it with:

```csharp
QueryBuilder<Customer> typed = dynamicBuilder.ToQueryBuilder<Customer>();
```

Conversion creates typed expression trees. If dynamic filtering needs a new
operator or conversion behavior, update the dynamic filter-rule classes and
their converters in `Abstractions/Utils`, and add focused tests for valid and
invalid values. Do not bypass the typed builder by injecting provider-specific
expressions into dynamic DTOs.

## Implementation guardrails

- Public APIs use nullable reference types and .NET 8/C# modern syntax; keep
  nullable annotations accurate.
- `QueryOrderToQueryable` and `QueryIncludeToQueryable` build expression
  trees deliberately. Changes here must preserve the correct generic method,
  lambda parameter type, ordering position (`OrderBy` versus `ThenBy`), and
  query provider execution.
- A collection include's filter/order/page operations must remain inside its
  `Include` expression; they must not become root-query operations.
- Avoid adding query execution (`ToList`, `AsEnumerable`, `Compile`, etc.) to
  the builder or translator. Execution belongs to the caller after
  `ToQueryable`.
- Preserve source compatibility for public package APIs. Both main projects
  are packable and currently publish version `0.0.12`; coordinate package
  metadata/version changes intentionally.
- The public translation extension classes intentionally live in the
  `System.Linq` namespace, enabling `builder.ToQueryable(...)`. Do not move
  them without considering breaking changes and extension-method discovery.

## Testing and validation

From this directory, use:

```powershell
dotnet restore shlabs.DataQuery.sln
dotnet build shlabs.DataQuery.sln
dotnet test shlabs.DataQuery.sln
```

For a focused change, run the affected test project:

```powershell
dotnet test shlabs.DataQuery.Test\shlabs.DataQuery.Test.csproj
```

When changing any translation behavior, add or update tests covering the
generated query's observable result. At minimum, consider root filtering,
multi-column ordering, paging, reference includes, collection includes,
filtered collection includes, and nested `ThenInclude` chains. The sample
school domain is the intended test fixture for these cases.
