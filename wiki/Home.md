# FluentCMS Wiki

Welcome to the FluentCMS wiki! This documentation provides comprehensive information about the FluentCMS headless content management system.

## Overview

FluentCMS is a flexible, headless Content Management System backend built with ASP.NET Core, supporting multiple database providers through a unified repository pattern. It provides a clean API for content management that can be consumed by various frontend applications.

## Documentation Index

### Core Concepts
- [Architecture Overview](./Architecture-Overview.md) - System architecture and design principles
- [Repository Pattern](./Repository-Pattern.md) - Implementation of the repository pattern
- [Entity Model](./Entity-Model.md) - Core entity structure and design

### Database Providers
- [Database Providers Overview](./Database-Providers.md) - Introduction to all database providers
- [MongoDB Provider](./MongoDB-Provider.md) - MongoDB implementation details
- [LiteDB Provider](./LiteDB-Provider.md) - LiteDB implementation details
- [EntityFramework Provider](./EntityFramework-Provider.md) - Entity Framework Core implementation
- [SQLite Provider](./SQLite-Provider.md) - SQLite-specific implementation
- [SQL Server Provider](./SQL-Server-Provider.md) - SQL Server implementation

### Development Resources
- [Configuration Guide](./Configuration-Guide.md) - Configuration options for all providers
- [Development Guide](./Development-Guide.md) - Development setup and workflows
- [API Reference](./API-Reference.md) - API documentation

### Project Information
- [Memory Bank System](./Memory-Bank-System.md) - Explanation of the memory bank system
- [Roadmap](./Roadmap.md) - Future development plans

## Quick Start

To get started with FluentCMS:

1. Clone the repository
2. Restore dependencies with `dotnet restore`
3. Configure your preferred database provider
4. Build the solution with `dotnet build`

See the [Development Guide](./Development-Guide.md) for detailed setup instructions.

## Key Features

- 🏗️ Generic repository pattern implementation
- 📦 Multiple database provider support
- 🔄 Consistent API regardless of underlying database
- 📝 Audit tracking for entity changes
- ⚡ Asynchronous operations throughout

## Database Provider Support

| Provider | Status | Use Case |
|----------|--------|----------|
| MongoDB | ✅ Complete | Document storage, scalability |
| LiteDB | ✅ Complete | Embedded document database, simplicity |
| Entity Framework Core | 🚧 In Progress | ORM for relational databases |
| SQLite | 🚧 In Progress | File-based relational database |
| SQL Server | 🚧 In Progress | Enterprise relational database |

## Navigation

For a high-level project overview, see the main [README.md](../README.md).

For development context and current status, see the [memory bank](../memory-bank/).
