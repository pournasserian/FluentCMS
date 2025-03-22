using FluentCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCMS.Repositories.Abstractions;

/// <summary>
/// An enhanced version of IBaseEntityRepository with additional features for more flexible data access.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IBaseEntity</typeparam>
public interface IEnhancedBaseEntityRepository<TEntity> where TEntity : IBaseEntity
{
    #region Basic CRUD Operations

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created entity, or null if creation failed.</returns>
    Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple entities in the repository.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The collection of created entities.</returns>
    Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated entity, or null if the update failed.</returns>
    Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities in the repository.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The collection of updated entities.</returns>
    Task<IEnumerable<TEntity>> UpdateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deleted entity, or null if the deletion failed.</returns>
    Task<TEntity?> Delete(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of the entities to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The collection of deleted entities.</returns>
    Task<IEnumerable<TEntity>> DeleteMany(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    #endregion

    #region Retrieval Operations

    /// <summary>
    /// Gets all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity with the specified ID, or null if not found.</returns>
    Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple entities by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of the entities to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of entities matching the provided IDs.</returns>
    Task<IEnumerable<TEntity>> GetByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    #endregion

    #region Query Operations

    /// <summary>
    /// Finds entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of entities matching the predicate.</returns>
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single entity matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first entity matching the predicate, or null if none is found.</returns>
    Task<TEntity?> FindOne(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if any entity matches the predicate; otherwise, false.</returns>
    Task<bool> Exists(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity with the specified ID exists.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if an entity with the specified ID exists; otherwise, false.</returns>
    Task<bool> ExistsById(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Pagination

    /// <summary>
    /// Gets a page of entities from the repository.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A tuple containing the page of entities and the total count of entities.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPaged(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A tuple containing the page of filtered entities and the total count of filtered entities.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPaged(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    #endregion

    #region Sorting

    /// <summary>
    /// Gets all entities sorted by the specified key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the sorting key.</typeparam>
    /// <param name="keySelector">An expression to extract a key from an entity.</param>
    /// <param name="ascending">True to sort in ascending order; false to sort in descending order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A sorted collection of entities.</returns>
    Task<IEnumerable<TEntity>> GetAllSorted<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of entities sorted by the specified key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the sorting key.</typeparam>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="keySelector">An expression to extract a key from an entity.</param>
    /// <param name="ascending">True to sort in ascending order; false to sort in descending order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A tuple containing the sorted page of entities and the total count of entities.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedSorted<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified predicate and sorted by the specified key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the sorting key.</typeparam>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="keySelector">An expression to extract a key from an entity.</param>
    /// <param name="ascending">True to sort in ascending order; false to sort in descending order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A sorted collection of filtered entities.</returns>
    Task<IEnumerable<TEntity>> FindSorted<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a page of entities matching the specified predicate and sorted by the specified key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the sorting key.</typeparam>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="keySelector">An expression to extract a key from an entity.</param>
    /// <param name="ascending">True to sort in ascending order; false to sort in descending order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A tuple containing the sorted page of filtered entities and the total count of filtered entities.</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> FindPagedSorted<TKey>(Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true, CancellationToken cancellationToken = default);

    #endregion

    #region Soft Delete Operations

    /// <summary>
    /// Soft-deletes an entity by its ID, if the entity type supports soft delete.
    /// </summary>
    /// <param name="id">The ID of the entity to soft-delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the soft-delete operation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The soft-deleted entity, or null if the operation failed or the entity type doesn't support soft-delete.</returns>
    Task<TEntity?> SoftDelete(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Soft-deletes multiple entities by their IDs, if the entity type supports soft delete.
    /// </summary>
    /// <param name="ids">The IDs of the entities to soft-delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the soft-delete operation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The collection of soft-deleted entities, or an empty collection if the operation failed or the entity type doesn't support soft-delete.</returns>
    Task<IEnumerable<TEntity>> SoftDeleteMany(IEnumerable<Guid> ids, string? deletedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a soft-deleted entity by its ID, if the entity type supports soft delete.
    /// </summary>
    /// <param name="id">The ID of the entity to restore.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The restored entity, or null if the operation failed or the entity type doesn't support soft-delete.</returns>
    Task<TEntity?> Restore(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities, including soft-deleted ones, if the entity type supports soft delete.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of all entities, including soft-deleted ones if applicable.</returns>
    Task<IEnumerable<TEntity>> GetAllIncludeDeleted(CancellationToken cancellationToken = default);

    #endregion

    #region Aggregation Operations

    /// <summary>
    /// Counts all entities in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The count of all entities.</returns>
    Task<int> Count(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The count of entities matching the predicate.</returns>
    Task<int> Count(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    #endregion

    #region Auditing Extensions

    /// <summary>
    /// Creates a new entity with explicit audit information.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="createdBy">The identifier of the user creating the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created entity, or null if creation failed.</returns>
    Task<TEntity?> CreateWithAudit(TEntity entity, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity with explicit audit information.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="modifiedBy">The identifier of the user modifying the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated entity, or null if the update failed.</returns>
    Task<TEntity?> UpdateWithAudit(TEntity entity, string modifiedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Transaction Support

    /// <summary>
    /// Executes a series of operations within a transaction.
    /// </summary>
    /// <param name="operation">A function that takes a repository and returns a boolean indicating success.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the transaction was committed; otherwise, false.</returns>
    Task<bool> ExecuteInTransaction(Func<IEnhancedBaseEntityRepository<TEntity>, Task<bool>> operation, CancellationToken cancellationToken = default);

    #endregion

    #region Bulk Update Operations

    /// <summary>
    /// Updates specific fields for entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="fieldValues">A dictionary of field names and their new values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities updated.</returns>
    Task<int> UpdateManyWithFields(Expression<Func<TEntity, bool>> predicate, Dictionary<string, object> fieldValues, CancellationToken cancellationToken = default);

    #endregion

    #region Projection Support

    /// <summary>
    /// Projects all entities using the specified selector.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="selector">A projection function to apply to each entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of projected results.</returns>
    Task<IEnumerable<TResult>> Select<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    /// <summary>
    /// Projects entities matching the specified predicate using the specified selector.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="selector">A projection function to apply to each entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of projected results from filtered entities.</returns>
    Task<IEnumerable<TResult>> SelectWhere<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    #endregion

    #region Async Enumeration

    /// <summary>
    /// Gets all entities as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous stream of all entities.</returns>
    IAsyncEnumerable<TEntity> GetAllAsStream(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified predicate as an asynchronous stream.
    /// </summary>
    /// <param name="predicate">A predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous stream of entities matching the predicate.</returns>
    IAsyncEnumerable<TEntity> FindAsStream(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    #endregion
}
