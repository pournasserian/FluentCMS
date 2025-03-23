using FluentCMS.Entities;
using System;
using System.Collections.Generic;

namespace FluentCMS.Repositories.Abstractions.Querying;

/// <summary>
/// Represents the result of a paged query operation.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class PagedResult<TEntity> where TEntity : IBaseEntity
{
    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    public IEnumerable<TEntity> Items { get; }
    
    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; }
    
    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; }
    
    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    public long TotalCount { get; }
    
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPrevious => PageNumber > 1;
    
    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedResult{TEntity}"/> class.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    public PagedResult(IEnumerable<TEntity> items, int pageNumber, int pageSize, long totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
