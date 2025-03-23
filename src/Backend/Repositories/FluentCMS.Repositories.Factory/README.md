# FluentCMS Repository Factory

This package provides a factory implementation that enables configuration-based selection of database providers in FluentCMS. 
With this factory, you can switch between different database providers without changing your application code - 
just by modifying configuration in appsettings.json.

## Features

- Configuration-driven provider selection
- Support for multiple database providers:
  - MongoDB
  - LiteDB
  - PostgreSQL
  - MySQL
  - Entity Framework Core
  - SQLite
  - SQL Server
- Provider-specific configuration options
- Connection string builders for each provider
- Simplified dependency injection setup

## Installation

Add the FluentCMS.Repositories.Factory package to your project:

```bash
dotnet add package FluentCMS.Repositories.Factory
```

## Usage

### 1. Configuration in appsettings.json

Configure your database provider in appsettings.json:

```json
{
  "Repository": {
    "Provider": "MongoDB",
    
    "MongoDB": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "FluentCMS",
      "UseCamelCaseCollectionNames": true
    },
    
    "LiteDB": {
      "ConnectionString": "Filename=fluentcms.db;Connection=shared"
    },
    
    "EntityFramework": {
      "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=FluentCMS;Trusted_Connection=True;"
    },
    
    "SQLite": {
      "Filename": "fluentcms.db",
      "InMemory": false
    },
    
    "PostgreSQL": {
      "ConnectionString": "Host=localhost;Database=FluentCMS;Username=postgres;Password=yourpassword;"
    },
    
    "MySQL": {
      "ConnectionString": "Server=localhost;Database=FluentCMS;User=root;Password=password;"
    },
    
    "SqlServer": {
      "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=FluentCMS;Trusted_Connection=True;"
    }
  }
}
```

### 2. Register the Factory in Startup.cs or Program.cs

```csharp
// Program.cs
using FluentCMS.Repositories.Factory;

var builder = WebApplication.CreateBuilder(args);

// Register the repository factory
builder.Services.AddRepositoryFactory(builder.Configuration);

// Your other service registrations
// ...

var app = builder.Build();
```

Or in Startup.cs:

```csharp
// Startup.cs
using FluentCMS.Repositories.Factory;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Register the repository factory
        services.AddRepositoryFactory(Configuration);
        
        // Your other service registrations
        // ...
    }
    
    // Configure method
    // ...
}
```

### 3. Use the Repository in Your Code

Inject `IBaseEntityRepository<TEntity>` as usual:

```csharp
public class MyService
{
    private readonly IBaseEntityRepository<User> _userRepository;
    
    public MyService(IBaseEntityRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User?> GetUserAsync(Guid id)
    {
        return await _userRepository.GetById(id);
    }
}
```

## Provider-Specific Configuration

### MongoDB

```json
"MongoDB": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "FluentCMS",
  "UseCamelCaseCollectionNames": true,
  "CollectionNamePrefix": "",
  "CollectionNameSuffix": ""
}
```

### LiteDB

```json
"LiteDB": {
  "ConnectionString": "Filename=fluentcms.db;Connection=shared",
  "DatabasePath": "fluentcms.db",
  "Password": "optional-password",
  "ConnectionMode": "Shared"
}
```

### Entity Framework Core

```json
"EntityFramework": {
  "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=FluentCMS;Trusted_Connection=True;",
  "UseCamelCaseTableNames": true,
  "TableNamePrefix": "",
  "TableNameSuffix": "",
  "DefaultSchema": "dbo",
  "AutoMigrateDatabase": true,
  "UsePluralTableNames": true
}
```

### SQLite

```json
"SQLite": {
  "ConnectionString": "Data Source=fluentcms.db",
  "Filename": "fluentcms.db",
  "InMemory": false,
  "Password": "optional-password"
}
```

### PostgreSQL

```json
"PostgreSQL": {
  "ConnectionString": "Host=localhost;Database=FluentCMS;Username=postgres;Password=yourpassword;",
  "ServerVersion": "15.0",
  "UseConnectionPooling": true,
  "MaxPoolSize": 100,
  "MinPoolSize": 0,
  "ConnectionTimeout": 30,
  "UseSsl": false,
  "SslMode": "Prefer",
  "Schema": "public",
  "EnableRetryOnFailure": true,
  "MaxRetryCount": 5,
  "EnableBatchCommands": true,
  "UseJsonForComplexTypes": false,
  "UseCaseInsensitiveCollation": true
}
```

### MySQL

```json
"MySQL": {
  "ConnectionString": "Server=localhost;Database=FluentCMS;User=root;Password=password;",
  "ServerVersion": "8.0.28",
  "UseConnectionPooling": true,
  "MaxPoolSize": 100,
  "ConnectionTimeout": 30,
  "EnableRetryOnFailure": true,
  "MaxRetryCount": 5,
  "DefaultCharSet": "utf8mb4",
  "EnableBatchCommands": true
}
```

### SQL Server

```json
"SqlServer": {
  "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=FluentCMS;Trusted_Connection=True;",
  "Server": "(localdb)\\mssqllocaldb",
  "Database": "FluentCMS",
  "UseTrustedConnection": true,
  "Username": "",
  "Password": "",
  "EnableRetryOnFailure": true,
  "MaxRetryCount": 5,
  "CommandTimeout": 30,
  "EnableSensitiveDataLogging": false
}
```

## Component-Based Configuration

For some providers, you can use component-based configuration instead of connection strings:

### SQL Server Component-Based

```json
"SqlServer": {
  "Server": "(localdb)\\mssqllocaldb",
  "Database": "FluentCMS",
  "UseTrustedConnection": true
}
```

### SQLite Component-Based

```json
"SQLite": {
  "Filename": "fluentcms.db",
  "InMemory": false,
  "Password": "optional-password"
}
```

## Additional Configuration Options

### Custom Section Name

You can specify a custom configuration section name:

```csharp
services.AddRepositoryFactory(Configuration, "DatabaseSettings");
```

With corresponding configuration:

```json
{
  "DatabaseSettings": {
    "Provider": "MongoDB",
    "MongoDB": {
      // MongoDB settings...
    }
  }
}
```

### Programmatic Configuration

You can also configure the factory programmatically:

```csharp
services.AddRepositoryFactory(options =>
{
    options.Provider = "MongoDB";
    options.MongoDB.ConnectionString = "mongodb://localhost:27017";
    options.MongoDB.DatabaseName = "FluentCMS";
});
```

## Switching Providers

To switch database providers, simply change the `Provider` value in your configuration:

```json
{
  "Repository": {
    "Provider": "SQLite",  // Change to another provider
    
    // Provider-specific configurations remain the same
    "SQLite": {
      "Filename": "fluentcms.db"
    }
  }
}
```

No code changes are needed!
