# shlabs.DataQuery

A C# library for advanced, composable querying with Entity Framework Core using a custom `QueryBuilder` abstraction. This project enables dynamic query construction, including filtering, ordering, and navigation property inclusion, with a focus on flexibility and testability.

## Features

- Entity Framework Core integration
- Custom `QueryBuilder` for dynamic query composition
- Support for navigation property inclusion (with filtering, ordering, skip/take)
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

