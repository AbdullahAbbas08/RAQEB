namespace Raqeb.Shared.Models
{
    public class Localization : _Model
    {
        [Required]
        public string Code { get; set; }
        public virtual List<LanguageLocalization> LanguageLocalization { get; set; }
    }
    
  

}   