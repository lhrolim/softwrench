using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Configuration.Services.Api;
using System.Net.Mail;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Metadata;
using Common.Logging;
using softWrench.sW4.Util;

namespace softWrench.sW4.Email {
    public class EmailService : ISingletonComponent {

        private static readonly Regex HtmlImgRegex = new Regex("<img[^>]+src\\s*=\\s*['\"]([^'\"]+)['\"][^>]*>");

        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailService));
        public void SendEmail(EmailData emailData) {
            var objsmtpClient = new SmtpClient();
            objsmtpClient.Host = MetadataProvider.GlobalProperty("email.smtp.host", true);
            var overriddenPort = MetadataProvider.GlobalProperty("email.smtp.port");
            if (overriddenPort != null) {
                objsmtpClient.Port = Int32.Parse(overriddenPort);
            }
            objsmtpClient.EnableSsl = "true".EqualsIc(MetadataProvider.GlobalProperty("email.stmp.enableSSL", true));
            // Send the email message
            var email = new MailMessage(emailData.SendFrom, emailData.SendTo) {
                Subject = emailData.Subject,
                Body = emailData.Message,
                IsBodyHtml = true
            };
            if (emailData.Cc != null) {
                foreach (var ccemail in emailData.Cc.Split(' ', ',', ';')) {
                    email.CC.Add(ccemail);
                }
            }
            email.IsBodyHtml = true;
            try {
                objsmtpClient.Send(email);
            } catch (Exception ex) {
                Log.Error(ex);
                throw ex;
            }
        }

        public class EmailData {
            public EmailData(string sendFrom, string sendTo, string subject, string message) {
                Validate.NotNull(sendTo, "sentTo");
                Validate.NotNull(subject, "Subject");
                SendFrom = sendFrom;
                SendTo = sendTo;
                Subject = subject;
                Message = message;
            }


            public string SendFrom { get; set; }
            public string SendTo { get; set; }
            public string Cc { get; set; }
            public string Subject { get; set; }

            public string Message { get; set; }
        }


        private static MailContentWrapper BuildContentWrapper(String html) {
            IDictionary<String, String> cidToContent = new Dictionary<string, string>();
            MatchCollection matches = HtmlImgRegex.Matches(html);
            int index = 0;
            foreach (Match match in matches) {
                var src = match.Groups[0].Value;
                if (src.Trim().Length > 0 && html.IndexOf(src, StringComparison.Ordinal) != -1) {
                    const string srcToken = "src=\"";
                    var x = src.IndexOf(srcToken, StringComparison.Ordinal);
                    var y = src.IndexOf("\"", x + srcToken.Length, StringComparison.Ordinal);
                    var srcText = src.Substring(x + srcToken.Length, y);
                    var cid = "image" + index;
                    var newSrc = src.Replace(srcText, "cid:" + cid);
                    var base64Image = srcText.Split(',')[1];
                    cidToContent.Add(cid, base64Image);
                    html = html.Replace(src, newSrc);
                    index++;
                }
            }
            return new MailContentWrapper(html, cidToContent);
        }
        /**
         * <ul>
         * <li>body: html with images referenced by CID</li>
         * <li>cidToContent: maps CID to it´s base64 content</li>
         * </ul>
         * 
         * @author Rodrigo
         */

        private class MailContentWrapper {
            private String body;
            private IDictionary<String, String> cidToImage;

            public MailContentWrapper(String body, IDictionary<String, String> cidToImage) {
                this.body = body;
                this.cidToImage = cidToImage;
            }

            public String getBody() {
                return body;
            }

            public IDictionary<String, String> getCidToImage() {
                return cidToImage;
            }

        }
    }
}
