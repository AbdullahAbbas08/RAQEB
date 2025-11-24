namespace Raqeb.Shared.ViewModels
{
    public class AppSettings
    {
        public EmailSetting EmailSetting { get; set; }
    }

    public class EmailSetting
    {
        public string SMTPServer { get; set; }
        public int EmailPort { get; set; }
        public string EmailFrom { get; set; }
        public string EmailPassword { get; set; }
        public bool EnableSSL { get; set; }
    }


}
