using System;
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

        public FileContentResult Download(string id, string mode, AttachmentRequest request) {

            Tuple<Byte[], string> fileTuple;

            if (mode == "http") {
                fileTuple = _attachmentHandler.DownloadViaHttpById(id);
            } else if (mode == "ws") {
                fileTuple = _attachmentHandler.DownloadViaParentWS(id, request.ParentId, request.ParentApplication, request.ParentSchemaId);
            } else {
                throw new NotImplementedException(String.Format("{0} mode not implemented. Please use 'http' or 'ws'", mode));
            }

            if (fileTuple != null) {
                var result = new FileContentResult(fileTuple.Item1, System.Net.Mime.MediaTypeNames.Application.Octet) {
                    FileDownloadName = fileTuple.Item2
                };
                return result;
            }
            return null;
        }


        public string DownloadBase64(string id, string mode) {
            var fileTuple = _attachmentHandler.DownloadViaHttpById(id);
            return fileTuple == null ? null : Convert.ToBase64String(fileTuple.Item1);

        }

        [HttpGet]
        public string DownloadUrl(string id) {
            var file = _attachmentHandler.AttachmentDao.ById(id);
            var docinfoURL = (string)file.GetAttribute("urlname");
            return _attachmentHandler.GetFileUrl(docinfoURL);
        }
    }
}