# FluentCMS Technical Context

## Technologies Used

### Core Technologies

1. **ASP.NET Core**: The foundation framework for building the CMS backend.
   - Version: .NET Core 8.0 (inferred from project structure)
   - Used for API infrastructure, dependency injection, and configuration

2. **C#**: The primary programming language used for backend development.
   - Version: C# 12.0 (inferred)
   - Using latest language features like nullable reference types

### Data Access Technologies

1. **Repository Pattern**: Core architectural pattern for data access abstraction.
   - Generic repositories with CRUD operations
   - Specific implementations for various database providers

2. **MongoDB .NET Driver**: Client library for MongoDB integration.
   - Used in the MongoDB repository implementation
   - Provides access to MongoDB collections and CRUD operations

3. **LiteDB**: Embedded NoSQL database for .NET.
   - Used in the LiteDB repository implementation
   - Simple document-oriented database with minimal setup

4. **Entity Framework Core**: ORM framework for .NET (placeholder).
   - Will be used for SQL-based database providers
   - Provides object-relational mapping capabilities

### Upcoming/Planned Technologies

1. **ASP.NET Core Web API**: For creating RESTful API endpoints.
   - Will expose CMS functionality through HTTP interface
   - Will handle authentication, authorization, and API versioning

2. **Blazor Server**: For building the interactive admin UI.
   - Will provide server-rendered UI with SignalR communication
   - Will communicate with the CMS backend API

3. **Identity Server**: For authentication and authorization.
   - Will secure API access with OAuth 2.0/OpenID Connect
   - Will manage user authentication and permissions

## Development Setup

### Required Tools

1. **Visual Studio 2022** or **Visual Studio Code**
   - With .NET Core SDK 8.0 or later
   - C# extension and relevant tooling

2. **Database Tools** (depending on chosen provider)
   - MongoDB Compass for MongoDB
   - LiteDB Viewer for LiteDB
   - SQL Server Management Studio for SQL Server
   - DB Browser for SQLite

### Development Environment Setup

1. **Clone repository** from source control
2. **Restore NuGet packages**
3. **Configure database connection** based on chosen provider
4. **Run database migrations** (for EF Core providers)
5. **Launch application** in development mode

### Configuration Requirements

1. **appsettings.json** (To be configured)
   - Database connection strings
   - Logging settings
   - Environment-specific configuration

2. **Database-specific configuration**
   - MongoDB: Connection string, database name
   - LiteDB: File path
   - SQL Server: Connection string
   - SQLite: Database file path

## Technical Constraints

### Performance Constraints

1. **Asynchronous Operations**
   - All repository methods must be asynchronous
   - Must use `async`/`await` pattern consistently

2. **Error Handling**
   - Repositories should never throw exceptions directly to callers
   - All exceptions should be logged properly
   - Failed operations should return null or empty collections

3. **Transaction Support**
   - Must handle repository operations that require transaction support
   - Must account for differences in transaction support across providers

### Security Constraints

1. **Data Protection**
   - Sensitive data should be properly encrypted
   - No hardcoded credentials or connection strings in code

2. **Authentication/Authorization** (To be implemented)
   - API endpoints must be secured with appropriate authentication
   - Authorization policies must be enforced consistently

### Compatibility Constraints

1. **Database Compatibility**
   - Each repository implementation must be thoroughly tested
   - Must account for feature differences between database providers

2. **API Consistency**
   - API behavior must be consistent across database providers
   - Repository operations must return equivalent results regardless of provider

## Dependencies

### NuGet Packages

1. **Core Packages**
   - Microsoft.Extensions.DependencyInjection
   - Microsoft.Extensions.Configuration
   - Microsoft.Extensions.Logging
   - Microsoft.Extensions.Options

2. **MongoDB Packages**
   - MongoDB.Driver
   - MongoDB.Bson

3. **LiteDB Packages**
   - LiteDB

4. **Entity Framework Packages** (Planned)
   - Microsoft.EntityFrameworkCore
   - Provider-specific packages (SqlServer, SQLite)

### External Dependencies

1. **Database Systems**
   - MongoDB server (for MongoDB provider)
   - SQL Server (for SQL Server provider)
   - File system access (for LiteDB and SQLite providers)

## Development Standards

1. **Code Style**
   - Follow C# coding conventions
   - Use nullable reference types

2. **Testing**
   - Unit tests for all core functionality
   - Integration tests for repository implementations
   - Mock repositories for service testing

3. **Source Control**
   - Git for version control
   - Feature branch workflow
   - Pull requests for code reviews

4. **Documentation**
   - README files for project overview
   - Detailed API documentation
