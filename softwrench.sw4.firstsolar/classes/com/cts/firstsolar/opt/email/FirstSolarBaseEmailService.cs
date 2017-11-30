﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Security.Services;
using cts.commons.portable.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {
    public abstract class FirstSolarBaseEmailService<T> : ISingletonComponent {
        protected readonly IEmailService EmailService;
        protected Template Template;
        protected readonly RedirectService RedirectService;
        protected readonly IConfigurationFacade ConfigurationFacade;

        protected static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutEmailService));

        private readonly string _templatePath;
        protected readonly string HeaderImageUrl;
        protected readonly IApplicationConfiguration AppConfig;

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public IMaximoHibernateDAO MaximoDao { get; set; }

        public FirstSolarBaseEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade) {
            Log.Debug("init Log");
            EmailService = emailService;
            RedirectService = redirectService;
            ConfigurationFacade = configurationFacade;
            AppConfig = appConfig;
            _templatePath = AppDomain.CurrentDomain.BaseDirectory + GetTemplatePath();
            HeaderImageUrl = HandleHeaderImage();
        }

        public async Task<T> SendEmail(T request, WorkPackage package, WorkOrderData workOrderData,
            List<EmailAttachment> attachs = null) {
            if (ValidateSendMail(package.WorkorderId,workOrderData.WorkType)) {
                return await DoSendEmail(request, package, workOrderData, attachs);
            }
            return await Task.FromResult(request);

        }

        public abstract Task<T> DoSendEmail(T request, WorkPackage package, WorkOrderData workOrderData,
            List<EmailAttachment> attachs = null);

        public virtual string HandleEmailRecipient(AttributeHolder data, string attributeName) {
            var stringOrArray = data.GetAttribute(attributeName);
            if (stringOrArray == null) {
                return null;
            }

            if (stringOrArray is string) {
                return stringOrArray as string;
            }
            if (!(stringOrArray is Array) || ((Array)stringOrArray).Length == 0) {
                return null;
            }
            var sendToArray = ((IEnumerable)stringOrArray).Cast<object>().Select(x => x.ToString()).ToArray();
            return string.Join(", ", sendToArray);
        }

        protected Template BuildTemplate(string path) {
            var templateContent = File.ReadAllText(path);
            return Template.Parse(templateContent); // Parses and compiles the template  
        }


        protected Template BuildTemplate() {
            if (Template == null || AppConfig.IsLocal()) {
                Template = BuildTemplate(_templatePath);
            }
            return Template;
        }

        protected abstract string GetTemplatePath();

        private string HandleHeaderImage() {
            //otb image
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var clientKey = AppConfig.GetClientKey();

            if (File.Exists(baseDirectory + "//Content//Customers//" + clientKey + "//images//header-email.jpg")) {
                return "Content/Customers/" + clientKey + "/images/header-email.jpg";
            }
            if (File.Exists(baseDirectory + "//Content//Images//" + clientKey + "//header-email.jpg")) {
                return "Content/Images/" + clientKey + "/header-email.jpg";
            }
            return "Content/Images/header-email.jpg";
        }

        protected string GetHeaderURL() {
            return RedirectService.GetRootUrl() + HeaderImageUrl;
        }

        public abstract string RequestI18N();
        protected abstract EmailData BuildEmailData(T request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null);

        public string FmtDateTime(DateTime? date) {
            return BaseFmtDate(date, "G");
        }

        public string FmtDate(DateTime? date) {
            return BaseFmtDate(date, "MM/dd/yy");
        }

        private static string BaseFmtDate(DateTime? date, string format) {
            if (date == null) {
                return "";
            }
            var culture = new CultureInfo("en-US");
            return date.Value.ToString(format, culture);
        }

        protected string GetFrom() {
            return ConfigurationFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultFromEmailKey);
        }

        protected static DataMap GetWoData(WorkPackage package) {
            var user = SecurityFacade.CurrentUser();
            var woId = package.WorkorderId;
            var dataset = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarWorkPackageDataSet>();
            return AsyncHelper.RunSync(() => dataset.GetWorkorderRelatedData(user, woId));
        }

        protected static string SafePlaceholder(string value) {
            return string.IsNullOrEmpty(value) ? "&nbsp;" : value.Replace("\n", "<br/>");
        }

        private string GetWoWorkType(long woId) {
            var wos = MaximoDao.FindByNativeQuery("select worktype from workorder where workorderid = '{0}'".Fmt(woId));
            return wos.First()["worktype"];
        }

        private bool ValidateSendMail(long woid, string worktype) {
            if (string.IsNullOrEmpty(worktype)) {
                worktype = GetWoWorkType(woid);
            }
            return string.IsNullOrWhiteSpace(worktype) || !"PM".Equals(worktype.ToUpper());
        }
    }

    public class WorkOrderData {

        public string SiteId { get; set; }
        public string WorkType { get; set; }
    }
}
