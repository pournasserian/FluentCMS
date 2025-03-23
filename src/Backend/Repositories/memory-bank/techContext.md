# FluentCMS Repositories Technical Context

## Core Technologies

### Repository Pattern Implementation

- **Base Interface**: `IBaseEntityRepository<TEntity>`
- **Type Constraints**: `where TEntity : class, IBaseEntity`
- **Return Types**: Async methods with Task/Task<T> return types
- **Error Handling**: Try/catch with logging, null returns for failures

### Common Interface Structure

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

## Document Database Technologies

### MongoDB Provider

- **Core Library**: MongoDB.Driver
- **Connection Management**: MongoClient with connection pooling
- **Data Model**: BSON documents
- **ID Handling**: Guid mapped to MongoDB ObjectId
- **Collection Naming**: Configurable (default, camelCase, or custom)
- **Configuration**: MongoDbOptions with connection string and database name
- **Key Implementation Classes**:
  - `MongoDbEntityRepository<TEntity>`
  - `MongoDbOptions`

### LiteDB Provider

- **Core Library**: LiteDB
- **Database Type**: Embedded NoSQL document database
- **Data Model**: BSON documents
- **Storage**: Single file database
- **Thread Safety**: Managed through connection modes
- **Configuration**: LiteDbOptions with file path and share mode
- **Key Implementation Classes**:
  - `LiteDbEntityRepository<TEntity>`
  - `LiteDbOptions`

## Relational Database Technologies

### Entity Framework Core Base

- **Core Library**: Microsoft.EntityFrameworkCore
- **Data Access**: DbContext with DbSet<T>
- **Query Building**: LINQ to Entities
- **Change Tracking**: DbContext.ChangeTracker
- **Configuration**: EntityFrameworkOptions
- **Key Implementation Classes**:
  - `FluentCmsDbContext`
  - `EntityFrameworkEntityRepository<TEntity>`
  - `EntityFrameworkOptions`

### SQLite Provider

- **Core Library**: Microsoft.EntityFrameworkCore.Sqlite
- **Database Type**: File-based SQL database
- **Connection**: SqliteConnection
- **Storage**: Single file or in-memory
- **Configuration**: SqliteOptions with database path
- **Key Implementation Classes**:
  - `SqliteDbContext`
  - `SqliteOptions`

### SQL Server Provider

- **Core Library**: Microsoft.EntityFrameworkCore.SqlServer
- **Database Type**: Client/server relational database
- **Connection**: SqlConnection with connection pooling
- **Features**: Full SQL Server capabilities
- **Configuration**: SqlServerOptions with connection string
- **Key Implementation Classes**:
  - `SqlServerDbContext`
  - `SqlServerOptions`

## Technical Implementation Details

### MongoDB Implementation

```csharp
public class MongoDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly ILogger<MongoDbEntityRepository<TEntity>> _logger;
    
    // Implementation of CRUD operations using MongoDB driver
}
```

**Key Technical Approaches**:
- Collection per entity type
- FilterDefinition<T> for queries
- BsonDocument for projections
- FindOneAndReplace for atomic updates

### LiteDB Implementation

```csharp
public class LiteDbEntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<TEntity> _collection;
    private readonly ILogger<LiteDbEntityRepository<TEntity>> _logger;
    
    // Implementation of CRUD operations using LiteDB
}
```

**Key Technical Approaches**:
- Single LiteDatabase instance
- Collection per entity type
- BsonExpression for queries
- Index management for performance

### Entity Framework Implementation

```csharp
public class EntityFrameworkEntityRepository<TEntity> : IBaseEntityRepository<TEntity> 
    where TEntity : class, IBaseEntity
{
    private readonly FluentCmsDbContext _context;
    private readonly DbSet<TEntity> _entities;
    private readonly ILogger<EntityFrameworkEntityRepository<TEntity>> _logger;
    
    // Implementation of CRUD operations using EF Core
}
```

**Key Technical Approaches**:
- DbContext with DbSet<T> per entity
- Change tracking for efficient updates
- LINQ for queries
- SaveChanges/SaveChangesAsync for transaction handling

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

## Technical Considerations

### 1. Entity Tracking

**MongoDB & LiteDB**:
- No built-in change tracking
- Full document replacement on update
- Explicit handling of concurrent modifications

**Entity Framework Core**:
- Built-in change tracking
- Partial updates possible
- Concurrency handling through concurrency tokens

### 2. Transaction Support

**MongoDB**:
- Multi-document transactions in replica sets
- Single-document atomicity guaranteed
- Session-based transaction API

**LiteDB**:
- Transaction per operation
- Manual transaction blocks
- Limited multi-document transaction support

**Entity Framework Core**:
- Full transaction support
- DbContext.Database.BeginTransaction()
- Distributed transaction support where available

### 3. Query Capabilities

**MongoDB**:
- Rich query API
- Aggregation pipeline
- Text search
- Geospatial queries

**LiteDB**:
- Basic query capabilities
- LINQ-like syntax with limitations
- Index support for common scenarios

**Entity Framework Core**:
- Full LINQ support
- Provider-specific translations
- Eager and lazy loading
- Complex joins and includes

## Database-Specific Optimizations

### MongoDB Optimizations

- Index creation for frequently queried fields
- Document projection to limit returned fields
- Batch operations for bulk modifications
- Read preferences for replica sets

### LiteDB Optimizations

- Index management for query performance
- Auto-increment ID for certain scenarios
- File size management
- Memory-mapped file configuration

### SQL-Based Optimizations

- Appropriate index creation
- Query plan analysis
- Connection pooling
- Batched operations

## Relation to Backend Tech Context

This Repositories sub-project implements the data access components described in the [Backend techContext.md](../../memory-bank/techContext.md), providing more detailed technical information about each database provider implementation.
