using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services {
    public class DispatchEmailService : ISingletonComponent {

        private const string TemplatePath = "//Content//Customers//firstsolardispatch//htmls//templates//dispatch.html";
        private Template _template;

        [Import]
        public ConfigurationFacade ConfigurationFacade { get; set; }

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public IEmailService EmailService { get; set; }

        [Import]
        public RedirectService RedirectService { get; set; }

        public void SendEmail(DispatchTicket ticket, GfedSite site) {
            var subject = "[{0}] A new Ticket has been created".Fmt(ticket.Id);
            var to = BuildTo(site);
            if (string.IsNullOrEmpty(to)) {
                return;
            }
            var from = GetFrom();
            var msg = BuildMessage(ticket, site);
            var emailData = new EmailData(from, to, subject, msg);
            EmailService.SendEmailAsync(emailData);
        }

        private string GetFrom() {
            return ConfigurationFacade.Lookup<string>(FirstSolarDispatchConfigurations.DefaultFromEmailKey);
        }

        public string BuildMessage(DispatchTicket ticket, GfedSite site) {
            BuildTemplate();

            var accepturl = RedirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=ACCEPTED".Fmt(ticket.AccessToken));
            var rejecturl = RedirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=REJECTED".Fmt(ticket.AccessToken));
            var arrivedurl = RedirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=ARRIVED".Fmt(ticket.AccessToken));
            var resolvedurl = RedirectService.GetActionUrl("DispatchEmail", "ChangeStatus", "token={0}&status=RESOLVED".Fmt(ticket.AccessToken));

            var msg = _template.Render(Hash.FromAnonymousObject(new {
                accepturl,
                rejecturl,
                arrivedurl,
                resolvedurl,
                id = ticket.Id,
                site = site.FacilityName,
                reportedby = ticket.ReportedBy?.FullName
            }));
            return msg;
        }

        private static string BuildTo(GfedSite site) {
            var toList = new List<string>();
            if (!string.IsNullOrEmpty(site.PrimaryContactEmail)) {
                toList.Add(site.PrimaryContactEmail);
            }
            if (!string.IsNullOrEmpty(site.EscalationContactEmail)) {
                toList.Add(site.EscalationContactEmail);
            }
            return string.Join("; ", toList);
        }

        private void BuildTemplate() {
            if (_template != null && !ApplicationConfiguration.IsLocal()) return;
            var templateContent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + TemplatePath);
            _template = Template.Parse(templateContent); // Parses and compiles the template  
        }
    }
}
