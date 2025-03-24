using FluentCMS.Entities;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.Abstractions.Querying;

public class SortOption<TEntity> where TEntity : IBaseEntity
{
    public LambdaExpression KeySelector { get; }

    public SortDirection Direction { get; }

    public SortOption(LambdaExpression keySelector, SortDirection direction)
    {
        KeySelector = keySelector;
        Direction = direction;
    }
}
