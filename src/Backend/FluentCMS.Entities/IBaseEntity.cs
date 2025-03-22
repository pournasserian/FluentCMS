﻿namespace FluentCMS.Entities;

/// <summary>
/// Defines the contract for base entities in the system that include 
/// auditing and identification properties.
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    string? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    DateTime? LastModifiedDate { get; set; }
}
