namespace Raqeb.Shared.ModelInterfaces
{
    public interface IAuditableDelete
    {
        int? DeletedBy { get; set; }
        DateTimeOffset? DeletedOn { get; set; }
    }

}
