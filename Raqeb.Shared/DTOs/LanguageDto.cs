using Raqeb.Shared.Models;

namespace Raqeb.Shared.DTOs
{
    public class LanguageDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Direction { get; set; }

        public virtual ICollection<LanguageLocalization> LanguageLocalization { get; set; }
    }

}
