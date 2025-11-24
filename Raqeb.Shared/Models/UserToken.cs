namespace Raqeb.Shared.Models
{
    public class UserToken 
    {
        public int ID { get; set; }
        public string AccessTokenHash { get; set; }

        public DateTimeOffset AccessTokenExpiresDateTime { get; set; }

        [MaxLength(450)]
        public string RefreshTokenIdHash { get; set; }

        [MaxLength(450)]
        public string RefreshTokenIdHashSource { get; set; }

        public DateTimeOffset RefreshTokenExpiresDateTime { get; set; }

        public int UserId { get; set; }
        public int? ApplicationType { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

}