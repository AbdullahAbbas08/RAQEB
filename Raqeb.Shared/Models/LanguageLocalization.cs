namespace Raqeb.Shared.Models
{
    public class LanguageLocalization : _Model
    {
        [Required]
        public string Value { get; set; }

        public int LocalizationId { get; set; }
        [ForeignKey("LocalizationId")]
        public virtual Localization Localization { get; set; }
        
        public int LanguageId { get; set; }
        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; }
    }
    
  
    
  

}  