# LinqAPI

**LinqAPI** is a dynamic, generic API generator for ASP.NET Core that leverages LINQ-based filtering, paging, and CRUD operations with automatic mapping between your entities and DTOs. It provides a set of reusable controllers, services, and repositories designed to reduce boilerplate code and streamline API development.

## Features

- **Dynamic CRUD Operations**: Automatically generates common CRUD operations.
- **LINQ-based Filtering & Paging**: Supports dynamic filtering and paging via LINQ expressions.
- **Automatic Mapping**: Uses AutoMapper to automatically map between your entities and DTOs without the need for manual profile creation.
- **Extensible & Generic**: Easily extendable to any entity/DTO pair that follows the `BaseEntity<TId>` and `BaseDto<TId>` pattern.
- **DI Registration Helper**: Register all necessary services with a single call (`builder.Services.AddLinqApi()`).

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [AutoMapper](https://automapper.org/)
- Optional: [Microsoft.EntityFrameworkCore.InMemory](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory) for testing/demo purposes

### Installation

1. **Add LinqAPI to Your Project**

   You can add LinqAPI to your project via NuGet:

   ```bash
   dotnet add package LinqAPI
