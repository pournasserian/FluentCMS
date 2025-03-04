﻿namespace FluentCMS.Repositories.EFCore;

public abstract class AuditableEntityRepository<TEntity>(FluentCmsDbContext dbContext, IApiExecutionContext apiExecutionContext) : EntityRepository<TEntity>(dbContext), IAuditableEntityRepository<TEntity> where TEntity : class, IAuditableEntity
{
    protected readonly IApiExecutionContext ApiExecutionContext = apiExecutionContext;

    public override async Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        SetAuditableFieldsForCreate(entity);
        return await base.Create(entity, cancellationToken);
    }

    public override async Task<IEnumerable<TEntity>> CreateMany(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
            SetAuditableFieldsForCreate(entity);

        return await base.CreateMany(entities, cancellationToken);
    }

    public override async Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        var existing = await GetById(entity.Id, cancellationToken);
        if (existing is null)
            return default;

        SetAuditableFieldsForUpdate(entity, existing);

        return await base.Update(entity, cancellationToken);
    }

    protected void SetAuditableFieldsForCreate(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = ApiExecutionContext.Username;
    }

    protected void SetAuditableFieldsForUpdate(TEntity entity, TEntity oldEntity)
    {
        entity.CreatedAt = oldEntity.CreatedAt;
        entity.CreatedBy = oldEntity.CreatedBy;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = ApiExecutionContext.Username;
    }
}
