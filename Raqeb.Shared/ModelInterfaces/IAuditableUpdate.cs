namespace Raqeb.Shared.ModelInterfaces
{
    public interface IAuditableUpdate
    {
        int? UpdatedBy { get; set; }
        DateTimeOffset? UpdatedOn { get; set; }
    }
}
