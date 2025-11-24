using System.Net.Mail;

namespace Raqeb.Shared.ViewModels
{
    public class AuthModel
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
        public string refreshToken { get; set; }
        //public List<RolesViewModels> Roles { get; set; }
        public int Count { get; set; }
    }

}
