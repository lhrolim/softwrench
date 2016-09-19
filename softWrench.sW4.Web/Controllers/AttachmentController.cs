using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using cts.commons.web.Controller;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softWrench.sW4.Web.Controllers {

    [System.Web.Mvc.Authorize]
    public class AttachmentController : FileDownloadController {

        private readonly AttachmentHandler _attachmentHandler;

        public AttachmentController(AttachmentHandler attachmentHandler) {
            _attachmentHandler = attachmentHandler;
        }

        public async Task<FileContentResult> Download(string id, string mode, AttachmentRequest request) {

            Tuple<byte[], string> fileTuple;

            switch (mode) {
                case "http":
                    fileTuple = await _attachmentHandler.DownloadViaHttpById(id);
                    break;
                case "ws":
                    fileTuple = _attachmentHandler.DownloadViaParentWS(id, request.ParentId, request.ParentApplication, request.ParentSchemaId);
                    break;
                default:
                    throw new NotImplementedException(string.Format("{0} mode not implemented. Please use 'http' or 'ws'", mode));
            }

            if (fileTuple == null) return null;
            var result = new FileContentResult(fileTuple.Item1, System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = fileTuple.Item2
            };
            return result;
        }


        public async Task<string> DownloadBase64(string id, string mode) {
            var fileTuple = await _attachmentHandler.DownloadViaHttpById(id);
            return fileTuple == null ? null : Convert.ToBase64String(fileTuple.Item1);

        }

        [HttpGet]
        public async Task<string> DownloadUrl(string id) {
            var file = await _attachmentHandler.AttachmentDao.ById(id);
            var docinfoURL = (string)file.GetAttribute("urlname");
            return await _attachmentHandler.GetFileUrl(docinfoURL);
        }
    }
}