﻿using System;
using System.Web.Mvc;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.API;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
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