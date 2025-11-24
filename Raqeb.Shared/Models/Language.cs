namespace Raqeb.Shared.Models
{
    public class Language : _Model
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        public string Code { get; set; }
        public string Icon { get; set; }

        [MaxLength(150)]
        public string Direction { get; set; }

        public virtual ICollection<LanguageLocalization> LanguageLocalization { get; set; }
    }
   
}   