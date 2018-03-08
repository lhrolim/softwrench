using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Email;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public abstract class BaseDispatchEmailService : BaseDispatchGenericEmailService {

        [Import]
        public IAuditManager AuditManager { get; set; }

        protected List<object> BuildInverterInfo(DispatchTicket ticket) {
            return ticket.Inverters?.Select(BuildInverterInfo).ToList() ?? new List<object>();
        }

        public virtual async Task SendEmail(DispatchTicket ticket, bool force = false) {
            var hour = ticket.CalculateHours();
            if (hour == ticket.CalculateLastSentHours() && !force) {
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
            var emailData = new EmailData(@from, to, subject, msg) { BCc = GetBcc() };

            var emailService = SimpleInjectorGenericFactory.Instance.GetObject<EmailService>();
            emailService.SendEmailAsync(emailData, async success => {
                ticket.LastSent = DateTime.Now;
                if (DispatchTicketStatus.SCHEDULED.Equals(ticket.Status)) {
                    ticket.Status = DispatchTicketStatus.DISPATCHED;
                    AuditManager.CreateAuditEntry("dispatcher_status", "_dispatchticket", ticket.Id.ToString(), ticket.Id.ToString(), ticket.Status.LabelName(), DateTime.Now);
                }
                AfterSend(emailData);
                await dao.SaveAsync(ticket);

            });
        }

        protected virtual void AfterSend(EmailData emailData) {
            //NOOP
        }


        private static object BuildInverterInfo(Inverter inverter) {
            var sb = new StringBuilder();
            var maximoDao = SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>();
            var assetDescription = maximoDao.FindSingleByNativeQuery<string>("select description from asset where assetnum = ? and siteid = ?", inverter.AssetNum, inverter.Siteid);
            sb.AppendFormat("({0}) {1}", inverter.AssetNum, inverter.AssetDescription).Append(" - Class ").Append(inverter.FailureClass);
            if (!string.IsNullOrEmpty(inverter.ErrorCodes)) {
                sb.Append(" - ").Append(inverter.ErrorCodes);
            }
            return new {
                description = sb.ToString(),
                details = inverter.FailureDetails
            };
        }

        public virtual string BuildTo(GfedSite site, int hour) {
            if (!ApplicationConfiguration.IsProd()) {
                return SwConstants.DevTeamEmail;
            }
            return null;
        }

        public virtual string BuildMessage(DispatchTicket ticket, GfedSite site, bool allowRejection) {
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
