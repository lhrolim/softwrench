using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using cts.commons.portable.Util;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Util;
using LogManager = log4net.LogManager;
using Polly;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Exceptions;

namespace softWrench.sW4.Email {
    public class EmailService : IEmailService {
        private const int TRY_AGAIN_COUNT = 3;

        private static readonly Regex HtmlInlineImgRegex = new Regex("<img[^>]+src\\s*=\\s*['\"]\\s*data:([^'\"]+)['\"][^>]*>");

        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailService));

        [Import]
        private IConfigurationFacade ConfigFacade { get; set; }


        public EmailService() {
            Log.DebugFormat("init log...");
        }



        [CanBeNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        private SmtpClient ConfiguredSmtpClient() {
            var objsmtpClient = new SmtpClient();

            var enabled = ConfigFacade.Lookup<bool>(ConfigurationConstants.Email.Enabled);
            if (!enabled) {
                Log.WarnFormat("smtp server is disabled. Activate it under the configuration section");
                return null;
            }

            var host = ConfigFacade.Lookup<string>(ConfigurationConstants.Email.Host);
            if (host == null) {
                Log.WarnFormat("smtp server is not properly setup. Please visit the configuration section");
                throw new MissingConfigurationException(
                    "smtp server is not properly setup. Please visit the configuration section. (Set it to 'disabled' to prevent sending emails at all) ");
            }

            objsmtpClient.Host = host;

            var overriddenPort = ConfigFacade.Lookup<string>(ConfigurationConstants.Email.Port);
            if (overriddenPort != null) {
                objsmtpClient.Port = Int32.Parse(overriddenPort);
            }

            objsmtpClient.EnableSsl = ConfigFacade.Lookup<bool>(ConfigurationConstants.Email.EnableSSL);

            // Increase timeout value if needed - depended on site 
            var timeout = ConfigFacade.Lookup<int?>(ConfigurationConstants.Email.Timeout);
            if (timeout != null) {
                objsmtpClient.Timeout = timeout.Value;
            }

            var username = ConfigFacade.Lookup<string>(ConfigurationConstants.Email.UserName);
            var password = ConfigFacade.Lookup<string>(ConfigurationConstants.Email.Password);
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
                    smtpClient?.Send(email);
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
