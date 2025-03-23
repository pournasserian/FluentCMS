# FluentCMS Backend

The core backend components of the FluentCMS headless content management system. This sub-project contains the entity models, repository pattern implementations, and database providers.

## Key Components

- **Entity Models**
  - `FluentCMS.Entities` - Core domain entities and base classes

- **Repository Abstractions**
  - `FluentCMS.Repositories.Abstractions` - Repository interfaces and patterns

- **Database Providers**
  - `FluentCMS.Repositories.MongoDB` - MongoDB implementation
  - `FluentCMS.Repositories.LiteDB` - LiteDB implementation
  - `FluentCMS.Repositories.EntityFramework` - Entity Framework Core implementation
  - `FluentCMS.Repositories.SQLite` - SQLite implementation via Entity Framework
  - `FluentCMS.Repositories.SqlServer` - SQL Server implementation via Entity Framework

## Structure

```
Backend/
├── FluentCMS.Entities/            # Core entity models
├── Repositories/
│   ├── FluentCMS.Repositories.Abstractions/   # Interfaces and abstractions
│   ├── FluentCMS.Repositories.MongoDB/        # MongoDB implementation
│   ├── FluentCMS.Repositories.LiteDB/         # LiteDB implementation
│   ├── FluentCMS.Repositories.EntityFramework/ # EF Core implementation
│   ├── FluentCMS.Repositories.SQLite/         # SQLite implementation
│   └── FluentCMS.Repositories.SqlServer/      # SQL Server implementation
└── memory-bank/                   # Backend-specific memory bank
```

## Memory Bank

This sub-project maintains its own memory bank to track backend-specific development context:

- [Project Brief](./memory-bank/projectbrief.md) - Backend-specific goals and requirements
- [Active Context](./memory-bank/activeContext.md) - Current backend development focus
- [Technical Context](./memory-bank/techContext.md) - Backend-specific technologies
- [Progress](./memory-bank/progress.md) - Backend implementation progress

See the [main FluentCMS memory bank](../../memory-bank/) for overall project context.

## Development Status

- ✅ Entity models - Core models complete
- ✅ Repository abstractions - Interface defined
- ✅ MongoDB provider - Core implementation complete
- ✅ LiteDB provider - Core implementation complete
- 🚧 Entity Framework provider - In progress
- 🚧 SQLite provider - In progress
- 🚧 SQL Server provider - In progress

## Next Steps

1. Complete the Entity Framework repository implementation
2. Finalize SQLite and SQL Server providers
3. Implement advanced querying capabilities
4. Prepare for service layer integration
