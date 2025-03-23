# FluentCMS Sub-Project Registry

This file serves as a central registry for all sub-projects within the FluentCMS ecosystem. It helps maintain context when working on specific components of the larger project.

## Active Sub-Projects

### Backend Components

- [Backend Core](../src/Backend) - Core backend services and entity framework
  - **Purpose**: Provides the foundation for all data access and API components
  - **Status**: In active development
  - **Primary Focus**: Repository pattern implementation across multiple database providers

### Repository Implementations

- [Repositories](../src/Backend/Repositories) - Data access layer implementations
  - **Purpose**: Implements the repository pattern for different database providers
  - **Status**: Partially implemented (MongoDB, LiteDB complete; EF Core, SQLite, SQL Server in progress)
  - **Primary Focus**: Ensure consistent API behavior across all database providers

#### Specific Repository Providers

- [MongoDB Repository](../src/Backend/Repositories/FluentCMS.Repositories.MongoDB) - MongoDB implementation
- [LiteDB Repository](../src/Backend/Repositories/FluentCMS.Repositories.LiteDB) - LiteDB implementation
- [Entity Framework Repository](../src/Backend/Repositories/FluentCMS.Repositories.EntityFramework) - EF Core implementation
- [SQLite Repository](../src/Backend/Repositories/FluentCMS.Repositories.SQLite) - SQLite implementation
- [SQL Server Repository](../src/Backend/Repositories/FluentCMS.Repositories.SqlServer) - SQL Server implementation

## Planned Sub-Projects

### API Layer

- [API](../src/API) - RESTful API endpoints for content management
  - **Purpose**: Expose content management capabilities through standard HTTP endpoints
  - **Status**: Planned
  - **Primary Focus**: RESTful design, authentication, documentation

### Frontend Components

- [Admin UI](../src/Frontend) - Blazor-based administration interface
  - **Purpose**: Provide a user-friendly interface for content management
  - **Status**: Planned
  - **Primary Focus**: Intuitive UX, accessibility, responsive design

## Sub-Project References

Each sub-project maintains its own memory bank for project-specific context:

```
/FluentCMS/ (Main Project)
  ├── memory-bank/ (Main Memory Bank)
  └── src/
      ├── Backend/
      │   ├── memory-bank/ (Backend Memory Bank)
      │   └── Repositories/
      │       └── memory-bank/ (Repositories Memory Bank)
      └── ...
```

This hierarchical structure allows for:
1. High-level project context at the root level
2. Component-specific details at the sub-project level
3. Implementation-specific information at the implementation level
