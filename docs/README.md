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
  - [Filtering and Paging](#filtering-and-paging)
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
- **DI Registration Helper**: Simplifies dependency injection configuration via a single extension method (`builder.Services.AddLinqApi<DbContext>()`).

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- ASP.NET Core  (for building Web APIs)
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

// Register your DbContext
builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseInMemoryDatabase("DemoDb"));

// Register LinqAPI dependencies, including repository, service, and automatic mapping
builder.Services.AddLinqApi<DemoDbContext>();

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

## Filtering and Paging

LinqAPI provides powerful filtering and paging capabilities using dynamic LINQ queries. You can perform filtering using expressions such as `StartsWith`, `EndsWith`, `Contains`, and logical operators like `and`, `or`, `>`, `<`, `>=`, `<=`.

### **Example Request for `filterpaged` Endpoint**

```json
{
  "filter": "name.StartsWith(\"Product C\")",
  "pager": {
    "pageNumber": 1,
    "pageSize": 50
  },
  "orderby": "id",
  "desc": true
}
```

### **How Filtering Works**
The `filter` field is a **dynamic LINQ expression** that is executed against the database.

- `StartsWith("value")` – Matches strings that start with the given value.
- `EndsWith("value")` – Matches strings that end with the given value.
- `Contains("value")` – Matches strings that contain the given value.
- `>` `<` `>=` `<=` – Comparison operators.
- `and`, `or` – Logical operators.
- `1=1` – Used to retrieve all records without filtering.

#### **Example Queries:**

Retrieve products where price is greater than 20 and name contains "Product":
```json
{
  "filter": "price > 20 and name.Contains(\"Product\")"
}
```
Retrieve items where ID is less than 5 or the name starts with "Item":
```json
{
  "filter": "id < 5 or name.StartsWith(\"Item\")"
}
```

---

## **Paging System**
Paging is controlled using the `Pager` class:

```csharp
public class Pager
{
    private int _pageNumber = 1;
    private int _pageSize = 50;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value > int.MaxValue ? int.MaxValue : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : value > 500 ? 500 : value;
    }
}
```

- `PageNumber`: Defines the current page (default = 1).
- `PageSize`: Defines the number of results per page (min = 1, max = 500).

Retrieve all records without filtering:
```json
{
  "filter": "1=1",
  "pager": {
    "pageNumber": 1,
    "pageSize": 50
  }
}
```

### **Sorting with `orderby` & `desc`**
- `orderby`: The property name to sort results by.
- `desc`: Boolean flag for descending order (`true` for descending, `false` for ascending).

---

## **Including Navigation Properties**
The `Includes` field allows eager loading of related navigation properties.

```csharp
public class LinqFilterModel
{
    public string Filter { get; set; }
    public Pager Pager { get; set; }
    public List<string> Includes { get; set; }
    public string Orderby { get; set; }
    public bool Desc { get; set; }
}
```

#### **Example Request with Includes:**
```json
{
  "filter": "1=1",
  "pager": {
    "pageNumber": 1,
    "pageSize": 50
  },
  "includes": ["Category", "Supplier"]
}
```
This will include **Category** and **Supplier** navigation properties in the response.

---

## **Conclusion**
LinqAPI simplifies API development by providing **automated filtering, paging, and sorting using LINQ expressions**. With minimal setup, you can create powerful, dynamic APIs.

For any issues, feature requests, or questions, please open an issue in the repository or contact the maintainers.

Happy coding! 🚀
