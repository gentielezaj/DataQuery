# shlabs.DataQuery

A sample C# project demonstrating advanced querying with Entity Framework Core using a custom `QueryBuilder` abstraction. The project includes example data models for a school system and supports flexible query composition with navigation property inclusion.

## Features

- Entity Framework Core integration
- Custom `QueryBuilder` for dynamic query construction
- Example domain: Schools, Students, Teachers, Classes, Grades
- Database seeding with sample data
- Unit tests for query logic

## Getting Started

### Prerequisites

- .NET 7.0 SDK or later
- SQL Server or SQLite (configured in `AppDbContext`)

### Setup

1. Clone the repository.
2. Restore dependencies:
   ```
   dotnet restore
   ```
3. Build the solution:
   ```
   dotnet build
   ```
4. Run the example console app:
   ```
   dotnet run --project shlabs.DataQuery.Example.Run
   ```

### Running Tests

To execute unit tests:
```
dotnet test
```

## Project Structure

- `shlabs.DataQuery.Abstractions` — Query abstraction layer
- `shlabs.DataQuery.Example.Infrastructure` — Data models and EF Core configuration
- `shlabs.DataQuery.Example.Run` — Console app demonstrating usage
- `shlabs.DataQuery.Test` — Unit tests

## License

MIT License

---

You can expand this as needed for more details or usage examples.