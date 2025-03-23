using FluentCMS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FluentCMS.Repositories.EntityFramework;

/// <summary>
/// Base DbContext for FluentCMS Entity Framework repositories.
/// </summary>
public class FluentCmsDbContext : DbContext
{
    private readonly EntityFrameworkOptions _options;

    /// <summary>
    /// Initializes a new instance of the FluentCmsDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="efOptions">The Entity Framework configuration options.</param>
    public FluentCmsDbContext(
        DbContextOptions options,
        IOptions<EntityFrameworkOptions> efOptions)
        : base(options)
    {
        _options = efOptions?.Value ?? throw new ArgumentNullException(nameof(efOptions));
    }

    /// <summary>
    /// Configures the model for all entities in the context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        ConfigureEntityTypes(modelBuilder);
        
        // Apply naming conventions
        ApplyNamingConventions(modelBuilder);
    }

    /// <summary>
    /// Configures entity types that implement IBaseEntity.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
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

    /// <summary>
    /// Applies naming conventions to entities based on configuration options.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
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

    /// <summary>
    /// Gets the table name for an entity based on configuration options.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <returns>The table name.</returns>
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

    /// <summary>
    /// Simple pluralization for table names.
    /// </summary>
    /// <param name="word">The word to pluralize.</param>
    /// <returns>The pluralized word.</returns>
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

    /// <summary>
    /// Checks if a character is a vowel.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is a vowel, otherwise false.</returns>
    private static bool IsVowel(char c)
    {
        return "aeiou".Contains(char.ToLowerInvariant(c));
    }
}
