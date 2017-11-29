using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.api.classes.fwk.context;
using DotLiquid;
using softWrench.sW4.Util;
using cts.commons.portable.Util;
using System.Threading.Tasks;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Email;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public abstract class BaseDispatchEmailService : BaseDispatchGenericEmailService {
        protected List<object> BuildInverterInfo(DispatchTicket ticket) {
            return ticket.Inverters?.Select(BuildInverterInfo).ToList() ?? new List<object>();
        }

        public async Task SendEmail(DispatchTicket ticket) {
            var hour = ticket.CalculateHours();
            if (hour == ticket.CalculateLastSentHours() && !(hour == 0 && ticket.ImmediateDispatch)) {
                //not sending the same email twice for this hour timespan
                return;
            }
            var dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>();
            var site = await dao.FindSingleByQueryAsync<GfedSite>(GfedSite.FromGFedId, ticket.GfedId);
            var subject = "[#{0}] PE Dispatch (Hour {1}) – {2}".Fmt(ticket.Id, hour, site.FacilityName);
            if (!ApplicationConfiguration.IsProd()) {
                subject = "[{0}]".Fmt(ApplicationConfiguration.Profile) + subject;
            }
            var to = BuildTo(site, hour);
            if (string.IsNullOrEmpty(to)) {
                return;
            }
            var from = GetFrom();
            var msg = BuildMessage(ticket, site, hour <= 4);
            var emailData = new EmailData(@from, to, subject, msg);
            emailData.BCc += SwConstants.DevTeamEmail;

            var emailService = SimpleInjectorGenericFactory.Instance.GetObject<EmailService>();
            emailService.SendEmailAsync(emailData, async success => {
                ticket.LastSent = DateTime.Now;
                if (DispatchTicketStatus.SCHEDULED.Equals(ticket.Status)) {
                    ticket.Status = DispatchTicketStatus.SCHEDULED;
                }
                await dao.SaveAsync(ticket);

            });
        }

        private static object BuildInverterInfo(Inverter inverter) {
            var sb = new StringBuilder();
            var maximoDao = SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>();
            var assetDescription = maximoDao.FindSingleByNativeQuery<string>("select description from asset where assetnum = ? and siteid = ?", inverter.AssetNum, inverter.Siteid);
            sb.Append(inverter.AssetNum).Append(" - Class ").Append(inverter.FailureClass).Append(" - ").Append(assetDescription).Append(" - ").Append(inverter.ErrorCodes);
            return new {
                description = sb.ToString(),
                details = inverter.FailureDetails
            };
        }

        private static string BuildTo(GfedSite site, int hour) {
            var toList = new List<string>();


            if (!string.IsNullOrEmpty(site.PrimaryContactEmail)) {
                toList.Add(site.PrimaryContactEmail);
            }
            if (!string.IsNullOrEmpty(site.EscalationContactEmail) && hour > 0) {
                toList.Add(site.EscalationContactEmail);
            }
            if (hour > 1 && ApplicationConfiguration.IsProd()) {
                toList.Add("frank.kelly@firstsolar.com");
            }

            if (!toList.Any() && (ApplicationConfiguration.IsDev() || ApplicationConfiguration.IsQA())) {
                toList.Add(SwConstants.DevTeamEmail);
            }

            return string.Join("; ", toList);
        }

        public string BuildMessage(DispatchTicket ticket, GfedSite site, bool allowRejection) {
            var redirectService = SimpleInjectorGenericFactory.Instance.GetObject<RedirectService>();
            var accepturl = redirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=ACCEPTED".Fmt(ticket.AccessToken));
            var rejecturl = redirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=REJECTED".Fmt(ticket.AccessToken));
            var arrivedurl = redirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=ARRIVED".Fmt(ticket.AccessToken));
            var resolvedurl = redirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=RESOLVED".Fmt(ticket.AccessToken));
            var accepted = DispatchTicketStatus.ACCEPTED.Equals(ticket.Status) || DispatchTicketStatus.ARRIVED.Equals(ticket.Status);
            var data = new {
                accepted,
                accepturl,
                rejecturl,
                arrivedurl,
                allowRejection,
                resolvedurl,
                fourhourdeadline = ticket.FmtDateTime(ticket.DispatchExpectedDate?.Add(new TimeSpan(0, 4, 0, 0))),
                onsitedeadline = ticket.OnSiteDeadLine(),
                site = site.FacilityName,
                comments = ticket.Comments,
                inverters = BuildInverterInfo(ticket)
            };
            return BuildMessage(data);
        }


    }
}
