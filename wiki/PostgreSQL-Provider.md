# PostgreSQL Provider for FluentCMS

## Overview

The PostgreSQL provider for FluentCMS offers a robust, feature-rich implementation of the repository pattern using PostgreSQL as the underlying database. This provider leverages Entity Framework Core with the Npgsql provider to deliver high performance, reliability, and compatibility with all PostgreSQL features.

## Features

- **Full Repository Pattern Implementation**: Complete implementation of the `IBaseEntityRepository<T>` interface.
- **Entity Framework Core Integration**: Built on EF Core for reliable ORM capabilities.
- **PostgreSQL-Specific Optimizations**:
  - JSON/JSONB column support for complex properties
  - Case-insensitive text searches with proper collation
  - Native array type support
  - Schema management
  - Connection pooling configuration
- **Advanced Querying**: Support for complex filtering, sorting, and pagination.
- **Database Migration Support**: Automatic database schema migration.
- **Resilient Connections**: Connection retry, pooling, and error handling.

## Configuration

The PostgreSQL provider can be configured through appsettings.json or directly in code:

### appsettings.json Example

```json
{
  "Repository": {
    "Provider": "PostgreSQL",
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
  }
}
```

### Code Configuration

```csharp
// Direct PostgreSQL configuration
services.AddPostgreSqlRepositories(options => {
    options.ConnectionString = "Host=localhost;Database=FluentCMS;Username=postgres;Password=yourpassword;";
    options.UseJsonForComplexTypes = true;
    options.EnableRetryOnFailure = true;
    options.MaxRetryCount = 5;
});

// Using the repository factory
services.AddRepositoryFactory(options => {
    options.Provider = "PostgreSQL";
    options.PostgreSQL.ConnectionString = "Host=localhost;Database=FluentCMS;Username=postgres;Password=yourpassword;";
    options.PostgreSQL.UseJsonForComplexTypes = true;
});
```

## Connection String Format

PostgreSQL connection strings follow this format:

```
Host=hostname;Database=dbname;Username=user;Password=pass;Port=5432;
```

Common parameters:

| Parameter | Description | Default |
|-----------|-------------|---------|
| Host | The PostgreSQL server hostname | localhost |
| Database | The database name | None (Required) |
| Username | The database user | None (Required) |
| Password | The user's password | None (Required) |
| Port | The server port | 5432 |
| SSL Mode | The SSL connection mode (Disable, Prefer, Require) | Prefer |
| Timeout | Command timeout in seconds | 30 |
| Pooling | Whether to use connection pooling | true |
| Maximum Pool Size | Maximum connections in the pool | 100 |
| Minimum Pool Size | Minimum connections to maintain | 0 |

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| ConnectionString | The PostgreSQL connection string | Required |
| ServerVersion | PostgreSQL server version (e.g., "15.0") | Auto-detected |
| Schema | The database schema to use | "public" |
| UseConnectionPooling | Whether to use connection pooling | true |
| MaxPoolSize | Maximum number of connections in the pool | 100 |
| MinPoolSize | Minimum number of connections to maintain | 0 |
| ConnectionTimeout | Command timeout in seconds | 30 |
| UseSsl | Whether to use SSL for connections | false |
| SslMode | SSL mode (Disable, Allow, Prefer, Require, VerifyCA, VerifyFull) | "Prefer" |
| EnableRetryOnFailure | Whether to retry failed commands | true |
| MaxRetryCount | Maximum number of retry attempts | 5 |
| EnableBatchCommands | Whether to enable batched commands | true |
| UseJsonForComplexTypes | Store complex properties as JSONB | false |
| UseCaseInsensitiveCollation | Use case-insensitive collation for text columns | true |
| UseCamelCaseTableNames | Use camelCase for table names | false |
| TableNamePrefix | Prefix for table names | "" |
| TableNameSuffix | Suffix for table names | "" |
| UsePluralTableNames | Use plural form for table names | true |
| AutoMigrateDatabase | Automatically run database migrations | false |

## PostgreSQL-Specific Features

### JSON Storage

When `UseJsonForComplexTypes` is enabled, complex object properties are stored as JSONB columns in PostgreSQL, allowing for more flexible data models and native JSON querying capabilities.

```csharp
options.UseJsonForComplexTypes = true;
```

### Case-Insensitive Text Search

The provider automatically configures case-insensitive text comparison when `UseCaseInsensitiveCollation` is enabled:

```csharp
options.UseCaseInsensitiveCollation = true;
```

### Schema Management

You can specify a database schema other than the default "public":

```csharp
options.Schema = "fluentcms";
```

## Best Practices

1. **Connection Pooling**: Keep connection pooling enabled with appropriate pool size for your workload.
2. **Retry Configuration**: Enable retry on failure for improved resilience.
3. **JSON Storage**: Use JSON storage for flexible, schema-less properties, but use regular columns for properties you'll frequently query or index.
4. **Schema Design**: Use a dedicated schema for FluentCMS to isolate it from other applications.

## Performance Considerations

- **Indexes**: PostgreSQL provider uses the EF Core migrations system. Consider adding custom migrations to create indexes for frequently queried fields.
- **Connection Pooling**: Adjust pool sizes based on your application's needs.
- **Query Splitting**: The `EnableBatchCommands` option can improve performance for complex queries.
- **Batching**: EF Core batches commands by default, which can significantly improve performance.

## Migrations

### Automatic Migrations

Set `AutoMigrateDatabase` to true to automatically apply migrations at startup:

```csharp
options.AutoMigrateDatabase = true;
```

### Manual Migrations

For more control, you can manage migrations manually:

```bash
dotnet ef migrations add InitialCreate --project FluentCMS.Repositories.PostgreSQL
dotnet ef database update --project FluentCMS.Repositories.PostgreSQL
```

## See Also

- [Repository Pattern](./Repository-Pattern.md) - Overview of FluentCMS repository pattern implementation
- [Database Providers](./Database-Providers.md) - Comparison of available database providers
- [Entity Framework Provider](./EntityFramework-Provider.md) - Details on the base Entity Framework provider
