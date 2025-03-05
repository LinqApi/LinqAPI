# LinqAPI

**LinqAPI** is a dynamic and generic API generator for ASP.NET Core that leverages LINQ-based filtering, paging, and CRUD operations. It provides a set of reusable controllers, services, and repository implementations to reduce boilerplate code and streamline API development by automatically mapping between your entities and DTOs.

---

## Table of Contents

- [Features](#features)
- [Introduction](#introduction)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [DI Registration](#di-registration)
- [Usage](#usage)
  - [Creating Your API Controllers](#creating-your-api-controllers)
  - [Automatic Mapping](#automatic-mapping)
- [Demo Project](#demo-project)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgements](#acknowledgements)

---

## Features

- **Dynamic CRUD Operations**: Automatically handles common CRUD operations.
- **LINQ-Based Filtering & Paging**: Supports dynamic filtering using LINQ expressions and returns paginated results.
- **Automatic Mapping**: Uses AutoMapper with a dynamic mapping profile to map between your entities and DTOs without manual configuration.
- **Generic & Extensible**: Works with any entity/DTO pair that derives from `BaseEntity<TId>` and `BaseDto<TId>`.
- **DI Registration Helper**: Simplifies dependency injection configuration via a single extension method (`builder.Services.AddLinqApi()`).

---

## Introduction

Developing RESTful APIs in ASP.NET Core can involve a lot of repetitive code—especially when dealing with CRUD operations, filtering, and pagination. **LinqAPI** abstracts much of this complexity by providing a generic, reusable framework. It leverages the power of LINQ to allow dynamic filtering and paging while automatically mapping between your domain entities and Data Transfer Objects (DTOs).

The core components include:
- **Generic Controller (`LinqController`)**: Provides endpoints for CRUD, filtering, and paging.
- **Repository (`LinqRepository`)**: Implements common data access operations using Entity Framework Core.
- **Service (`LinqService`)**: Orchestrates repository calls and mapping between entities and DTOs.
- **Dynamic AutoMapper Registration**: Scans controllers and automatically creates mapping profiles between corresponding entity and DTO types.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- ASP.NET Core (for building Web APIs)
- Entity Framework Core (for data access)
- AutoMapper (for object mapping)
- Optional: [Microsoft.EntityFrameworkCore.InMemory](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory) for demo or testing purposes

### Installation

You can add LinqAPI to your project via NuGet:

```sh
dotnet add package LinqAPI
```

Or, if you're developing it locally, clone the repository and add it as a project reference.

### DI Registration

LinqAPI provides an extension method to simplify DI registration. In your `Program.cs` (or `Startup.cs`), add:

```csharp
using LinqAPI;

var builder = WebApplication.CreateBuilder(args);

// Register your DbContext (example using InMemory database for demo purposes)
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseInMemoryDatabase("DemoDb"));

// Register LinqAPI dependencies, including repository, service, and automatic mapping
builder.Services.AddLinqApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Usage

### Creating Your API Controllers

LinqAPI offers a generic controller that you can extend to create your API endpoints. For example, if you have a product API:

```csharp
using LinqAPI;
using YourProject.Entities;
using YourProject.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers
{
    public class ProductsController : LinqController<ProductEntity, ProductDto, int>
    {
        public ProductsController(ILinqService<ProductEntity, ProductDto, int> service) : base(service)
        {
        }
    }
}
```

Your entities and DTOs should adhere to the following patterns:

```csharp
// Entities/ProductEntity.cs
public class ProductEntity : BaseEntity<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// DTOs/ProductDto.cs
public class ProductDto : BaseDto<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Automatic Mapping

LinqAPI leverages AutoMapper to automatically map between your entity and DTO types. The library scans your assemblies for controllers that derive from `LinqController<T1, T2, TId>` and creates bi-directional mappings for the provided types.

---

## Demo Project

A demo ASP.NET Core Web API project is provided to illustrate how to integrate LinqAPI.

### Example: Demo DbContext & Data Seeding

```csharp
// Data/DemoDbContext.cs
using YourProject.Entities;
using Microsoft.EntityFrameworkCore;

namespace YourProject.Data
{
    public class DemoDbContext : DbContext
    {
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options) { }

        public DbSet<ProductEntity> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductEntity>().HasData(
                new ProductEntity { Id = 1, Name = "Product A", Price = 10.99m },
                new ProductEntity { Id = 2, Name = "Product B", Price = 20.50m },
                new ProductEntity { Id = 3, Name = "Product C", Price = 15.75m }
            );
        }
    }
}
```

---

## Testing

LinqAPI supports unit testing at the repository, service, and controller levels.

---

## Contributing

Contributions are welcome! Please review our [CONTRIBUTING.md](CONTRIBUTING.md) for details.

---

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.

---

## Acknowledgements

- [AutoMapper](https://automapper.org/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- The open-source community for valuable contributions.

---

## Final Notes

LinqAPI aims to reduce repetitive coding in API development by leveraging generics, LINQ filtering, and automatic mapping. With minimal configuration, you can rapidly set up a fully functional API.

For any issues, feature requests, or questions, please open an issue in the repository or contact the maintainers.

Happy coding!

