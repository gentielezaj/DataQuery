# Repository guide

## Working scope
- The active solution is `src/shlabs.DataQuery.sln`; `archive/QueryInfo.sln` is a separate historical implementation. Target `src/` for DataQuery changes.
- Read `src/AGENTS.md` for builder/translation conventions; `src/README.md` contains usage examples.
- `src/shlabs.DataQuery.TypeScript` is a standalone npm package for dynamic-query JSON types/helpers, outside the .NET solution. From that directory run `npm ci`, `npm test` (build + type checks + Node tests), and `npm pack`; `prepack` builds the distributable. The NuGet workflow does not publish it.

## Commands
Run these from `src/` so `global.json` selects the .NET 8 SDK (`latestMinor` roll-forward, no prereleases). All active projects target `net8.0`.

```powershell
dotnet restore shlabs.DataQuery.sln
dotnet build shlabs.DataQuery.sln --no-restore
dotnet test shlabs.DataQuery.sln --no-build
```

- Focused test run (builds the project and its dependencies): `dotnet test shlabs.DataQuery.Test/shlabs.DataQuery.Test.csproj --filter "FullyQualifiedName~QueryBuilderSqliteIntegrationTests"`. Replace the filter with a class or method name as needed.
- Tests use xUnit and real SQLite in memory, not EF's InMemory provider. `DatabaseFixture` holds the connection open and calls `EnsureCreated`; model seed data comes from `src/shlabs.DataQuery.Example.Infrastructure/Config/AddDataToTable.cs`. No external database or migrations are required for tests.
- Demo: `dotnet run --project shlabs.DataQuery.Example.Run/shlabs.DataQuery.Example.Run.csproj`. It calls `Migrate` on the hard-coded `SetUp.DbPath` (`C:\workspace\github\gentielezaj\DataQuery\QueryBuilder.db`) and waits at `Console.ReadKey`; use tests for automated verification.

## Boundaries and gotchas
- `shlabs.DataQuery.Abstractions` owns the typed/dynamic query description and expression conversion, with no EF dependency. `shlabs.DataQuery` owns provider translation through `QueryBuilderToQueryable.cs`, `QueryOrderToQueryable.cs`, and `QueryIncludeToQueryable.cs`.
- Translation extensions deliberately live in `System.Linq`. `ToQueryable` composes onto an existing source and remains deferred: root operations are filter → order → skip → take → includes. Collection-include filtering/ordering/paging stays inside the include expression.
- `QueryBuilder.Clone()` copies the include list but shares its descriptors and the ordering object; it is not a deep clone.
- Both library projects set `GeneratePackageOnBuild`; ordinary builds also create NuGet packages. Read package versions from the project files rather than assuming they match older prose.
- `.github/workflows/push-to-nudget.yml` publishes on `v*` tags, with Abstractions before implementation, and overrides the pack version from the tag. It does not run tests; local test verification matters before release work.
