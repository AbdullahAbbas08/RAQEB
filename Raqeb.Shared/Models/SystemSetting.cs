namespace Raqeb.Shared.Models
{
    public class SystemSetting:_Model
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Type { get; set; }
        public string Notes { get; set; }
        public string Value { get; set; }
        public virtual Localization Localization { get; set; }
    }

}  