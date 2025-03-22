# FluentCMS Coding Standards

This document outlines the coding standards and best practices for the FluentCMS project.

## 1. Naming Conventions

### 1.1 General Guidelines
- Use **PascalCase** for class names, interfaces, public members, and constants.
- Use **camelCase** for local variables, parameters, and private fields.
- Prefix interfaces with `I` (e.g., `IBaseEntity`).
- Prefix private fields with `_` (e.g., `private readonly DbContext _context;`).

### 1.2 File Naming
- Files should be named according to the main class/interface they contain.
- Extension method classes should be suffixed with `Extensions` (e.g., `ServiceCollectionExtensions`).
- Repository implementations should be named according to their database provider (e.g., `MongoDbEntityRepository`).

## 2. Code Organization

### 2.1 Namespaces
- Use a consistent namespace structure that mirrors the folder structure.
- Main namespaces should follow the pattern: `FluentCMS.{Area}.{Subarea}`.

### 2.2 Class Structure
- Organize members in the following order:
  1. Private fields
  2. Constructors
  3. Properties
  4. Public methods
  5. Protected methods
  6. Private methods

### 2.3 Project Organization
- Keep related functionality in the same project.
- Maintain clear boundaries between layers (entities, repositories, services, etc.).
- Infrastructure concerns should be isolated in dedicated classes/folders.

## 3. Documentation

### 3.1 XML Documentation
- All public APIs must have XML documentation.
- Include `<summary>` tags for all public types and members.
- Document parameters using `<param>` and return values using `<returns>`.
- Use `<exception>` to document exceptions that may be thrown.

### 3.2 Comments
- Add comments for complex algorithms or business rules.
- Avoid redundant comments that merely restate what the code does.
- Use `TODO:`, `HACK:`, or `FIXME:` prefixes for temporary comments.

## 4. Error Handling

### 4.1 Exceptions
- Create custom exception types for specific error scenarios.
- Catch exceptions only when you can handle them appropriately.
- Log exceptions with sufficient context for troubleshooting.
- Use exception filters where appropriate.

### 4.2 Null Checking
- Use null-conditional operators (`?.`) and null-coalescing operators (`??`) where appropriate.
- Consider using C# 8.0+ nullable reference types for new code.
- Validate method parameters at the beginning of methods.

## 5. Asynchronous Programming

### 5.1 Task Usage
- Use `async`/`await` consistently rather than mixing with `Task.Result` or `.Wait()`.
- Do NOT suffix asynchronous methods with `Async` (e.g., use `GetById` instead of `GetByIdAsync`).
- Return `Task<T>` for methods that return a value, and `Task` for methods that don't.
- Use `ConfigureAwait(false)` where appropriate to prevent deadlocks.

## 6. LINQ Usage

### 6.1 Query Syntax vs. Method Syntax
- Prefer method syntax for simple queries.
- Consider query syntax for complex queries with multiple joins or groupings.
- Use meaningful variable names in LINQ expressions.

### 6.2 Deferred Execution
- Be aware of deferred execution in LINQ queries.
- Use `.ToList()`, `.ToArray()`, or `.ToDictionary()` to materialize queries when needed.

## 7. Dependency Injection

### 7.1 Service Registration
- Register dependencies in the appropriate scope (singleton, scoped, or transient).
- Use extension methods for registering related services (e.g., `AddMongoDb`).
- Consider using the options pattern for configuration.

### 7.2 Constructor Injection
- Prefer constructor injection over property or method injection.
- Consider using the `[FromServices]` attribute for action method injection in controllers.

## 8. Testing

### 8.1 Unit Tests
- Write unit tests for all business logic.
- Use a naming convention like `[MethodName]_[Scenario]_[ExpectedResult]`.
- Mock external dependencies when testing business logic.
- Aim for high code coverage, especially in core business logic.

### 8.2 Integration Tests
- Write integration tests for repository implementations.
- Use in-memory databases or containers for testing when possible.

## 9. Repository Pattern Implementation

### 9.1 Generic Repositories
- Follow the repository pattern consistently across different database providers.
- Implement common interfaces like `IBaseEntityRepository<T>` for all repositories.
- Use generic type constraints to enforce entity requirements.

### 9.2 Query Optimization
- Be mindful of N+1 query issues.
- Use projection (`.Select()`) to limit the data retrieved.
- Consider implementing specialized query methods for complex scenarios.
