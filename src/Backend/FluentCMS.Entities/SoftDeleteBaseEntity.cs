﻿namespace FluentCMS.Entities;

/// <summary>
/// Base entity implementation that supports soft delete functionality by 
/// extending <see cref="BaseEntity"/> and implementing <see cref="ISoftDeleteBaseEntity"/>.
/// </summary>
public class SoftDeleteBaseEntity : BaseEntity, ISoftDeleteBaseEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is marked as deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was marked as deleted.
    /// </summary>
    public DateTime? DeletedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who marked the entity as deleted.
    /// </summary>
    public string? DeletedBy { get; set; }
}
