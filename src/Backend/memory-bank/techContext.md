# FluentCMS Backend Technical Context

## Core Technologies

### .NET Core and C#

- **Version:** .NET 8.0
- **Language:** C# 12.0
- **Key Features:** 
  - Nullable reference types
  - Async/await patterns
  - Generic type constraints
  - Expression trees (for query building)

### Entity Framework Core

- **Version:** Latest EF Core 8.0
- **Usage:** ORM for SQL-based database providers
- **Key Features:**
  - DbContext for database session management
  - LINQ provider for query building
  - Change tracking for efficient updates
  - Migrations for schema management

### MongoDB Driver

- **Version:** MongoDB.Driver (latest stable)
- **Usage:** NoSQL document database provider
- **Key Features:**
  - Collection-based data access
  - BSON serialization
  - Aggregation pipeline
  - Index management

### LiteDB

- **Version:** LiteDB (latest stable)
- **Usage:** Embedded document database provider
- **Key Features:**
  - Serverless operation
  - Document-oriented model
  - Index support
  - ACID transactions

## Database Provider Specifics

### MongoDB Provider

- Connection management through MongoClient
- Document mapping through BsonClassMap
- Query building with FilterDefinition<T>
- Single collection per entity type

### LiteDB Provider

- File-based database with LiteDatabase
- Collection access via GetCollection<T>
- Direct document serialization
- Thread-safe operations with shared connection

### Entity Framework Provider

- DbContext with DbSet<T> for entity access
- LINQ for query building
- Change tracking for update efficiency
- Connection management through DbContextOptionsBuilder

### SQLite Provider

- Entity Framework Core with SQLite provider
- File-based database operation
- In-memory capability for testing
- Connection configuration through SqliteConnection

### SQL Server Provider

- Entity Framework Core with SQL Server provider
- Connection pooling for performance
- Distributed transaction support
- SQL Server-specific optimizations

## Repository Pattern Implementation

### Core Interface: IBaseEntityRepository<TEntity>

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

### Common Implementation Patterns

- Constructor-based dependency injection
- Options pattern for configuration
- ILogger for diagnostic logging
- Exception handling with detailed error information
- Consistent async/await pattern

## Technical Constraints

### Performance Requirements

- Efficient batch operations
- Minimal memory overhead
- Proper connection management
- Optimized query execution

### Error Handling

- Consistent exception handling across providers
- Detailed logging of errors
- Non-leaky abstraction (repository errors vs. database errors)
- Graceful degradation when possible

### Transaction Support

- Atomic operations for all providers
- Multi-entity transactions where supported
- Compensating transactions for providers with limited support
- Consistent transaction behavior across providers

## Development Tools

- Visual Studio 2022 / Visual Studio Code
- NuGet for package management
- xUnit for unit and integration testing
- Moq for mocking in tests

## Implementation Considerations

### Entity Tracking

- MongoDB: No built-in tracking, manual implementation
- LiteDB: No built-in tracking, manual implementation
- EF Core: Built-in tracking with DbContext.ChangeTracker

### Query Capabilities

- MongoDB: Rich query API with FilterDefinition<T>
- LiteDB: LINQ-like query API with limitations
- EF Core: Full LINQ support with provider-specific translations

### Identity Management

- MongoDB: Manual ID generation, typically using Guid
- LiteDB: BsonId attribute, auto-generation supported
- EF Core: Key attribute, various generation strategies

### Relationship Handling

- MongoDB: References or embedded documents
- LiteDB: References or embedded documents
- EF Core: Navigation properties with foreign keys

## Relation to Main Tech Stack

This Backend sub-project implements the data access components described in the [main techContext.md](../../memory-bank/techContext.md), specifically focusing on the repository pattern implementation and database provider integration.
