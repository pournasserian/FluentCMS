# MySQL Provider in FluentCMS

## Overview

The MySQL provider in FluentCMS offers integration with MySQL databases through Entity Framework Core, providing a fully-featured relational database option while maintaining the consistent repository pattern used throughout the system.

## Implementation

The MySQL provider is implemented in the `FluentCMS.Repositories.MySQL` assembly and builds on the Entity Framework Core infrastructure, using the Pomelo.EntityFrameworkCore.MySql package for MySQL-specific functionality.

### Key Components

- `MySqlOptions`: Configuration options for MySQL repositories
- `MySqlDbContext`: MySQL-specific DbContext implementation
- `MySqlEntityRepository<TEntity>`: Repository implementation for MySQL
- `ServiceCollectionExtensions`: Extension methods for dependency injection registration

## Installation

To use the MySQL provider, add a reference to the `FluentCMS.Repositories.MySQL` package:

```bash
dotnet add package FluentCMS.Repositories.MySQL
```

## Configuration

### Direct Provider Configuration

You can directly configure and use the MySQL provider with the extension methods:

```csharp
// Program.cs or Startup.cs
services.AddMySqlRepositories(options =>
{
    // Required
    options.ConnectionString = "Server=localhost;Database=FluentCMS;User=root;Password=password;";
    
    // Optional - MySQL-specific settings
    options.ServerVersion = "8.0.28";
    options.CharacterSet = "utf8mb4";
    options.UseConnectionPooling = true;
    options.ConnectionTimeout = 30;
    options.MaxPoolSize = 100;
    options.MinPoolSize = 0;
    options.UseSsl = false;
    
    // Optional - Entity Framework configuration
    options.UseCamelCaseTableNames = true;
    options.TableNamePrefix = "cms_";
    options.DefaultSchema = "fluentcms";
    options.AutoMigrateDatabase = true;
    options.UsePluralTableNames = true;
});
```

### Repository Factory Configuration

When using the Repository Factory, configure MySQL as follows:

```csharp
// Program.cs or Startup.cs
services.AddRepositoryFactory(options =>
{
    options.Provider = "MySQL";
    options.MySQL.ConnectionString = "Server=localhost;Database=FluentCMS;User=root;Password=password;";
    options.MySQL.ServerVersion = "8.0.28";
    // Other options as needed
});
```

### Configuration in appsettings.json

```json
{
  "Repository": {
    "Provider": "MySQL",
    "MySQL": {
      "ConnectionString": "Server=localhost;Database=FluentCMS;User=root;Password=password;",
      "ServerVersion": "8.0.28",
      "CharacterSet": "utf8mb4",
      "UseConnectionPooling": true,
      "ConnectionTimeout": 30,
      "MaxPoolSize": 100,
      "MinPoolSize": 0,
      "UseSsl": false
    },
    "EntityFramework": {
      "UseCamelCaseTableNames": true,
      "TableNamePrefix": "cms_",
      "UsePluralTableNames": true
    }
  }
}
```

## Configuration Options

### MySQL-Specific Options

| Option | Description | Default |
|--------|-------------|---------|
| `ConnectionString` | MySQL connection string | Required |
| `ServerVersion` | MySQL server version (e.g., "8.0.28") | Detected from connection string |
| `CharacterSet` | Default character set | "utf8mb4" |
| `UseConnectionPooling` | Whether to use connection pooling | true |
| `ConnectionTimeout` | Connection timeout in seconds | 30 |
| `MaxPoolSize` | Maximum connection pool size | 100 |
| `MinPoolSize` | Minimum connection pool size | 0 |
| `UseSsl` | Whether to use SSL/TLS for connections | false |

### Common Entity Framework Options

| Option | Description | Default |
|--------|-------------|---------|
| `UseCamelCaseTableNames` | Use camelCase for table names | false |
| `TableNamePrefix` | Prefix for all table names | "" |
| `TableNameSuffix` | Suffix for all table names | "" |
| `DefaultSchema` | Default database schema | null |
| `AutoMigrateDatabase` | Auto-migrate on startup | false |
| `UsePluralTableNames` | Use pluralized table names | true |

## Best Practices

### Connection String Security

Store your connection string securely:

- For development, use user secrets: `dotnet user-secrets set "Repository:MySQL:ConnectionString" "your-connection-string"`
- For production, use environment variables or a secure configuration provider

### Entity Design

For optimal MySQL performance:

- Be mindful of index usage
- Consider appropriate column types and lengths
- Avoid overly complex relationships that may impact query performance

### Transaction Handling

The MySQL provider supports transactions through Entity Framework Core. Use the appropriate transaction scope when performing multiple related operations:

```csharp
using (var transaction = await _dbContext.Database.BeginTransactionAsync())
{
    try
    {
        // Multiple repository operations
        await _repository.Create(entity1);
        await _repository.Update(entity2);
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## Performance Considerations

### Connection Pooling

For web applications, connection pooling is enabled by default. Adjust the pool size based on your expected workload:

```csharp
options.MaxPoolSize = 200; // Increase for high-concurrency workloads
```

### Parameter Sizing

When sending large amounts of data:

```csharp
// Configure command timeout for long-running operations
options.ConnectionTimeout = 60; // Seconds
```

## Migrations

When using migrations with MySQL, ensure you're using the appropriate tooling:

```bash
# Install EF Core tools if needed
dotnet tool install --global dotnet-ef

# Add migration
dotnet ef migrations add InitialCreate --project YourProject

# Update database
dotnet ef database update --project YourProject
```

## Compatibility

The MySQL provider is built using Pomelo.EntityFrameworkCore.MySql, which supports:

- MySQL 5.6, 5.7, 8.0+
- MariaDB 10.1+
- .NET 8.0
- Entity Framework Core 8.0

## Limitations

- Certain advanced MySQL features (like spatial data) may require custom configuration
- Complex text searches should use MySQL's full-text search capabilities rather than LIKE patterns for performance

## See Also

- [Database Providers](./Database-Providers.md) - Overview of all database providers
- [Repository Pattern](./Repository-Pattern.md) - How the repository pattern is implemented
- [Entity Model](./Entity-Model.md) - Entity structure and design
- [Home](./Home.md) - Return to wiki home
