using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Util;
using softWrench.sW4.Security.Services;
using System.Web.Http;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications.DataSet;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Data.API;

namespace softWrench.sW4.Web.Controllers {

    public class AttachmentController : FileDownloadController {

        private readonly AttachmentHandler _attachmentHandler = new AttachmentHandler();

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
            } else {
                return null;
            }
        }
    }
}