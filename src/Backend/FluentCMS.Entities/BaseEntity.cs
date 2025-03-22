namespace FluentCMS.Entities;

public interface IBaseEntity
{
    Guid Id { get; set; }
}

public class BaseEntity : IBaseEntity
{
    public Guid Id { get; set; }
}
