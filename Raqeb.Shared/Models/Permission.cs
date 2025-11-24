namespace Raqeb.Shared.Models
{
    public class Permission: _Model
    {
        [Required]
        [MaxLength(150)]
        public string Code { get; set; }
        public virtual Localization Localization { get; set; }
    }

}  