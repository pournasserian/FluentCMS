using FluentCMS.Entities;

namespace FluentCMS.Repositories.Abstractions.Querying;

public class PagedResult<TEntity> where TEntity : IBaseEntity
{
    public IEnumerable<TEntity> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public long TotalCount { get; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;

    public PagedResult(IEnumerable<TEntity> items, int pageNumber, int pageSize, long totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
