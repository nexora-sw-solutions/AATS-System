namespace AuditApp.Domain.Entities;

public class TaxAccountRecord : AuditBaseEntity
{
    public Guid? AssignedTo { get; set; }
    public User? AssignedUser { get; set; }
}
