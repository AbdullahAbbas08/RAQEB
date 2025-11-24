namespace Raqeb.Shared.DTOs
{
    public class UserDTO
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Mobile { get; set; }
        public string Image { get; set; }
        public byte[] Logo { get; set; }
        [NotMapped]
        public IFormFile LogoForm { get; set; }
        public string LogoBase64 { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
    }

}
 