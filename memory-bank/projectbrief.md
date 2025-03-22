# FluentCMS Project Brief

## Overview

FluentCMS is a headless Content Management System (CMS) backend built with ASP.NET Core. It provides a flexible and modular API for content management that can be consumed by various frontend applications. The primary frontend will be built using Blazor Server technology.

## Project Goals

1. Create a flexible headless CMS backend with a clean, modular architecture
2. Support multiple database providers through a unified repository pattern
3. Provide a comprehensive API for content management operations
4. Enable easy integration with various frontend technologies
5. Specifically support a Blazor interactive server frontend (to be developed separately)

## Core Features

1. **Modular Data Access**
   - Abstract repository pattern implementation
   - Support for multiple database providers (MongoDB, LiteDB, Entity Framework, SQLite, SQL Server)
   - Consistent API regardless of the underlying database technology

2. **Entity Framework**
   - Base entities with standard properties (ID, audit fields)
   - Support for entity relationships
   - Data validation

3. **API Capabilities** (To be implemented)
   - Content type definition
   - Content creation, reading, updating, deletion
   - Content querying and filtering
   - User authentication and authorization
   - Media management

## Technical Requirements

1. Built with ASP.NET Core
2. Follows clean architecture principles
3. Implements repository pattern for data access
4. Supports multiple database providers
5. Provides extension points for customization
6. Follows RESTful API design principles
7. Includes comprehensive documentation

## Target Audience

1. Developers building custom CMS solutions
2. Projects requiring a headless CMS with .NET backend
3. Applications needing flexible content management with multiple database options

## Success Criteria

1. Complete and functional repository implementations for all database providers
2. Well-documented API with examples
3. Easy configuration and setup process
4. Extensible architecture for custom requirements
5. Successful integration with Blazor frontend (future)

## Current Status

The project is in the early development phase, focusing on building the backend components first:
- Core entity models are defined
- Repository pattern abstractions are in place
- Initial implementations for MongoDB and LiteDB providers are in progress
- Other database providers (EntityFramework, SQLite, SqlServer) have placeholder projects set up
