# Repository Pattern in FluentCMS

## Overview

The repository pattern is a fundamental architectural component in FluentCMS, providing an abstraction layer between the data access logic and the business logic. This pattern allows for database provider flexibility while maintaining a consistent API.

## Purpose and Benefits

The repository pattern in FluentCMS serves several key purposes:

1. **Abstraction**: Hides the details of data persistence from the business logic
2. **Consistency**: Provides a uniform interface regardless of the underlying database technology
3. **Testability**: Enables easy mocking and unit testing of data access code
4. **Flexibility**: Allows swapping database providers without changing application code
5. **Separation of Concerns**: Isolates data access logic from business logic

## Core Interface

All repository implementations in FluentCMS implement the `IBaseEntityRepository<TEntity>` interface:

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

Key aspects of this interface:

- **Generic Type Constraint**: Ensures entities implement `IBaseEntity`
- **Asynchronous Operations**: All methods return `Task` for async/await pattern
- **Nullable Return Types**: Uses C# nullable reference types for potential null returns
- **CRUD Operations**: Provides complete Create, Read, Update, Delete functionality
- **Batch Operations**: Supports both single and multiple entity operations

## Implementation Pattern

Each database provider implements this interface in its own way, following a common pattern:

```csharp
public class [Provider]EntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    // Provider-specific fields
    private readonly [ProviderSpecificType] _connection;
    private readonly ILogger<[Provider]EntityRepository<TEntity>> _logger;
    
    // Constructor with dependency injection
    public [Provider]EntityRepository([ProviderSpecificType] connection, 
                                     ILogger<[Provider]EntityRepository<TEntity>> logger)
    {
        _connection = connection;
        _logger = logger;
    }
    
    // Implementation of interface methods...
}
```

## Provider Registration Pattern

All providers follow a consistent registration pattern using extension methods on IServiceCollection:

```csharp
// MongoDB registration
services.AddMongoDbRepositories(options => {
    options.ConnectionString = "mongodb://localhost:27017";
    options.DatabaseName = "FluentCMS";
});

// LiteDB registration
services.AddLiteDbRepositories(options => {
    options.ConnectionString = "Filename=fluentcms.db;Connection=shared";
});

// EF Core/SQL Server registration
services.AddSqlServerRepositories(options => {
    options.ConnectionString = "Server=.;Database=FluentCMS;Trusted_Connection=True;";
});

// SQLite registration
services.AddSqliteRepositories(options => {
    options.ConnectionString = "Data Source=fluentcms.db";
});
```

This allows for consistent configuration across all providers while accommodating provider-specific options.

## Provider-Specific Implementations

### MongoDB Implementation

```csharp
public class MongoDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly ILogger<MongoDbEntityRepository<TEntity>> _logger;
    
    // Implementation details using MongoDB driver
}
```

**Key MongoDB Techniques**:
- Uses `FilterDefinition<TEntity>` for queries
- Implements atomic updates with `FindOneAndReplace`
- Handles MongoDB-specific ID conversions
- Uses bulk write operations for batch methods

### LiteDB Implementation

```csharp
public class LiteDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<TEntity> _collection;
    private readonly ILogger<LiteDbEntityRepository<TEntity>> _logger;
    
    // Implementation details using LiteDB
}
```

**Key LiteDB Techniques**:
- Uses `ILiteCollection<TEntity>` for entity access
- Implements BsonMapper for document serialization
- Handles file-based database connections
- Manages proper resource disposal

### Entity Framework Implementation (In Progress)

```csharp
public class EntityFrameworkEntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    private readonly FluentCmsDbContext _context;
    private readonly DbSet<TEntity> _entities;
    private readonly ILogger<EntityFrameworkEntityRepository<TEntity>> _logger;
    
    // Implementation details using Entity Framework Core
}
```

**Key EF Core Techniques**:
- Uses `DbSet<TEntity>` for entity access
- Leverages change tracking for efficient updates
- Implements LINQ queries for fetching data
- Handles transaction management

## Handling Cross-Database Concerns

### 1. Entity Tracking

Each provider handles entity tracking differently:

- **MongoDB & LiteDB**: No built-in tracking, so full document replacement on updates
- **Entity Framework**: Uses ChangeTracker for detecting and applying changes

### 2. Transaction Support

Transaction support varies by provider:

- **MongoDB**: Supports multi-document transactions in replica sets
- **LiteDB**: Supports basic transaction blocks
- **Entity Framework**: Full transaction support with savepoints

### 3. ID Generation

Each provider handles ID generation according to its capabilities:

- All providers use GUID/UUID as the primary key type
- ID generation happens at the repository level before persistence
- Entity IDs remain consistent across all providers

## Future Extensions

The repository pattern in FluentCMS is designed for future extension:

1. **Advanced Querying**: Planned specification pattern for complex queries
2. **Pagination Support**: For handling large datasets
3. **Sorting and Filtering**: Standard mechanisms across all providers
4. **Projection Capabilities**: For fetching partial entities

## See Also

- [Architecture Overview](./Architecture-Overview.md) - System architecture and design principles
- [Entity Model](./Entity-Model.md) - Core entity structure and design
- [Database Providers](./Database-Providers.md) - Overview of all database providers
- [Home](./Home.md) - Return to wiki home
