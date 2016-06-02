using System;
using cts.commons.simpleinjector;
using log4net;
using NHibernate.Linq;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Data.PDF;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class PdfEmailReportHandler : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(PdfEmailReportHandler));

        private readonly PdfService _pdfService;
        private readonly IMemoryContextLookuper _lookuper;

        public PdfEmailReportHandler(PdfService pdfService, IMemoryContextLookuper lookuper) {
            _pdfService = pdfService;
            _lookuper = lookuper;
            Log.Debug("init...");
        }

        public AttachmentDTO CreateDetailsAttachment(string detailsHtml, string appTitle, string userId) {
            var appContext = ProcessAppContext();
            var serverPath = AppDomain.CurrentDomain.BaseDirectory;
            var processedHtml = ProcessHtml(detailsHtml, appContext, serverPath);

            // limits title to 20 chars
            var fullTitle = appTitle + " Details";
            var title = fullTitle;
            if (title.Length > 20) {
                title = "";
                appTitle.Split(' ').ForEach(token => title += token.Substring(0, 1).ToUpper());
                title += " Details";
            }
            var hasUserId = !string.IsNullOrEmpty(userId);
            var description = fullTitle + (hasUserId ? " #" + userId : "");

            var path = appTitle + (hasUserId ? "(" + userId + ")" : "") + ".pdf";
            path = path.Replace(' ', '_');

            var dto = new AttachmentDTO() {
                Path = path,
                BinaryData = _pdfService.HtmlToPdf(processedHtml, fullTitle),
                Title = title,
                Description = description
            };

            //Comment to generate files for testing
            //System.IO.File.WriteAllBytes(@"C:\test.pdf", dto.BinaryData);
            //System.IO.File.WriteAllText(@"C:\test.html", processedHtml);

            return dto;
        }

        private string ProcessAppContext() {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            var appContext = fullContext.Context;
            var seccondBarIndex = appContext.IndexOf("/", 1, StringComparison.Ordinal);
            if (seccondBarIndex > 0) {
                appContext = appContext.Substring(0, appContext.IndexOf("/", 1, StringComparison.Ordinal));
            }

            if (Log.IsDebugEnabled) {
                Log.DebugFormat("Details Commlog original App context: " + appContext);
            }
            return appContext;
        }

        internal static string ProcessHtml(string detailsHtml, string appContext, string serverPath) {


            if (!appContext.StartsWith("/")) {
                appContext = "/" + appContext;
            }

            if (!appContext.EndsWith("/")) {
                appContext = appContext + "/";
            }

            // appcontext not found on html, probably appcontext is "/"
            // or appcontext found as the content folder also probably appcontext is "/"
            if (detailsHtml.IndexOf("<link href=\"" + appContext, StringComparison.Ordinal) < 0 || "/Content".Equals(appContext)) {
                appContext = "/";
            }
            if (!serverPath.EndsWith("/")) {
                serverPath = serverPath + "/";
            }


            // make css paths absolute
            var processedHtml = detailsHtml.Replace("<link href=\"" + appContext, "<link href=\"" + serverPath);
            // make images paths absolute
            processedHtml = processedHtml.Replace(" src=\"" + appContext, " src=\"" + serverPath);


            Log.DebugFormat("Details Commlog App context: " + appContext);
            Log.DebugFormat("Details Commlog Server path: " + serverPath);
            Log.DebugFormat(processedHtml);
            return processedHtml;
        }
    }
}
