using FluentCMS.Entities;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.Abstractions.Querying;

public class QueryParameters<TEntity> where TEntity : IBaseEntity
{
    public Expression<Func<TEntity, bool>>? FilterExpression { get; set; }

    public List<SortOption<TEntity>> SortOptions { get; } = new();

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public QueryParameters<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        FilterExpression = filter;
        return this;
    }

    public QueryParameters<TEntity> AddSortAscending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        SortOptions.Add(new SortOption<TEntity>(keySelector, SortDirection.Ascending));
        return this;
    }

    public QueryParameters<TEntity> AddSortDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        SortOptions.Add(new SortOption<TEntity>(keySelector, SortDirection.Descending));
        return this;
    }

    public QueryParameters<TEntity> WithPaging(int pageNumber, int pageSize)
    {
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Max(1, pageSize);
        return this;
    }
}
