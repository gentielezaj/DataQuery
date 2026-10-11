# shlabs-dataquery

TypeScript types and dependency-free helpers for the JSON contract of
`shlabs.DataQuery.Abstractions.Dynamic.QueryBuilder`. Works with browser
clients and ESM Node.js applications; JavaScript and `.d.ts` declarations ship
together. Requires TypeScript 5+ when consuming the types.

## Build and install locally

From this directory:

```sh
npm ci
npm test
npm pack
```

Then, from your frontend project, install the generated archive (adjust the path):

```sh
npm install /path/to/DataQuery/src/shlabs.DataQuery.TypeScript/shlabs-dataquery-0.1.1.tgz
```

The package name is `shlabs-dataquery`. Once published to an npm registry,
consumers can use `npm install shlabs-dataquery`. The repository's existing
NuGet release workflow does not publish this npm package.

## Build a request

```typescript
import {
  and,
  or,
  criteria,
  asc,
  page,
  QueryBuilderCriteriaConditions as Operators,
  type DynamicQueryBuilder
} from "shlabs-dataquery";

const query: DynamicQueryBuilder = {
  filter: and(
    criteria("schoolId", Operators.Equal, 1),
    or(
      criteria("name", Operators.Contains, "Jane"),
      criteria("name", Operators.Contains, "John")
    )
  ),
  order: [asc("name"), asc("id")],
  ...page(0, 20)
};

await fetch("/api/students/search", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(query)
});

const parameters = new URLSearchParams({ query: JSON.stringify(query) });
await fetch(`/api/students?${parameters}`);
```

For a model without helpers, use the same types with object literals:

```typescript
import type { QueryBuilder } from "shlabs-dataquery";

const query: QueryBuilder = {
  filter: {
    condition: "and",
    rules: [{ field: "schoolId", operator: "equal", value: 1 }]
  },
  order: [{ field: "id", direction: "asc" }],
  skip: 0,
  take: 20
};
```

## Contract and helpers

- `QueryBuilder` (alias `DynamicQueryBuilder`) contains optional `filter`,
  `order`, `skip`, and `take`; these properties also accept `null`.
- `QueryBuilderFilter` groups criteria or nested groups using `and` or `or`.
  The root `filter` is a group, not a single criterion. Wrap one rule in `and()`.
- `criteria(field, operator, value)` accepts string, number, boolean, or null.
  Send dates as ISO strings and enum values as names or numeric strings. The
  backend converts values according to the entity property's type.
- `and(...rules)` and `or(...rules)` create groups. An empty group means no
  filtering in the .NET implementation.
- `asc(field)` and `desc(field)` create ordered keys; array order is preserved.
  Field paths can be nested, for example `school.name`.
- `page(pageNumber, pageSize)` uses a zero-based page number, returns `skip`
  and `take`, and rejects invalid or overflowing .NET `Int32` paging values.
  Supply deterministic ordering when paging.
- `QueryBuilderCriteriaConditions`, `QueryBuilderFilterConditions`, and
  `QueryOrderDirections` are runtime constants and corresponding string-union
  types. Raw string literals such as `"equal"` and `"asc"` are also accepted.

Types describe the wire contract, not backend support for every property type.
`in` and `notIn` are declared by .NET but currently unimplemented. Nullable
values are converted using their underlying type, including nullable enums.
Use JSON `null` for null equality, or `null` / `notNull` operators to check a
nullable or reference property. Helpers do not execute queries or validate
entity fields.

## ASP.NET Core setup

Register both JSON converters on the server:

```csharp
using System.Text.Json.Serialization;
using shlabs.DataQuery.Abstractions.Utils;

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new QueryBuilderFilterRuleConverter());
});
```

For POST, bind `shlabs.DataQuery.Abstractions.Dynamic.QueryBuilder` with
`[FromBody]`, then call `request.ToQueryBuilder<Student>()`. For GET, bind the
JSON query parameter as a string and deserialize it using the same MVC options
before converting. See [the repository README](../README.md#json-parsing-and-aspnet-core-controllers)
for full controller examples.
