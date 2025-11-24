namespace Raqeb.Shared.ViewModels
{
    public class LanguageLocalizationListDto
    {
        public int ID { get; set; }
        public string Value { get; set; }
        public int LocalizationId { get; set; }
        public string LocalizationCode { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
    }
}
