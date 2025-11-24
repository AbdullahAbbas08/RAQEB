using Raqeb.Shared.Models;

namespace Raqeb.Shared.DTOs
{
    public class LocalizationDto
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public virtual List<LanguageLocalizationDto> LanguageLocalization { get; set; }
    }
}
