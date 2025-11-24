using EMailIntegration;
using Raqeb.Shared.Models;
using System.Net.Mail;

namespace Raqeb.BL.Repositories
{
    public interface IMailServices
    {
        void SendEmailDefault(string toEmailList, string subject, string body, bool htmlEnabled, AlternateView htmlView, string ccEmailList);
        void GetATSNotificationEmailMessage(ref string mailMessage, string AtsSerial, string ATSCloseDate, string AuditorName, string SiteName, string ATSLink);
        void GetATSCloseNotificationEmailMessage(ref string mailMessage, string AtsSerial, string ResponsibleUserName, string CorrectionAction, string SiteName);

        AlternateView CreateAlternateView(string message, string logoPath);

    }

    public class MailServices : Repository<SystemSetting>, IMailServices
    {
        private EmailIntegration EmailIntegration { get; set; }

        public MailServices(IUnitOfWork _uow) : base(_uow)
        {

            EmailSetting emailSetting = new EmailSetting();


            emailSetting.SMTPServer = _uow.DbContext.SystemSettings.Where(x => x.Code.Equals("SMTPServer")).FirstOrDefault().Value; //"smtp.office365.com";
            emailSetting.EmailPort = Convert.ToInt32(_uow.DbContext.SystemSettings.Where(x => x.Code.Equals("EmailPort")).FirstOrDefault().Value); //Convert.ToInt32("587");
            emailSetting.EmailFrom = _uow.DbContext.SystemSettings.Where(x => x.Code.Equals("EmailFrom")).FirstOrDefault().Value; //"grcsend.1987@outlook.com";
            emailSetting.EmailPassword = _uow.DbContext.SystemSettings.Where(x => x.Code.Equals("EmailPassword")).FirstOrDefault().Value; //"963@852Gg";
            emailSetting.EnableSSL = Convert.ToBoolean(_uow.DbContext.SystemSettings.Where(x => x.Code.Equals("EnableSSL")).FirstOrDefault().Value);


            //emailSetting.SMTPServer = "smtp.office365.com";
            //emailSetting.EmailPort = Convert.ToInt32("587");
            //emailSetting.EmailFrom = "grcsend.1987@outlook.com";
            //emailSetting.EmailPassword = "963@852Gg";
            //emailSetting.EnableSSL = Convert.ToBoolean("true");

            this.EmailIntegration = new EmailIntegration(emailSetting);
        }

        public void SendEmailDefault(string toEmailList, string subject, string body, bool htmlEnabled, AlternateView htmlView, string ccEmailList)
        {
            this.EmailIntegration.SendEmailDefault(toEmailList, subject, body, htmlEnabled, htmlView, ccEmailList);
        }

        public void GetATSNotificationEmailMessage(ref string mailMessage, string AtsSerial, string ATSCloseDate, string AuditorName, string SiteName, string ATSLink)
        {
            try
            {
                //string systemsettinglink = _systemSettingsService.GetByCode("taskDetailsLink").SystemSettingValue;

                string Email_style = @"
                                            text-align: center;
                                            flex-direction: column;
                                            justify-content: center;
                                            align-items: center;";
                string Email_image_style = @" 
                                            text-align: center !important;
                                            margin: auto !important;
                                            justify-content: center;
                                            margin: 32px;";
                string image_style = @"margin: 0;";
                string table_style = @"
                                            width: 100%;
                                            margin-bottom: -5px;
                                            direction: rtl;
                                            border: 1px solid #cccccc;
                                            display: table;
                                            border-collapse: collapse;
                                            border-spacing: 2px;
                                            border-color: grey;";
                string tr_style = @" 
                                            //white-space: normal;
                                            //line-height: normal;
                                            font-weight: normal;
                                            font-size: medium;
                                            font-style: normal;
                                            color: -internal-quirk-inherit;
                                            text-align: start;
                                            font-variant: normal;
                                    ";
                string td_style = @"
                                    padding: 10px;
                                    width: 3px;
                                    margin-bottom: -3px;
                                    font-weight: 600;
                                    margin: -6px;
                                    
                                    ";
                string td_style_En = @"
                                        padding: 10px;
                                        width: 3px;
                                        margin-bottom: -3px;
                                        font-weight: 600;
                                        margin: -6px;
                                        direction: ltr;";
                string w_20 = @"width: 20%;";
                string w_30 = @"width: 30%;";
                string rtl = @"direction: rtl;";
                string text_center = @" text-align: center;";
                string tr_display = @" display: {displayOption}; ";


                // Add url
                string HtmlString_new = $@"                                           
                                          <div style='{Email_style}'>
                                           <img style='{Email_image_style}' src='cid:TopHeader'>
                                           <div style='
                                                width: 100%;                                                
                                                display: flex;
                                                flex-direction: column;
                                                justify-content: center;
                                                margin: 0;
                                                padding: 0;
                                                
                                            '>		
                                         <table style='width: 100%' border='1'>
                                            <tr style='{tr_style}'>
                                                <td colspan='5' style='{td_style}'>
                                                    <h2 style=' text-align: center; background: #04049c; padding: 9px 0; font-weight: 900; margin-bottom: 0px; color: #fff;'> تنبيه بالمهام </h2>
                                                </td>
                                            </tr>
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> ATS Serial </td>
                                                <td colspan='3' style='{td_style + ';' + rtl}text-align: center;'>{AtsSerial}</td>
                                                <td style ='{td_style + ';' + w_20 + rtl}text-align: center;'> رقم </td>   
                                            </tr >   
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> ATS Close Date </td>
                                                <td colspan='3' style='{td_style + ';' + rtl}text-align: center;'>{ATSCloseDate}</td>
                                                <td style ='{td_style + ';' + w_20 + rtl}text-align: center;'> تاريخ انهاء  </td>   
                                            </tr >   
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> Auditor </td>   
                                                <td colspan='3' style='{td_style + ';'}text-align: center;'><span style='unicode-bidi: bidi-override;direction: rtl;'>{AuditorName}</span></td>
                                                <td style='{td_style + ';' + w_20 + rtl}text-align: center;'> المرسل </td>
                                            </tr >  
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> Site Name </td>   
                                                <td colspan='3' style='{td_style + ';'}text-align: center;'><span style='unicode-bidi: bidi-override;'>{SiteName}</span></td>
                                                <td style='{td_style + ';' + w_20 + rtl}text-align: center;'> اسم المشروع </td>
                                            </tr >  
                                           
                                            <tr style='{tr_style}'>
                                                <td colspan='5' style='{td_style + ';' + rtl}text-align: center; padding-top: 20px'><b><h3> <a href ='{ATSLink}'>رابط ATS<a/> </h3></b></td>
                                            </tr > 

                                            </table>
                                                </div>                                                     
                                                    </div>";

                //< tr style = '{tr_style}' >
                //                               < td colspan = '5' style = '{td_style + '; ' + rtl}text-align: center; padding-top: 20px' >< b >< h3 > < a href = '{policylink}' > رابط السياسة < a /> </ h3 ></ b ></ td >
                //                           </ tr >

                mailMessage = HtmlString_new;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetATSCloseNotificationEmailMessage(ref string mailMessage, string AtsSerial, string ResponsibleUserName, string CorrectionAction, string SiteName)
        {
            try
            {
                //string systemsettinglink = _systemSettingsService.GetByCode("taskDetailsLink").SystemSettingValue;

                string Email_style = @"
                                            text-align: center;
                                            flex-direction: column;
                                            justify-content: center;
                                            align-items: center;";
                string Email_image_style = @" 
                                            text-align: center !important;
                                            margin: auto !important;
                                            justify-content: center;
                                            margin: 32px;";
                string image_style = @"margin: 0;";
                string table_style = @"
                                            width: 100%;
                                            margin-bottom: -5px;
                                            direction: rtl;
                                            border: 1px solid #cccccc;
                                            display: table;
                                            border-collapse: collapse;
                                            border-spacing: 2px;
                                            border-color: grey;";
                string tr_style = @" 
                                            //white-space: normal;
                                            //line-height: normal;
                                            font-weight: normal;
                                            font-size: medium;
                                            font-style: normal;
                                            color: -internal-quirk-inherit;
                                            text-align: start;
                                            font-variant: normal;
                                    ";
                string td_style = @"
                                    padding: 10px;
                                    width: 3px;
                                    margin-bottom: -3px;
                                    font-weight: 600;
                                    margin: -6px;
                                    
                                    ";
                string td_style_En = @"
                                        padding: 10px;
                                        width: 3px;
                                        margin-bottom: -3px;
                                        font-weight: 600;
                                        margin: -6px;
                                        direction: ltr;";
                string w_20 = @"width: 20%;";
                string w_30 = @"width: 30%;";
                string rtl = @"direction: rtl;";
                string text_center = @" text-align: center;";
                string tr_display = @" display: {displayOption}; ";


                // Add url
                string HtmlString_new = $@"                                           
                                          <div style='{Email_style}'>
                                           <img style='{Email_image_style}' src='cid:TopHeader'>
                                           <div style='
                                                width: 100%;                                                
                                                display: flex;
                                                flex-direction: column;
                                                justify-content: center;
                                                margin: 0;
                                                padding: 0;
                                                
                                            '>		
                                         <table style='width: 100%' border='1'>
                                            <tr style='{tr_style}'>
                                                <td colspan='5' style='{td_style}'>
                                                    <h2 style=' text-align: center; background: #04049c; padding: 9px 0; font-weight: 900; margin-bottom: 0px; color: #fff;'> تنبيه باغلاق المهام </h2>
                                                </td>
                                            </tr>
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> ATS Serial </td>
                                                <td colspan='3' style='{td_style + ';' + rtl}text-align: center;'>{AtsSerial}</td>
                                                <td style ='{td_style + ';' + w_20 + rtl}text-align: center;'> رقم </td>   
                                            </tr >  
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> Responsible User </td>   
                                                <td colspan='3' style='{td_style + ';'}text-align: center;'><span style='unicode-bidi: bidi-override;direction: rtl;'>{ResponsibleUserName}</span></td>
                                                <td style='{td_style + ';' + w_20 + rtl}text-align: center;'> مسؤول المهمة </td>
                                            </tr >  
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> Correction Action </td>   
                                                <td colspan='3' style='{td_style + ';'}text-align: center;'><span style='unicode-bidi: bidi-override;'>{CorrectionAction}</span></td>
                                                <td style='{td_style + ';' + w_20 + rtl}text-align: center;'> الاجراء </td>
                                            </tr >  
                                            <tr style='{tr_style}'>
                                                <td style='{td_style_En + ';' + w_20}text-align: center;'> Site Name </td>   
                                                <td colspan='3' style='{td_style + ';'}text-align: center;'><span style='unicode-bidi: bidi-override;'>{SiteName}</span></td>
                                                <td style='{td_style + ';' + w_20 + rtl}text-align: center;'> اسم المشروع </td>
                                            </tr >  
                                           
                                            </table>
                                                </div>                                                     
                                                    </div>";

                //< tr style = '{tr_style}' >
                //                               < td colspan = '5' style = '{td_style + '; ' + rtl}text-align: center; padding-top: 20px' >< b >< h3 > < a href = '{policylink}' > رابط السياسة < a /> </ h3 ></ b ></ td >
                //                           </ tr >

                mailMessage = HtmlString_new;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public AlternateView CreateAlternateView(string message, string logoPath)
        {
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message, null, "text/html");

            string path_TopHeader = Path.Combine(logoPath); //My TopHeader

            LinkedResource imagelink_TopHeader = new LinkedResource(path_TopHeader, "image/png")
            {
                ContentId = "TopHeader",

                TransferEncoding = System.Net.Mime.TransferEncoding.Base64
            };
            htmlView.LinkedResources.Add(imagelink_TopHeader);

            return htmlView;
        }

    }

}
