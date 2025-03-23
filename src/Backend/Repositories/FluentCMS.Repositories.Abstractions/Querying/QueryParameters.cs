using FluentCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.Abstractions.Querying;

/// <summary>
/// Represents parameters for querying entities with support for filtering, sorting, and pagination.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class QueryParameters<TEntity> where TEntity : IBaseEntity
{
    /// <summary>
    /// Gets or sets the filter expression.
    /// </summary>
    public Expression<Func<TEntity, bool>>? FilterExpression { get; set; }
    
    /// <summary>
    /// Gets the sort options.
    /// </summary>
    public List<SortOption<TEntity>> SortOptions { get; } = new();
    
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Sets the filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>The current instance for chaining.</returns>
    public QueryParameters<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        FilterExpression = filter;
        return this;
    }
    
    /// <summary>
    /// Adds an ascending sort option.
    /// </summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="keySelector">The key selector expression.</param>
    /// <returns>The current instance for chaining.</returns>
    public QueryParameters<TEntity> AddSortAscending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        SortOptions.Add(new SortOption<TEntity>(keySelector, SortDirection.Ascending));
        return this;
    }
    
    /// <summary>
    /// Adds a descending sort option.
    /// </summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="keySelector">The key selector expression.</param>
    /// <returns>The current instance for chaining.</returns>
    public QueryParameters<TEntity> AddSortDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        SortOptions.Add(new SortOption<TEntity>(keySelector, SortDirection.Descending));
        return this;
    }
    
    /// <summary>
    /// Sets the pagination parameters.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The current instance for chaining.</returns>
    public QueryParameters<TEntity> WithPaging(int pageNumber, int pageSize)
    {
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Max(1, pageSize);
        return this;
    }
}
