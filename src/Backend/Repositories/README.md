# FluentCMS Repository Implementations

This directory contains the repository pattern implementations for different database providers in the FluentCMS system. Each provider implements the same interface but is optimized for its specific database technology.

## Key Components

- **Abstractions**
  - `FluentCMS.Repositories.Abstractions` - Core repository interfaces and patterns

- **Concrete Implementations**
  - `FluentCMS.Repositories.MongoDB` - MongoDB implementation
  - `FluentCMS.Repositories.LiteDB` - LiteDB implementation
  - `FluentCMS.Repositories.EntityFramework` - Entity Framework Core base implementation
  - `FluentCMS.Repositories.SQLite` - SQLite implementation via Entity Framework
  - `FluentCMS.Repositories.SqlServer` - SQL Server implementation via Entity Framework
  - `FluentCMS.Repositories.MySQL` - MySQL implementation via Pomelo Entity Framework

## Repository Pattern

All implementations follow the repository pattern, providing a consistent abstraction over data access operations:

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

## Choosing a Provider

Each provider has different characteristics:

- **MongoDB**: Scalable document database, ideal for flexible schemas and high write loads
- **LiteDB**: Embedded NoSQL database, perfect for small applications with minimal setup
- **Entity Framework**: Familiar ORM experience with strong LINQ support
- **SQLite**: Lightweight file-based SQL database with EF Core support
- **SQL Server**: Full-featured relational database with advanced capabilities
- **MySQL**: Popular open-source relational database with wide community support

## Configuration Examples

### MongoDB

```csharp
services.AddMongoDbRepositories(options =>
{
    options.ConnectionString = "mongodb://localhost:27017";
    options.DatabaseName = "FluentCMS";
    options.UseCamelCaseCollectionNames = true;
});
```

### LiteDB

```csharp
services.AddLiteDbRepositories(options =>
{
    options.ConnectionString = "Filename=fluentcms.db;Connection=shared";
});
```

### Entity Framework (SQL Server)

```csharp
services.AddSqlServerRepositories(options =>
{
    options.ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=FluentCMS;Trusted_Connection=True;";
});
```

### SQLite

```csharp
services.AddSqliteRepositories(options =>
{
    options.ConnectionString = "Data Source=fluentcms.db";
});
```

### MySQL

```csharp
services.AddMySqlRepositories(options =>
{
    options.ConnectionString = "Server=localhost;Database=FluentCMS;User=root;Password=password;";
    options.ServerVersion = "8.0.28"; // Optional specific version
});
```

## Memory Bank

This sub-project maintains its own memory bank:

- [Project Brief](./memory-bank/projectbrief.md) - Repository-specific goals and requirements
- [Active Context](./memory-bank/activeContext.md) - Current repository development focus
- [Technical Context](./memory-bank/techContext.md) - Repository implementation technologies
- [Progress](./memory-bank/progress.md) - Repository implementation progress

See the [Backend memory bank](../memory-bank/) and [main FluentCMS memory bank](../../../memory-bank/) for additional context.

## Development Status

- ✅ Repository abstraction layer complete
- ✅ MongoDB provider complete
- ✅ LiteDB provider complete
- ✅ MySQL provider complete
- 🚧 Entity Framework Core provider in progress
- 🚧 SQLite provider in progress
- 🚧 SQL Server provider in progress
