using Raqeb.Shared.Models;

namespace Raqeb.Shared.DTOs
{
    public class LanguageLocalizationDto
    {
        public int ID { get; set; }
        public string Value { get; set; }
        public virtual LocalizationDto Localization { get; set; }
    }

}