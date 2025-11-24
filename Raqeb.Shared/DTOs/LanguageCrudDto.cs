namespace Raqeb.Shared.DTOs
{
    public class LanguageCrudDto 
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Icon { get; set; }
        public string Direction { get; set; }
        public IFormFile LogoForm { get; set; }

    }
}
