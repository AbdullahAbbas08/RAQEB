using Microsoft.Extensions.Options;
using Raqeb.Shared.Encryption;
using Raqeb.Shared.ViewModels;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Transactions;
using System.Xml;
using System.Xml.Xsl;

namespace EMailIntegration
{
    public class EmailIntegration
    {
        private EmailSetting emailSetting;

        public EmailIntegration(EmailSetting _emailSetting)
        {
            this.emailSetting = _emailSetting;
        }

        public void SendEmailDefault(string toEmailList, string subject, string body, bool htmlEnabled, AlternateView htmlView, string ccEmailList)
        {
            try
            {
                SmtpClient EmailSettings = new SmtpClient();

                EmailSettings.Host = this.emailSetting.SMTPServer;
                EmailSettings.Port = this.emailSetting.EmailPort;
                EmailSettings.UseDefaultCredentials = false;

                EmailSettings.Credentials = new NetworkCredential(this.emailSetting.EmailFrom, this.emailSetting.EmailPassword);
                EmailSettings.EnableSsl = Convert.ToBoolean(this.emailSetting.EnableSSL);
                EmailSettings.DeliveryMethod = SmtpDeliveryMethod.Network;

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(this.emailSetting.EmailFrom)
                };
                // mailMessage.To.Add(email);
                if (toEmailList != null)
                {
                    var individualEmails = toEmailList.Split(",");
                    foreach (var item in individualEmails)
                    {
                        mailMessage.To.Add(new MailAddress(item));

                    }
                    mailMessage.IsBodyHtml = htmlEnabled;
                    mailMessage.Subject = RemoveSpecialChars(subject);

                    if (!string.IsNullOrEmpty(ccEmailList))
                    {
                        var emails = ccEmailList.Split(',');
                        if (emails.Length > 0)
                            foreach (var item in emails)
                            {
                                mailMessage.CC.Add(item);
                            }
                    }

                    if (htmlEnabled)
                    {
                        mailMessage.AlternateViews.Add(htmlView);
                        //AlternateView htmlView = CreateAlternateView(New_Message, null, "text/html");
                    }

                    if (!String.IsNullOrEmpty(body))
                    {
                        mailMessage.Body = body;
                    }

                    EmailSettings.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static string RemoveSpecialChars(string str)
        {
            // Create  a string array and add the special characters you want to remove
            //Replace('\r', ' ').Replace('\n', ' ');
            string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]", "\r", "\n" };
            //Iterate the number of times based on the String array length.
            for (int i = 0; i < chars.Length; i++)
            {
                if (str.Contains(chars[i]))
                {
                    str = str.Replace(chars[i], " ");
                }
            }
            return str;
        }
    }
}
