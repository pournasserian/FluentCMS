# Database Providers in FluentCMS

## Overview

One of the core features of FluentCMS is its support for multiple database providers through a unified repository pattern. This approach gives developers the flexibility to choose the database technology that best fits their specific needs, without changing application code.

## Supported Providers

FluentCMS currently supports the following database providers:

| Provider | Status | Implementation | Use Cases |
|----------|--------|----------------|-----------|
| MongoDB | ✅ Complete | `FluentCMS.Repositories.MongoDB` | Document storage, scalability, flexible schema |
| LiteDB | ✅ Complete | `FluentCMS.Repositories.LiteDB` | Embedded database, serverless, simple deployment |
| Entity Framework Core | 🚧 In Progress | `FluentCMS.Repositories.EntityFramework` | ORM, relational databases, LINQ support |
| SQLite | 🚧 In Progress | `FluentCMS.Repositories.SQLite` | File-based SQL, embedded, cross-platform |
| SQL Server | 🚧 In Progress | `FluentCMS.Repositories.SqlServer` | Enterprise SQL, performance, scalability |

## Provider Characteristics

### MongoDB Provider

**Technology:** MongoDB document database

**Key Features:**
- Schema-less document storage
- High performance for write-heavy workloads
- Horizontal scaling through sharding
- Flexible indexing and querying
- Rich query capabilities with aggregation pipeline

**Ideal For:**
- Content repositories with flexible schemas
- High write volume applications
- Systems that need horizontal scaling
- Projects with evolving data models

**Configuration Example:**
```csharp
services.AddMongoDbRepositories(options =>
{
    options.ConnectionString = "mongodb://localhost:27017";
    options.DatabaseName = "FluentCMS";
    options.UseCamelCaseCollectionNames = true;
});
```

### LiteDB Provider

**Technology:** Embedded NoSQL document database

**Key Features:**
- Single file database
- Zero configuration
- Embedded engine (no server required)
- Document-based storage with BSON format
- Small footprint (~350kb)

**Ideal For:**
- Desktop applications
- Simple deployments
- Prototyping and development
- Applications with modest data requirements
- Serverless scenarios

**Configuration Example:**
```csharp
services.AddLiteDbRepositories(options =>
{
    options.ConnectionString = "Filename=fluentcms.db;Connection=shared";
});
```

### Entity Framework Core Provider

**Technology:** Object-Relational Mapper (ORM) for .NET

**Key Features:**
- Works with multiple relational databases
- Strong LINQ support for queries
- Change tracking for efficient updates
- Migration support for schema evolution
- Rich mapping capabilities

**Ideal For:**
- Applications with complex relational data
- Projects requiring strong data consistency
- Systems with well-defined schemas
- Integration with existing relational databases

**Configuration Example:**
```csharp
services.AddEntityFrameworkRepositories(options =>
{
    options.UseProvider = "SqlServer"; // or "SQLite", etc.
    options.ConnectionString = "your-connection-string";
});
```

### SQLite Provider

**Technology:** File-based SQL database

**Key Features:**
- Self-contained, serverless database
- Cross-platform compatibility
- Single file storage
- ACID-compliant transactions
- In-memory capability for testing

**Ideal For:**
- Local applications
- Cross-platform deployments
- Development and testing
- Applications with moderate concurrency needs
- Edge computing scenarios

**Configuration Example:**
```csharp
services.AddSqliteRepositories(options =>
{
    options.ConnectionString = "Data Source=fluentcms.db";
});
```

### SQL Server Provider

**Technology:** Microsoft's enterprise relational database

**Key Features:**
- Enterprise-grade reliability
- Advanced performance features
- Comprehensive security model
- Advanced querying capabilities
- Rich ecosystem of tools

**Ideal For:**
- Enterprise applications
- Systems with complex transactions
- High concurrency environments
- Applications requiring advanced SQL features
- Integration with Microsoft ecosystem

**Configuration Example:**
```csharp
services.AddSqlServerRepositories(options =>
{
    options.ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=FluentCMS;Trusted_Connection=True;";
});
```

## Provider Selection Criteria

When choosing a database provider for your FluentCMS application, consider the following factors:

1. **Deployment Environment**
   - Cloud, on-premises, edge, or embedded
   - Operating system and platform constraints
   - Available resources (memory, CPU, storage)

2. **Data Characteristics**
   - Schema rigidity vs. flexibility needs
   - Relationship complexity
   - Expected data volume and growth
   - Query patterns and complexity

3. **Operational Requirements**
   - Scaling strategy (vertical vs. horizontal)
   - Backup and recovery needs
   - High availability requirements
   - Monitoring and management capabilities

4. **Development Considerations**
   - Team familiarity with the technology
   - Debugging and development tools
   - Testing approach
   - Migration and schema evolution strategy

## Switching Providers

One of the key benefits of FluentCMS's architecture is the ability to switch database providers with minimal code changes. To switch providers:

1. Add the NuGet package for the desired provider
2. Update the configuration in `Startup.cs` or `Program.cs`
3. Migrate your data (if needed)

No changes to your business logic or service layer are required when switching providers.

## Implementation Details

Each provider implements the following repository interface:

```csharp
public interface IBaseEntityRepository<TEntity> where TEntity : class, IBaseEntity
{
    Task<TEntity?> Create(TEntity entity);
    Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities);
    Task<TEntity?> Update(TEntity entity);
    Task<IEnumerable<TEntity>> UpdateMany(IEnumerable<TEntity> entities);
    Task<TEntity?> Delete(Guid id);
    Task<IEnumerable<TEntity>> DeleteMany(IEnumerable<Guid> ids);
    Task<IEnumerable<TEntity>> GetAll();
    Task<TEntity?> GetById(Guid id);
    Task<IEnumerable<TEntity>> GetByIds(IEnumerable<Guid> ids);
}
```

The implementations handle database-specific concerns while maintaining consistent behavior.

## Provider-Specific Documentation

For detailed implementation information on each provider, see the following pages:

- [MongoDB Provider](./MongoDB-Provider.md)
- [LiteDB Provider](./LiteDB-Provider.md)
- [EntityFramework Provider](./EntityFramework-Provider.md)
- [SQLite Provider](./SQLite-Provider.md)
- [SQL Server Provider](./SQL-Server-Provider.md)

## See Also

- [Repository Pattern](./Repository-Pattern.md) - How the repository pattern is implemented
- [Configuration Guide](./Configuration-Guide.md) - Detailed configuration options
- [Entity Model](./Entity-Model.md) - Entity structure and design
- [Home](./Home.md) - Return to wiki home
