using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using cts.commons.portable.Util;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using log4net;
using softWrench.sW4.Metadata;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Util;
using LogManager = log4net.LogManager;
using Polly;

namespace softWrench.sW4.Email {
    public class EmailService : IEmailService {
        private const int TRY_AGAIN_COUNT = 3;

        private static readonly Regex HtmlInlineImgRegex = new Regex("<img[^>]+src\\s*=\\s*['\"]\\s*data:([^'\"]+)['\"][^>]*>");

        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailService));

        public EmailService() {
            Log.DebugFormat("init log...");
        }

        private SmtpClient ConfiguredSmtpClient() {
            var objsmtpClient = new SmtpClient();

            objsmtpClient.Host = MetadataProvider.GlobalProperty("email.smtp.host", true);

            var overriddenPort = MetadataProvider.GlobalProperty("email.smtp.port");
            if (overriddenPort != null) {
                objsmtpClient.Port = Int32.Parse(overriddenPort);
            }

            objsmtpClient.EnableSsl = "true".EqualsIc(MetadataProvider.GlobalProperty("email.smtp.enableSSL"));

            // Increase timeout value if needed - depended on site 
            var timeout = MetadataProvider.GlobalProperty("email.smtp.timeout");
            if (timeout != null) {
                objsmtpClient.Timeout = Int32.Parse(timeout);
            }

            var username = MetadataProvider.GlobalProperty("email.smtp.username");
            var password = MetadataProvider.GlobalProperty("email.smtp.password");
            if (username != null && password != null) {
                objsmtpClient.UseDefaultCredentials = false;
                objsmtpClient.Credentials = new NetworkCredential(username, password);
                objsmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            }


            Log.DebugFormat("smtp client object: host= {0}, port={1}, enableSSL ={2} , useDefaultCredentials= {3}, username ={4} ".Fmt(objsmtpClient.Host, objsmtpClient.Port, objsmtpClient.EnableSsl, objsmtpClient.UseDefaultCredentials, username));

            return objsmtpClient;
        }

        /// <summary>
        /// Sends email in a fire-and-forget way.
        /// </summary>
        /// <param name="emailData"></param>
        public virtual void SendEmailAsync(EmailData emailData) {
            Log.DebugFormat("sending email asynchronoysly");
            // Send the email message asynchronously
            Task.Run(() => SendEmail(emailData));
        }

        /// <summary>
        /// Sends email synchronously
        /// </summary>
        /// <param name="emailData">The email data</param>
        public virtual void SendEmail(EmailData emailData) {
            try {
                Policy.Handle<SmtpFailedRecipientsException>(ex => {
                    var tryAgain = CheckEmailClientMailboxBusy(ex.StatusCode);

                    if (!tryAgain) {
                        for (int i = 0; i < ex.InnerExceptions.Length; i++) {
                            tryAgain = CheckEmailClientMailboxBusy(ex.InnerExceptions[i].StatusCode);
                            if (tryAgain) {
                                break;
                            }
                        }
                    }

                    return tryAgain;
                })
                .WaitAndRetry(TRY_AGAIN_COUNT, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                )
                .Execute(() => {
                    Log.DebugFormat("start sending email");
                    var smtpClient = ConfiguredSmtpClient();
                    var email = BuildMailMessage(emailData);
                    // Send the email message synchronously
                    smtpClient.Send(email);
                });
            } catch (Exception ex) {
                Log.Error(ex);
                throw;
            }
        }

        public virtual EmailAttachment CreateAttachment(string fileContent, string attachmentName) {
            try {
                return CreateAttachment(Encoding.UTF8.GetBytes(fileContent), attachmentName);
            } catch (Exception e) {
                Log.Error("error creating attachment", e);
                throw;
            }


        }

        public virtual EmailAttachment CreateAttachment(Byte[] fileContent, string attachmentName) {
            try {
                return new EmailAttachment() { AttachmentBinary = fileContent, AttachmentName = attachmentName };
            } catch (Exception e) {
                Log.Error("error creating attachment", e);
                throw;
            }
        }

        private bool CheckEmailClientMailboxBusy(SmtpStatusCode status) {
            return (status == SmtpStatusCode.MailboxBusy || status == SmtpStatusCode.MailboxUnavailable);
        }

        private MailMessage BuildMailMessage(EmailData emailData) {
            var email = new MailMessage() {
                From = new MailAddress(emailData.SendFrom ?? MetadataProvider.GlobalProperty("defaultEmail")),
                Subject = emailData.Subject,
                IsBodyHtml = true
            };

            var anyAddressSet = false;

            if (!string.IsNullOrEmpty((emailData.SendTo))) {
                foreach (
                    var emailaddress in emailData.SendTo.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (AllowedToAdd(emailaddress)) {
                        email.To.Add(emailaddress.Trim());
                        anyAddressSet = true;
                    }
                }
            }

            if (!anyAddressSet) {
                throw new InvalidOperationException(
                    "Email cannot be sent to {0}. Dev environments limit the domain which can be sent to avoid sending unadvertised test emails"
                        .Fmt(emailData.SendTo));
            }


            if (!string.IsNullOrEmpty(emailData.Cc)) {
                foreach (var emailaddress in emailData.Cc.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (AllowedToAdd(emailaddress)) {
                        email.CC.Add(emailaddress.Trim());
                    }
                }
            }

            if (!string.IsNullOrEmpty(emailData.BCc)) {
                foreach (var emailaddress in emailData.BCc.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (AllowedToAdd(emailaddress)) {
                        email.Bcc.Add(emailaddress.Trim());
                    }
                }
            }

            if (emailData.Attachments != null) {
                HandleAttachments(emailData.Attachments, email);
            }

            // adds email body
            email.AlternateViews.Add(BuildContent(emailData.Message));

            return email;
        }

        public static AlternateView BuildContent(string html) {
            var matches = HtmlInlineImgRegex.Matches(html);
            var inlines = new List<LinkedResource>();

            foreach (Match match in matches) {
                var src = match.Groups[0].Value;
                if (src.Trim().Length == 0 || html.IndexOf(src, StringComparison.Ordinal) == -1) {
                    continue;
                }

                const string srcToken = "src=\"";
                var start = src.IndexOf(srcToken, StringComparison.Ordinal) + srcToken.Length;
                var end = src.IndexOf("\"", start, StringComparison.Ordinal);
                var srcText = src.Substring(start, end - start);
                var base64Image = srcText.Split(',')[1];

                var byteArray = Convert.FromBase64String(base64Image);
                var stream = new MemoryStream(byteArray);
                var cid = Guid.NewGuid().ToString();
                var inline = new LinkedResource(stream) { ContentId = cid };
                inlines.Add(inline);

                var newSrc = src.Replace(srcText, "cid:" + cid);
                html = html.Replace(src, newSrc);
            }
            var alternateView = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
            inlines.ForEach(inline => alternateView.LinkedResources.Add(inline));

            return alternateView;
        }

        private static Boolean AllowedToAdd(string emailaddress) {
            if (!ApplicationConfiguration.IsLocal()) {
                return true;
            }
            if (!new EmailAddressAttribute().IsValid(emailaddress)) {
                Log.WarnFormat("The email ( {0}) is not valid".Fmt(emailaddress));
                return false;
            }

            var domain = emailaddress.Split('@')[1];
            if (!domain.EqualsAny("controltechnologysolutions.com", "softwrenchsolutions.com", "amlabs.com.br", "gmail.com")) {
                Log.WarnFormat("This email ( {0}) is not valid for this environment".Fmt(emailaddress));
                return false;
            }

            return true;
        }

        private void HandleAttachments(List<EmailAttachment> attachments, MailMessage email) {
            foreach (var attachment in attachments) {
                byte[] bytes;
                if (attachment.AttachmentBinary != null) {
                    bytes = attachment.AttachmentBinary;
                } else {
                    var htmlData = attachment.AttachmentData;
                    var encodedAttachment = htmlData.Substring(htmlData.IndexOf(",", StringComparison.Ordinal) + 1);
                    bytes = Convert.FromBase64String(encodedAttachment);
                }
                email.Attachments.Add(new Attachment(new MemoryStream(bytes), attachment.AttachmentName));
            }
        }

    }
}
