﻿namespace FluentCMS.Entities;

/// <summary>
/// Base entity class that implements <see cref="IBaseEntity"/> and provides 
/// common properties for all entities in the system.
/// </summary>
public abstract class BaseEntity : IBaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}
