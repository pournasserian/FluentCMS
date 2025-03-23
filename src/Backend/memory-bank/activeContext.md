# FluentCMS Backend Active Context

## Current Work Focus

The current development focus for the FluentCMS Backend is on:

1. **Entity Framework Core Implementation**
   - Building the base EntityFrameworkEntityRepository<TEntity> class
   - Setting up the FluentCmsDbContext with proper entity configuration
   - Implementing CRUD operations with EF Core tracking
   - Creating configuration options for EF Core providers

2. **SQLite Provider Implementation**
   - Extending the Entity Framework implementation for SQLite
   - Setting up SQLite-specific connection handling
   - Creating SQLite database initialization and migration
   - Optimizing for embedded database scenarios

3. **SQL Server Provider Implementation**
   - Adapting the Entity Framework implementation for SQL Server
   - Configuring SQL Server-specific options
   - Setting up proper connection string handling
   - Ensuring compatibility with various SQL Server versions

## Recent Changes

1. **Entity Framework Implementation**
   - Created EntityFrameworkOptions.cs for configuration
   - Implemented FluentCmsDbContext with DbSet<T> for entities
   - Started implementing EntityFrameworkEntityRepository<TEntity>
   - Added service collection extension methods for registration

2. **SQLite Implementation**
   - Created SqliteOptions.cs for SQLite-specific configuration
   - Implemented SqliteDbContext extending FluentCmsDbContext
   - Added service collection extensions for SQLite registration

3. **SQL Server Implementation**
   - Created SqlServerOptions.cs for SQL Server configuration
   - Implemented SqlServerDbContext extending FluentCmsDbContext
   - Added service collection extensions for SQL Server registration

## Next Steps

1. **Complete Entity Framework Implementation**
   - Finish CRUD operations in EntityFrameworkEntityRepository<TEntity>
   - Implement proper tracking and change detection
   - Add transaction support
   - Create integration tests

2. **Enhance SQLite Provider**
   - Implement SQLite-specific optimizations
   - Set up database file creation and initialization
   - Test with in-memory SQLite for unit testing

3. **Finalize SQL Server Provider**
   - Add SQL Server-specific configurations
   - Implement connection pooling optimizations
   - Test with local and cloud SQL Server instances

## Current Challenges

1. **Cross-Provider Consistency**
   - Ensuring that Entity Framework providers behave consistently with MongoDB and LiteDB
   - Managing differences in transaction support across providers
   - Handling identity generation consistently

2. **Entity Framework Abstractions**
   - Balancing EF Core features with repository abstraction
   - Avoiding leaky abstractions while utilizing EF Core capabilities
   - Maintaining consistent async patterns across providers

3. **Performance Considerations**
   - Optimizing query performance for each provider
   - Proper handling of connection management
   - Balancing memory usage with performance

## Active Design Decisions

### Entity Framework Core Integration

**Decision Context:** How to integrate Entity Framework Core with the repository pattern while maintaining clean abstractions.

**Options Considered:**
- Direct Entity Framework usage with minimal abstraction
- Full repository abstraction with limited EF Core features
- Hybrid approach with repository pattern but provider-specific extensions

**Current Direction:**
Implementing a hybrid approach that uses the repository pattern for CRUD operations but allows for provider-specific extensions when needed for advanced scenarios.

### SQLite Configuration

**Decision Context:** How to handle SQLite database creation and initialization.

**Options Considered:**
- Code-first migrations
- Manual schema creation
- Hybrid approach with basic schema and runtime enhancements

**Current Direction:**
Using code-first migrations for schema creation, with options for runtime schema verification and updates.

### SQL Server Provider Features

**Decision Context:** Which SQL Server-specific features to support.

**Options Considered:**
- Basic implementation with standard features only
- Comprehensive implementation with SQL Server-specific optimizations
- Configurable approach based on connection string capabilities

**Current Direction:**
Implementing a configurable approach that adapts to the capabilities of the connected SQL Server instance, enabling advanced features when available.

## Relation to Main Project Focus

This Backend sub-project work directly supports the current active context of the main FluentCMS project, specifically the "Complete Additional Repository Implementations" focus area mentioned in [the main activeContext.md](../../memory-bank/activeContext.md).
