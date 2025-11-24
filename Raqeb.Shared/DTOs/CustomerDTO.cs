namespace Raqeb.Shared.DTOs
{
    public class CustomerDTO
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Logo { get; set; }
        public string LogoBase64 { get; set; }
        public IFormFile LogoForm { get; set; }
        public int MaxNumberOfSites { get; set; }
        public int? SitesCount { get; set; }
    }




}
