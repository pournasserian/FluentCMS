# FluentCMS Active Context

## Current Work Focus

The current development focus is on building the core backend components of the FluentCMS system. Specifically, the work is centered on:

1. **Repository Pattern Implementation**
   - Implementing and refining the repository abstraction layer
   - Developing concrete implementations for different database providers
   - Ensuring consistent behavior across all repository implementations

2. **Entity Structure**
   - Defining core entity interfaces and base classes
   - Implementing audit tracking capabilities
   - Setting up the foundation for future content-specific entities

3. **Database Provider Support**
   - MongoDB implementation (near complete)
   - LiteDB implementation (near complete)
   - Preparing for Entity Framework, SQLite, and SQL Server implementations

## Recent Changes

1. **Repository Infrastructure**
   - Implemented `IBaseEntityRepository<TEntity>` interface defining standard CRUD operations
   - Created base entity classes with ID and audit fields
   - Set up repository pattern with generic type constraints

2. **MongoDB Implementation**
   - Implemented full CRUD operations with MongoDB driver
   - Added configuration options for connection, database name, and collection naming
   - Included error handling and logging

3. **LiteDB Implementation**
   - Implemented repository operations using LiteDB
   - Added configuration options
   - Set up proper resource management for database connections

## Next Steps

The following tasks are planned for the immediate future:

1. **Complete Additional Repository Implementations**
   - Implement Entity Framework Core repository
   - Finish SQLite repository implementation
   - Complete SQL Server repository implementation

2. **Content Type System**
   - Design and implement a content type definition system
   - Create a flexible schema for defining content structure
   - Develop validation rules for content type constraints

3. **API Layer Development**
   - Set up ASP.NET Core Web API project
   - Implement controllers for entity operations
   - Create endpoints for content type management

4. **Service Layer**
   - Design service interfaces for business operations
   - Implement services that orchestrate repository operations
   - Add validation and business rule enforcement

## Active Decisions and Considerations

### 1. Repository Pattern Extensions

**Decision Context:** The current repository pattern implements basic CRUD operations, but we need to consider how to extend this for:
- Advanced querying capabilities
- Pagination
- Sorting
- Filtering

**Options Being Considered:**
- Extend the base repository interface with additional methods
- Create specialized query objects/specifications
- Use extension methods for advanced querying
- Implement a CQRS pattern for more complex operations

**Current Thinking:**
The team is leaning toward using a combination of specification pattern and extension methods to keep the base interface clean while enabling advanced querying capabilities.

### 2. Content Type System Design

**Decision Context:** We need a flexible way to define content types that works across different database providers.

**Options Being Considered:**
- Document-based schema with JSON validation
- Code-first approach with dynamic compilation
- Database-stored schemas with validation at the service layer
- Hybrid approach combining database-stored schemas with code generation

**Current Thinking:**
A document-based schema stored in the database with a service-layer validation approach seems most promising for flexibility and cross-database compatibility.

### 3. Authentication and Authorization

**Decision Context:** The system needs a secure authentication and authorization mechanism.

**Options Being Considered:**
- Built-in ASP.NET Core Identity
- Identity Server integration
- Custom authentication with JWT
- External identity provider integration (Auth0, Azure AD)

**Current Thinking:**
ASP.NET Core Identity with JWT token support for API authentication provides a good balance of built-in functionality and customization options.

### 4. Repository Transaction Support

**Decision Context:** Different database providers have varying transaction support capabilities.

**Options Being Considered:**
- Implement unit of work pattern
- Use database-specific transaction mechanisms
- Design services to be transaction-aware
- Implement compensating transactions for providers with limited support

**Current Thinking:**
We're considering implementing a unit of work pattern that adapts to the capabilities of each database provider, with fallback strategies for providers with limited transaction support.

## Current Technical Challenges

1. **Cross-Provider Consistency**
   - Ensuring consistent behavior across different database implementations
   - Handling provider-specific features without leaking abstractions
   - Testing strategies for verifying equivalent behavior

2. **Performance Optimization**
   - Balancing abstraction with performance
   - Optimizing query execution for each provider
   - Handling large datasets efficiently

3. **Schema Evolution**
   - Designing for content type schema changes over time
   - Migration strategies for different database providers
   - Versioning of content types and content
