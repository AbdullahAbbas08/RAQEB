namespace Raqeb.Shared.ViewModels
{
    public class ChangePasswordModel
    {

        public string? OldPassword { get; set; }

        [Required]
        [MaxLength(450)]
        public string NewPassword { get; set; }

    }

}
