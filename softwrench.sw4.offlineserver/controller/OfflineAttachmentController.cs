using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softwrench.sw4.offlineserver.controller {

    [Authorize]
    public class OfflineAttachmentController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(OfflineAttachmentController));

        private readonly AttachmentHandler _attachmentHandler;

        private IConfigurationFacade _facade;

        public OfflineAttachmentController(AttachmentHandler attachmentHandler, IConfigurationFacade facade) {
            _attachmentHandler = attachmentHandler;
            _facade = facade;
            Log.Debug("init...");
        }

        [HttpGet]
        public async Task<FileResult> DownloadBase64(string id) {
            if (id == null || id == "undefined") {
                return null;
            }

            if (!await _facade.LookupAsync<bool>(OfflineConstants.EnableOfflineAttachments)) {
                Log.InfoFormat("Ignoring offline attachment {0} because it is turned off", id);
                return null;
            }

            Log.InfoFormat("Downloading offline attachment {0}", id);
            var fileTuple = await _attachmentHandler.DownloadViaHttpByIdReturningMime(id);
            if (fileTuple == null) {
                return null;
            }
            var attachSt = Convert.ToBase64String(fileTuple.Item1);
            Log.InfoFormat("Offline attachment {0} downloaded,with size {1}", id, Size(fileTuple.Item1.Length));
            return new FileResult {
                Content = attachSt,
                MimeType = fileTuple.Item2
            };
        }


        public string Size(int len) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        public class FileResult {
            public string Content {
                get; set;
            }
            public string MimeType {
                get; set;
            }
        }


    }
}