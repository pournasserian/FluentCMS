# FluentCMS Test Structure

This document outlines the hybrid test structure planned for FluentCMS, which provides a balance between separation of test code and clear mapping to implementation components.

## Hybrid Test Structure

```
FluentCMS/
├── src/                           # Production code
│   ├── FluentCMS.sln
│   ├── Backend/
│   │   ├── FluentCMS.Entities/
│   │   ├── Repositories/
│   │   │   ├── FluentCMS.Repositories.Abstractions/
│   │   │   ├── FluentCMS.Repositories.MongoDB/
│   │   │   └── ...
├── tests/                         # Top-level tests directory
│   ├── FluentCMS.Tests.sln        # Separate solution for all tests
│   ├── Unit/                      # Unit tests by component
│   │   ├── Backend/
│   │   │   ├── FluentCMS.Entities.Tests/
│   │   │   ├── Repositories/
│   │   │   │   ├── FluentCMS.Repositories.Abstractions.Tests/
│   │   │   │   ├── FluentCMS.Repositories.MongoDB.Tests/
│   │   │   │   └── ...
│   ├── Integration/               # Integration tests
│   │   ├── Backend/
│   │   │   ├── Repositories/
│   │   │   │   ├── FluentCMS.Repositories.MongoDB.Integration.Tests/
│   │   │   │   └── ...
│   ├── Performance/               # Performance tests
│   ├── E2E/                       # End-to-end tests
│   └── TestUtilities/             # Shared test utilities/helpers
```

## Key Benefits

1. **Clear Separation**: Test code is completely separate from production code
2. **Parallel Structure**: Test projects mirror the structure of production code for easy navigation
3. **Test Type Organization**: Tests are organized by type (unit, integration, performance, etc.)
4. **Shared Test Resources**: Common test utilities can be shared across different test projects
5. **Independent Test Solution**: Ability to load just the test solution when focusing on testing
6. **Specialized CI Configuration**: Easier to configure CI/CD to run specific test types

## Implementation Guide

### 1. Create Directory Structure

```bash
mkdir -p tests/Unit/Backend/Repositories tests/Integration/Backend/Repositories tests/Performance tests/E2E tests/TestUtilities
```

### 2. Create Test Solution

```bash
cd tests
dotnet new sln -n FluentCMS.Tests
```

### 3. Moving Existing Test Projects

For each existing test project:

1. Create the target directory:
```bash
mkdir -p tests/Unit/Backend/Path/To/Project.Tests
```

2. Copy the project file with updated references:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- ... other properties ... -->
  <ItemGroup>
    <ProjectReference Include="../../../../../src/Path/To/Project/Project.csproj" />
  </ItemGroup>
</Project>
```

3. Copy test files and resources

4. Add to the test solution:
```bash
cd tests
dotnet sln add Unit/Backend/Path/To/Project.Tests/Project.Tests.csproj --solution-folder "Unit Tests"
```

### 4. Creating New Test Projects

For new test projects, follow this pattern:

1. Unit tests:
```bash
cd tests/Unit/Backend/Path/To
dotnet new xunit -n Project.Tests
cd Project.Tests
dotnet add reference ../../../../../src/Path/To/Project/Project.csproj
```

2. Integration tests:
```bash
cd tests/Integration/Backend/Path/To
dotnet new xunit -n Project.Integration.Tests
cd Project.Integration.Tests
dotnet add reference ../../../../../src/Path/To/Project/Project.csproj
```

3. Add to the solution:
```bash
cd tests
dotnet sln add Unit/Backend/Path/To/Project.Tests/Project.Tests.csproj --solution-folder "Unit Tests"
```

## Naming Conventions

- Unit test projects: `{ProjectName}.Tests`
- Integration test projects: `{ProjectName}.Integration.Tests`
- Performance test projects: `{ProjectName}.Performance.Tests`
- E2E test projects: `{ProjectName}.E2E.Tests`
- Test utility projects: `FluentCMS.TestUtilities.{Category}`

## Test Project Organization

Mirror the namespace structure of the implementation:

```csharp
namespace FluentCMS.Repositories.Factory.Tests
{
    public class RepositoryFactoryTests
    {
        // Tests for RepositoryFactory class
    }
    
    namespace Providers
    {
        public class MongoDbProviderConfiguratorTests
        {
            // Tests for MongoDbProviderConfigurator class
        }
    }
}
```

## CI/CD Integration

In CI/CD pipelines, you can selectively run tests by type:

```yaml
# Example GitHub Actions workflow
jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run unit tests
        run: dotnet test tests/Unit --filter Category=UnitTest

  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run integration tests
        run: dotnet test tests/Integration --filter Category=IntegrationTest
```

## Recommendations for Future Test Development

1. **Test Data Management**:
   - Place test data in a `TestData` folder within each test project
   - For shared test data, use the `tests/TestUtilities/TestData` folder

2. **Mock Management**:
   - Create reusable mocks in the `TestUtilities` projects
   - Use a consistent mocking pattern across all test projects

3. **Test Categories**:
   - Apply categories/traits to tests for selective execution:
   ```csharp
   [Fact]
   [Trait("Category", "UnitTest")]
   public void SomeTest() { ... }
   ```

4. **Shared Test Fixtures**:
   - Use shared test fixtures for common setup and teardown code
   - Implement in `TestUtilities` for reuse across test projects
