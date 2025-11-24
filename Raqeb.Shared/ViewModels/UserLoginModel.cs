namespace Raqeb.Shared.ViewModels
{
    public class UserLoginModel
    {
        [Required]
        [MaxLength(450)]
        public string Email { get; set; }
        [Required]
        [MaxLength(450)]
        public string Password { get; set; }
        public string culture { get; set; }
    }


}
