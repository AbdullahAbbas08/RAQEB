namespace Raqeb.Shared.Models
{
    public class CustomerSystemSetting : _Model
    {
        [Required]
        [MaxLength(150)]
        public string Value { get; set; }

        public int SystemSettingId  { get; set; }
        [ForeignKey("SystemSettingId")]
        public virtual SystemSetting SystemSetting  { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }

}  