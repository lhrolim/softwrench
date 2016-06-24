using System;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softwrench.sw4.offlineserver.controller {

    [Authorize]
    public class OfflineAttachmentController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(OfflineAttachmentController));

        private readonly AttachmentHandler _attachmentHandler;

        public OfflineAttachmentController(AttachmentHandler attachmentHandler) {
            _attachmentHandler = attachmentHandler;
            Log.Debug("init...");
        }

        [HttpGet]
        public async Task<FileResult> DownloadBase64(string id) {
            Log.InfoFormat("Downloading offline attachment {0}", id);
            var fileTuple = await _attachmentHandler.DownloadViaHttpByIdReturningMime(id);
            if (fileTuple == null) {
                return null;
            }
            return new FileResult() {
                Content = Convert.ToBase64String(fileTuple.Item1),
                MimeType = fileTuple.Item2
            };
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