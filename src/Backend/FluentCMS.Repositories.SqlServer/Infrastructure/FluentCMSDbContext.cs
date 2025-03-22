using FluentCMS.Entities;
using FluentCMS.Repositories.SqlServer.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCMS.Repositories.SqlServer.Infrastructure;

/// <summary>
/// Entity Framework Core DbContext for FluentCMS entities using SQL Server.
/// </summary>
public class FluentCMSDbContext : DbContext
{
    private readonly SqlServerSettings _settings;
    private readonly IEnumerable<Type> _entityTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentCMSDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="settings">The SQL Server settings.</param>
    /// <param name="entityTypes">The entity types to include in the context.</param>
    public FluentCMSDbContext(
        DbContextOptions<FluentCMSDbContext> options, 
        SqlServerSettings settings,
        IEnumerable<Type> entityTypes = null)
        : base(options)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _entityTypes = entityTypes ?? GetEntityTypesFromAssembly(typeof(IBaseEntity).Assembly);
    }

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register all entities that implement IBaseEntity
        foreach (var entityType in _entityTypes)
        {
            if (typeof(IBaseEntity).IsAssignableFrom(entityType) && entityType.IsClass && !entityType.IsAbstract)
            {
                modelBuilder.Entity(entityType);
            }
        }

        // Set default schema if specified
        if (!string.IsNullOrEmpty(_settings.SchemaName))
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetSchema(_settings.SchemaName);
            }
        }

        // Apply table prefix if specified
        if (!string.IsNullOrEmpty(_settings.TablePrefix))
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName($"{_settings.TablePrefix}{entityType.GetTableName()}");
            }
        }

        // Apply global soft delete filter if enabled
        if (_settings.ApplySoftDeleteFilter)
        {
            ApplySoftDeleteFilter(modelBuilder);
        }

        // Apply custom entity configurations
        ApplyEntityConfigurations(modelBuilder);
        
        // Apply SQL Server specific optimizations
        if (_settings.EnableSqlServerFeatures)
        {
            ApplySqlServerOptimizations(modelBuilder);
        }
    }

    /// <summary>
    /// Configures the options to be used by this context.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_settings.ConnectionString, sqlServerOptions => 
            {
                sqlServerOptions.EnableRetryOnFailure(
                    _settings.MaxRetryCount,
                    TimeSpan.FromSeconds(_settings.MaxRetryDelay),
                    null);
                    
                if (_settings.CommandTimeout > 0)
                {
                    sqlServerOptions.CommandTimeout(_settings.CommandTimeout);
                }
                
                // Configure migrations assembly
                sqlServerOptions.MigrationsAssembly(typeof(FluentCMSDbContext).Assembly.GetName().Name);
            });
        }

        if (!_settings.AutoDetectChanges)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        if (_settings.EnableDetailedErrors)
        {
            optionsBuilder.EnableDetailedErrors();
        }

        if (_settings.EnableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    /// <summary>
    /// Creates a DbSet for the given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A set for the given entity type.</returns>
    public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class, IBaseEntity
    {
        return Set<TEntity>();
    }

    private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        // Apply query filter to soft-deletable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeleteBaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
                var isFalseExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(isFalseExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        foreach (var entityType in _entityTypes)
        {
            if (typeof(IBaseEntity).IsAssignableFrom(entityType) && entityType.IsClass && !entityType.IsAbstract)
            {
                // Configure entity Id as the primary key
                modelBuilder.Entity(entityType).HasKey("Id");

                // Configure audit fields
                modelBuilder.Entity(entityType).Property("CreatedDate").IsRequired();
                modelBuilder.Entity(entityType).Property("CreatedBy").HasMaxLength(100);
                modelBuilder.Entity(entityType).Property("LastModifiedDate");
                modelBuilder.Entity(entityType).Property("LastModifiedBy").HasMaxLength(100);

                // Configure soft delete fields for ISoftDeleteBaseEntity
                if (typeof(ISoftDeleteBaseEntity).IsAssignableFrom(entityType))
                {
                    modelBuilder.Entity(entityType).Property("IsDeleted").IsRequired().HasDefaultValue(false);
                    modelBuilder.Entity(entityType).Property("DeletedDate");
                    modelBuilder.Entity(entityType).Property("DeletedBy").HasMaxLength(100);
                }
            }
        }
    }
    
    private void ApplySqlServerOptimizations(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Create clustered index on Id
            modelBuilder.Entity(entityType.ClrType).HasIndex("Id").IsClustered();
            
            // Create non-clustered index on CreatedDate for better performance on time-based queries
            modelBuilder.Entity(entityType.ClrType).HasIndex("CreatedDate").IsUnique(false).IsClustered(false);
            
            // For soft-deletable entities, create a filtered index on non-deleted entities
            if (typeof(ISoftDeleteBaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("IsDeleted")
                    .HasFilter("[IsDeleted] = 0") // Only index non-deleted entities
                    .IsUnique(false)
                    .IsClustered(false);
            }
            
            // Apply table partitioning if configured
            if (_settings.UseTablePartitioning)
            {
                // SQL Server partitioning would be configured here
                // This is more complex and would require a database-specific implementation
                // Basic example to partition by year using CreatedDate
                // Note: This is just a simplified example and not a complete implementation
                if (entityType.GetProperties().Any(p => p.Name == "CreatedDate"))
                {
                    // Configure partition scheme in SQL scripts
                }
            }
        }
    }

    private IEnumerable<Type> GetEntityTypesFromAssembly(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => typeof(IBaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
    }
}
