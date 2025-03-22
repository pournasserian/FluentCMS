namespace FluentCMS.Entities;

public interface IAuditableEntity : IBaseEntity
{
    string? CreatedBy { get; set; }
    DateTime CreatedDate { get; set; }
    string? LastModifiedBy { get; set; }
    DateTime? LastModifiedDate { get; set; }
}


public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
}
