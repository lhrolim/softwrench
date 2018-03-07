using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Email;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public class DispatchSmsEmailService : BaseDispatchEmailService {
        private const int SmsCharLimit = 140;


        protected override string GetTemplateFilePath() {
            return null;
        }



        public override async Task SendEmail(DispatchTicket ticket, bool force = false) {
            var hour = ticket.CalculateHours();
            if (hour == ticket.CalculateLastSentHours() && !force) {
                //not sending the same email twice for this hour timespan
                return;
            }
            var dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>();
            var site = await dao.FindSingleByQueryAsync<GfedSite>(GfedSite.FromGFedId, ticket.GfedId);

            var to = BuildTo(site, hour);
            if (string.IsNullOrEmpty(to)) {
                return;
            }
            var from = GetFrom();
            var tuples = BuildMessages(ticket, site, hour);


            await Task.Run(() => {
                foreach (var tuple in tuples) {
                    var emailData = new EmailData(@from, to, tuple.Item1, tuple.Item2) { BCc = GetBcc() };

                    var emailService = SimpleInjectorGenericFactory.Instance.GetObject<EmailService>();
                    emailService.SendEmail(emailData);

                    //to ensure the sms are dispatched in the correct order
                    Thread.Sleep(300);
                }

                ticket.LastSent = DateTime.Now;
                if (DispatchTicketStatus.SCHEDULED.Equals(ticket.Status)) {
                    ticket.Status = DispatchTicketStatus.SCHEDULED;
                }
                LoggingUtil.DefaultLog.InfoFormat("dispatchsms messages sent to " + to);
                //last in loop update db
                dao.Save(ticket);
            });





        }

        /// <summary>
        /// There´s a limit of 140 chars that can be sent on each sms, so we need to account for that, reducing the message to its minimum
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="site"></param>
        /// <param name="hour"></param>
        /// <returns></returns>
        private IList<Tuple<string, string>> BuildMessages(DispatchTicket ticket, GfedSite site, int hour) {
            IList<Tuple<string, string>> msgs = new List<Tuple<string, string>>();

            var firstSubject = "#{0} PE Hr {1} {2}".Fmt(ticket.Id, hour, site.FacilityName);
            if (!ApplicationConfiguration.IsProd()) {
                firstSubject = "[{0}]".Fmt(ApplicationConfiguration.Profile) + firstSubject;
            }

            var firstMessage = new StringBuilder("4 Hr Rspns ");
            firstMessage.Append(ticket.FmtDateTime(ticket.DispatchExpectedDate?.Add(new TimeSpan(0, 4, 0, 0)))).Append("\n");
            firstMessage.AppendFormat("On site {0}", ticket.OnSiteDeadLine()).Append("\n");


            if (ticket.Comments != null) {
                var ticketsCommentLength = (ticket.Comments?.Length);
                if (firstMessage.Length + firstSubject.Length + ticketsCommentLength <= SmsCharLimit) {
                    firstMessage.Append(ticket.Comments);
                } else {
                    var availableSpace = SmsCharLimit - firstMessage.Length - firstSubject.Length - 3;
                    firstMessage.Append(ticket.Comments.Substring(0, availableSpace)).Append("...");
                }
            }


            firstMessage.Append(ticket.Comments);

            msgs.Add(new Tuple<string, string>(firstSubject, firstMessage.ToString()));

            foreach (var inverter in ticket.Inverters) {
                var sb = new StringBuilder();
                sb.AppendLine(inverter.AssetDescription);

                if (inverter.FailureDetails != null) {
                    if (sb.Length + inverter.FailureDetails.Length <= SmsCharLimit) {
                        sb.Append(inverter.FailureDetails);
                    } else {
                        var availableSpace = SmsCharLimit - sb.Length - 3;
                        sb.Append(inverter.FailureDetails.Substring(0, availableSpace)).Append("...");
                    }
                }

                msgs.Add(new Tuple<string, string>("", sb.ToString()));
            }

            var redirectService = SimpleInjectorGenericFactory.Instance.GetObject<RedirectService>();
            msgs.Add(new Tuple<string, string>("", "Accept: " + redirectService.GetActionUrl("DispatchEmail", "Ac", "token={0}".Fmt(ticket.AccessToken))));
            if (hour <= 4) {
                msgs.Add(new Tuple<string, string>("", "Reject: " + redirectService.GetActionUrl("DispatchEmail", "Rj", "token={0}".Fmt(ticket.AccessToken))));
            }



            return msgs;

        }

        protected override string GetBcc() {
            var bbc = SwConstants.DevTeamEmail + "; ";
            bbc += ConfigFacade.Lookup<string>(FirstSolarDispatchConfigurations.BccSmsEmailsToNotify);
            return bbc;
        }


        public override string BuildTo(GfedSite site, int hour) {
            var toList = new List<string>();
            toList.Add(ConfigFacade.Lookup<string>(FirstSolarDispatchConfigurations.ToSmsEmailsToNotify));

            if (ApplicationConfiguration.IsProd()) {
                if (!string.IsNullOrEmpty(site.PrimaryContactSmsEmail)) {
                    toList.Add(site.PrimaryContactSmsEmail);
                }
                if (!string.IsNullOrEmpty(site.EscalationContactSmsEmail) && hour > 0) {
                    toList.Add(site.EscalationContactSmsEmail);
                }
                if (hour > 1 && ApplicationConfiguration.IsProd()) {
                    toList.Add("6198882505@txt.att.net");
                }

            }


            return string.Join("; ", toList);
        }

        protected override void AfterSend(EmailData emailData) {
            LoggingUtil.DefaultLog.InfoFormat("dispatchsms sent to " + emailData.SendTo);
        }
    }
}
