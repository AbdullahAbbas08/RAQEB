using Microsoft.AspNetCore.Http;

namespace Raqeb.Shared.Models
{
    public class User : _Model
    {
        [Required]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress, ErrorMessage = "PleaseEnterValidEmail")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [MaxLength(150)]
        public string NameAr { get; set; }

        [Required]
        [MaxLength(150)]
        public string NameEn { get; set; }

        [Required]
        [MaxLength(15)]
        public string Mobile { get; set; }

        public string Image { get; set; }

        public byte[] Logo { get; set; }
        [NotMapped]
        public IFormFile LogoForm { get; set; }

        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<UserToken> UserTokens { get; set; }
    }
    
   
}  