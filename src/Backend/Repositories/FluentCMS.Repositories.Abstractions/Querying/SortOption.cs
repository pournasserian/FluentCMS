using FluentCMS.Entities;
using System.Linq.Expressions;

namespace FluentCMS.Repositories.Abstractions.Querying;

/// <summary>
/// Represents a sort option for entity queries.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class SortOption<TEntity> where TEntity : IBaseEntity
{
    /// <summary>
    /// Gets the key selector expression.
    /// </summary>
    public LambdaExpression KeySelector { get; }
    
    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SortDirection Direction { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SortOption{TEntity}"/> class.
    /// </summary>
    /// <param name="keySelector">The key selector expression.</param>
    /// <param name="direction">The sort direction.</param>
    public SortOption(LambdaExpression keySelector, SortDirection direction)
    {
        KeySelector = keySelector;
        Direction = direction;
    }
}
