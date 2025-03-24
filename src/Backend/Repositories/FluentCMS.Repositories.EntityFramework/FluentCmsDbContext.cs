using FluentCMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentCMS.Repositories.EntityFramework;

public class FluentCmsDbContext : DbContext
{
    private readonly EntityFrameworkOptions _options;

    public FluentCmsDbContext(DbContextOptions options, IOptions<EntityFrameworkOptions> efOptions) : base(options)
    {
        _options = efOptions?.Value ?? throw new ArgumentNullException(nameof(efOptions));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        ConfigureEntityTypes(modelBuilder);

        // Apply naming conventions
        ApplyNamingConventions(modelBuilder);
    }

    protected virtual void ConfigureEntityTypes(ModelBuilder modelBuilder)
    {
        // Get all entity types from model
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(IBaseEntity).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            // Configure primary key
            modelBuilder.Entity(entityType.ClrType)
                .HasKey(nameof(IBaseEntity.Id));

            // Configure auditable properties if entity implements IAuditableEntity
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.CreatedDate))
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.CreatedBy))
                    .HasMaxLength(256);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.LastModifiedDate));

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.LastModifiedBy))
                    .HasMaxLength(256);
            }
        }
    }

    protected virtual void ApplyNamingConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Get initial table name from entity name
            var tableName = entityType.GetTableName();
            if (tableName == null) continue;

            // Apply transformations based on options
            tableName = GetTableName(entityType.ClrType.Name);

            // Set the table name
            entityType.SetTableName(tableName);

            // Set schema if specified
            if (!string.IsNullOrEmpty(_options.DefaultSchema))
            {
                entityType.SetSchema(_options.DefaultSchema);
            }
        }
    }

    protected virtual string GetTableName(string entityName)
    {
        string tableName = entityName;

        // Handle pluralization if enabled
        if (_options.UsePluralTableNames)
        {
            tableName = Pluralize(tableName);
        }

        // Apply camelCase if configured
        if (_options.UseCamelCaseTableNames)
        {
            tableName = char.ToLowerInvariant(tableName[0]) + tableName[1..];
        }

        // Apply prefix and suffix
        if (!string.IsNullOrEmpty(_options.TableNamePrefix))
        {
            tableName = _options.TableNamePrefix + tableName;
        }

        if (!string.IsNullOrEmpty(_options.TableNameSuffix))
        {
            tableName = tableName + _options.TableNameSuffix;
        }

        return tableName;
    }

    private static string Pluralize(string word)
    {
        // Handle simple plurals
        if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") ||
            word.EndsWith("ch") || word.EndsWith("sh"))
        {
            return word + "es";
        }
        else if (word.EndsWith("y") && !IsVowel(word[^2]))
        {
            return word[..^1] + "ies";
        }
        else
        {
            return word + "s";
        }
    }

    private static bool IsVowel(char c)
    {
        return "aeiou".Contains(char.ToLowerInvariant(c));
    }
}
