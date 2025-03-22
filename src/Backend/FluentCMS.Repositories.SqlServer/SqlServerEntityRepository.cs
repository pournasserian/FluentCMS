using FluentCMS.Entities;
using FluentCMS.Repositories.Abstractions;
using FluentCMS.Repositories.Core;
using FluentCMS.Repositories.SqlServer.Configuration;
using FluentCMS.Repositories.SqlServer.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace FluentCMS.Repositories.SqlServer;

/// <summary>
/// SQL Server implementation of the entity repository using Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IBaseEntity</typeparam>
public class SqlServerEntityRepository<TEntity> : BaseEntityRepositoryBase<TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly FluentCMSDbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;
    private readonly SqlServerSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerEntityRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="settings">The SQL Server settings.</param>
    public SqlServerEntityRepository(
        FluentCMSDbContext dbContext,
        SqlServerSettings settings)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dbSet = _dbContext.GetDbSet<TEntity>();
    }

    #region Basic CRUD Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> CreateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> CreateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any())
            return entitiesList;

        // Use bulk insert for SQL Server if there are many entities
        if (_settings.EnableSqlServerFeatures && entitiesList.Count > 100)
        {
            await BulkInsertAsync(entitiesList, cancellationToken);
            return entitiesList;
        }
        else
        {
            await _dbSet.AddRangeAsync(entitiesList, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entitiesList;
        }
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> UpdateInternalAsync(TEntity entity, CancellationToken cancellationToken)
    {
        // Make sure the entity is tracked
        var trackedEntity = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
        if (trackedEntity == null)
            return null;

        // Update the entity properties
        _dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return trackedEntity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> UpdateManyInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any())
            return entitiesList;

        // For SQL Server, use bulk update if there are many entities
        if (_settings.EnableSqlServerFeatures && entitiesList.Count > 100)
        {
            await BulkUpdateAsync(entitiesList, cancellationToken);
            return entitiesList;
        }
        else
        {
            var updatedEntities = new List<TEntity>();
            
            foreach (var entity in entitiesList)
            {
                var trackedEntity = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
                if (trackedEntity != null)
                {
                    _dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);
                    updatedEntities.Add(trackedEntity);
                }
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return updatedEntities;
        }
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> DeleteInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null)
            return null;

        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> DeleteManyInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();

        var entities = await _dbSet.Where(e => idsList.Contains(e.Id)).ToListAsync(cancellationToken);
        if (!entities.Any())
            return Enumerable.Empty<TEntity>();

        // Use direct SQL for large delete operations
        if (_settings.EnableSqlServerFeatures && idsList.Count > 100)
        {
            await BulkDeleteAsync(idsList, cancellationToken);
            return entities;
        }
        else
        {
            _dbSet.RemoveRange(entities);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entities;
        }
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken)
    {
        // Use query hints for better performance if enabled
        if (_settings.UseQueryHints)
        {
            return await _dbSet
                .TagWith("GetAll")
                .FromSqlRaw($"SELECT * FROM {GetTableName()} WITH (NOLOCK)")
                .ToListAsync(cancellationToken);
        }
        else
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetByIdsInternalAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();

        // For SQL Server, optimization using table-valued parameters for large lists
        if (_settings.EnableSqlServerFeatures && idsList.Count > 100)
        {
            return await GetByIdsWithTableValuedParameterAsync(idsList, cancellationToken);
        }
        else
        {
            return await _dbSet.Where(e => idsList.Contains(e.Id)).ToListAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> FindOneInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<bool> ExistsInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    #endregion

    #region Pagination

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedInternalAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        
        // Use optimized SQL Server paging with OFFSET-FETCH
        if (_settings.UseQueryHints)
        {
            var items = await _dbSet
                .TagWith("GetPaged")
                .FromSqlRaw($@"
                    SELECT * FROM {GetTableName()} WITH (NOLOCK)
                    ORDER BY Id
                    OFFSET {(pageNumber - 1) * pageSize} ROWS
                    FETCH NEXT {pageSize} ROWS ONLY")
                .ToListAsync(cancellationToken);
                
            return (items, totalCount);
        }
        else
        {
            var items = await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
                
            return (items, totalCount);
        }
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedInternalAsync(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    #endregion

    #region Sorting

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllSortedInternalAsync<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = ascending
            ? _dbSet.OrderBy(keySelector)
            : _dbSet.OrderByDescending(keySelector);
            
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSortedInternalAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var query = ascending
            ? _dbSet.OrderBy(keySelector)
            : _dbSet.OrderByDescending(keySelector);
            
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> FindSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = ascending
            ? _dbSet.Where(predicate).OrderBy(keySelector)
            : _dbSet.Where(predicate).OrderByDescending(keySelector);
            
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSortedInternalAsync<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        var sortedQuery = ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
            
        var items = await sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
    }

    #endregion

    #region Soft Delete Operations

    /// <inheritdoc />
    protected override async Task<TEntity?> SoftDeleteInternalAsync(Guid id, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        // Get all entities (including soft-deleted ones)
        var allDbSet = _dbContext.Set<TEntity>().IgnoreQueryFilters();
        
        // Find the entity
        var entity = await allDbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity == null)
            return null;
            
        // Cast to ISoftDeleteBaseEntity and set soft delete properties
        var softDeleteEntity = entity as ISoftDeleteBaseEntity;
        if (softDeleteEntity != null)
        {
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedDate = DateTime.UtcNow;
            softDeleteEntity.DeletedBy = deletedBy;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        
        return null;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> SoftDeleteManyInternalAsync(IEnumerable<Guid> ids, string? deletedBy, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return Enumerable.Empty<TEntity>();
            
        var idsList = ids.ToList();
        if (!idsList.Any())
            return Enumerable.Empty<TEntity>();
            
        // Get all entities (including soft-deleted ones)
        var allDbSet = _dbContext.Set<TEntity>().IgnoreQueryFilters();
        
        // Find the entities
        var entities = await allDbSet.Where(e => idsList.Contains(e.Id)).ToListAsync(cancellationToken);
        if (!entities.Any())
            return Enumerable.Empty<TEntity>();
            
        // Use direct SQL for large soft-delete operations
        if (_settings.EnableSqlServerFeatures && idsList.Count > 100)
        {
            await BulkSoftDeleteAsync(idsList, deletedBy, cancellationToken);
            return entities;
        }
        else
        {
            // Update each entity
            foreach (var entity in entities)
            {
                var softDeleteEntity = entity as ISoftDeleteBaseEntity;
                if (softDeleteEntity != null)
                {
                    softDeleteEntity.IsDeleted = true;
                    softDeleteEntity.DeletedDate = DateTime.UtcNow;
                    softDeleteEntity.DeletedBy = deletedBy;
                }
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entities;
        }
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> RestoreInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        // Check if entity supports soft delete
        if (!typeof(ISoftDeleteBaseEntity).IsAssignableFrom(typeof(TEntity)))
            return null;
            
        // Get all entities (including soft-deleted ones)
        var allDbSet = _dbContext.Set<TEntity>().IgnoreQueryFilters();
        
        // Find the entity
        var entity = await allDbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity == null)
            return null;
            
        // Cast to ISoftDeleteBaseEntity and reset soft delete properties
        var softDeleteEntity = entity as ISoftDeleteBaseEntity;
        if (softDeleteEntity != null)
        {
            softDeleteEntity.IsDeleted = false;
            softDeleteEntity.DeletedDate = null;
            softDeleteEntity.DeletedBy = null;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
        
        return null;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>> GetAllIncludeDeletedInternalAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<TEntity>().IgnoreQueryFilters().ToListAsync(cancellationToken);
    }

    #endregion

    #region Aggregation Operations

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<int> CountInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    #endregion

    #region Bulk Update Operations

    /// <inheritdoc />
    protected override async Task<int> UpdateManyWithFieldsInternalAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken)
    {
        if (!fieldValues.Any())
            return 0;
            
        // SQL Server supports direct UPDATE statements
        if (_settings.EnableSqlServerFeatures)
        {
            return await BulkUpdateWithFieldsAsync(predicate, fieldValues, cancellationToken);
        }
        else
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            if (!entities.Any())
                return 0;
                
            var updatedCount = 0;
            
            foreach (var entity in entities)
            {
                var updated = false;
                var entityEntry = _dbContext.Entry(entity);
                
                foreach (var kvp in fieldValues)
                {
                    var property = entityEntry.Property(kvp.Key);
                    if (property != null)
                    {
                        property.CurrentValue = kvp.Value;
                        updated = true;
                    }
                }
                
                if (updated)
                {
                    updatedCount++;
                }
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return updatedCount;
        }
    }

    #endregion

    #region Projection Support

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectInternalAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await _dbSet.Select(selector).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TResult>> SelectWhereInternalAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).Select(selector).ToListAsync(cancellationToken);
    }

    #endregion

    #region Async Enumeration

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> GetAllAsStreamInternalAsync(CancellationToken cancellationToken)
    {
        return _dbSet.AsAsyncEnumerable();
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<TEntity> FindAsStreamInternalAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return _dbSet.Where(predicate).AsAsyncEnumerable();
    }

    #endregion

    #region Transaction Support

    /// <inheritdoc />
    protected override async Task<bool> ExecuteInTransactionInternalAsync(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken)
    {
        // Using EF Core's transaction API
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var result = await operation(this);
            
            if (result)
            {
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion

    #region SQL Server Specific Optimizations

    /// <summary>
    /// Gets the table name for the entity type.
    /// </summary>
    /// <returns>The table name.</returns>
    private string GetTableName()
    {
        var entityType = typeof(TEntity);
        var schema = string.IsNullOrEmpty(_settings.SchemaName) ? "dbo" : _settings.SchemaName;
        var table = entityType.Name;
        
        // Remove "Entity" suffix if present
        if (table.EndsWith("Entity"))
        {
            table = table.Substring(0, table.Length - 6);
        }
        
        // Add prefix if configured
        if (!string.IsNullOrEmpty(_settings.TablePrefix))
        {
            table = $"{_settings.TablePrefix}{table}";
        }
        
        return $"[{schema}].[{table}]";
    }

    /// <summary>
    /// Performs a bulk insert operation using SqlBulkCopy.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        // This is a simplified example. In a real application, you'd need to create a DataTable
        // and map entity properties to DataTable columns.
        
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
            bulkCopy.DestinationTableName = GetTableName();
            bulkCopy.BulkCopyTimeout = _settings.CommandTimeout;
            
            // Create DataTable from entities
            var dataTable = CreateDataTable(entities);
            
            // Map columns
            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
            
            // Perform bulk insert
            await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Performs a bulk update operation using SQL statements.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        // In a real application, you'd use a more efficient approach like table-valued parameters.
        // For simplicity, we'll use multiple UPDATE statements in a transaction.
        
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandTimeout = _settings.CommandTimeout;
            
            // Build SQL command for each entity
            foreach (var entity in entities)
            {
                var sql = new StringBuilder();
                sql.Append($"UPDATE {GetTableName()} SET ");
                
                // Add properties to update
                var properties = typeof(TEntity).GetProperties()
                    .Where(p => p.Name != "Id" && p.CanWrite)
                    .ToList();
                
                for (var i = 0; i < properties.Count; i++)
                {
                    var prop = properties[i];
                    sql.Append($"[{prop.Name}] = @{prop.Name}");
                    
                    if (i < properties.Count - 1)
                    {
                        sql.Append(", ");
                    }
                }
                
                sql.Append(" WHERE [Id] = @Id");
                
                command.CommandText = sql.ToString();
                command.Parameters.Clear();
                
                // Add parameters
                command.Parameters.Add(new SqlParameter("@Id", entity.Id));
                
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(entity) ?? DBNull.Value;
                    command.Parameters.Add(new SqlParameter($"@{prop.Name}", value));
                }
                
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Performs a bulk delete operation using SQL statements.
    /// </summary>
    /// <param name="ids">The IDs of entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        // For large delete operations, we'll use a table-valued parameter
        
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandTimeout = _settings.CommandTimeout;
            
            // Use IN clause with batches of IDs to avoid parameter limits
            var idBatches = SplitIntoBatches(ids.ToList(), 1000);
            
            foreach (var batch in idBatches)
            {
                var parameters = new List<SqlParameter>();
                var placeholders = new List<string>();
                
                for (var i = 0; i < batch.Count; i++)
                {
                    var paramName = $"@Id{i}";
                    placeholders.Add(paramName);
                    parameters.Add(new SqlParameter(paramName, batch[i]));
                }
                
                var sql = $"DELETE FROM {GetTableName()} WHERE [Id] IN ({string.Join(",", placeholders)})";
                
                command.CommandText = sql;
                command.Parameters.Clear();
                
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Performs a bulk soft delete operation using SQL statements.
    /// </summary>
    /// <param name="ids">The IDs of entities to soft delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the operation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task BulkSoftDeleteAsync(IEnumerable<Guid> ids, string? deletedBy, CancellationToken cancellationToken)
    {
        // For large soft delete operations, we'll use a table-valued parameter
        
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandTimeout = _settings.CommandTimeout;
            
            // Use IN clause with batches of IDs to avoid parameter limits
            var idBatches = SplitIntoBatches(ids.ToList(), 1000);
            
            foreach (var batch in idBatches)
            {
                var parameters = new List<SqlParameter>();
                var placeholders = new List<string>();
                
                for (var i = 0; i < batch.Count; i++)
                {
                    var paramName = $"@Id{i}";
                    placeholders.Add(paramName);
                    parameters.Add(new SqlParameter(paramName, batch[i]));
                }
                
                var sql = $@"
                    UPDATE {GetTableName()} SET 
                    [IsDeleted] = 1, 
                    [DeletedDate] = @DeletedDate, 
                    [DeletedBy] = @DeletedBy
                    WHERE [Id] IN ({string.Join(",", placeholders)})";
                
                command.CommandText = sql;
                command.Parameters.Clear();
                
                command.Parameters.Add(new SqlParameter("@DeletedDate", DateTime.UtcNow));
                command.Parameters.Add(new SqlParameter("@DeletedBy", deletedBy ?? (object)DBNull.Value));
                
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Performs a bulk update operation with specific fields using SQL statements.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="fieldValues">A dictionary of field names and their new values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities updated.</returns>
    private async Task<int> B
