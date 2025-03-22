﻿namespace FluentCMS.Entities;

/// <summary>
/// Defines the contract for entities that support soft delete functionality.
/// </summary>
public interface ISoftDeleteBaseEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is marked as deleted.
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was marked as deleted.
    /// </summary>
    DateTime? DeletedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who marked the entity as deleted.
    /// </summary>
    string? DeletedBy { get; set; }
}
