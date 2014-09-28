﻿using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using System.IO;

namespace softWrench.sW4.Data.Persistence.WS.Mea {
    class MeaAttachmentHandler : AttachmentHandler {


        /// <summary>
        /// On Mea there´s no way to save the attachment directly from a ws. Hence, we need to save it to the filesystem.
        /// 
        /// The path used, will then be used on the ws to point to the right location of the file
        /// 
        /// </summary>
        /// <param name="attachmentData"></param>
        /// <param name="base64Delegate"></param>
        /// <param name="docLink"></param>
        /// <param name="attachmentPath"></param>
        ///
        //TODO: code to delete wrongly created files, if ws fails
        protected override void HandleAttachmentDataAndPath(string attachmentData,
            object docLink, string attachmentPath) {
            
            var pathVariable = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksPath);
            var pathToSave = pathVariable + attachmentPath;
            //TODO: Handle exceptions
            File.WriteAllBytes(pathToSave, FileUtils.ToByteArrayFromHtmlString(attachmentData));
            WsUtil.SetValue(docLink, "URLNAME", pathToSave);
            WsUtil.SetValue(docLink, "NEWURLNAME", pathToSave);
        }

        protected override int GetMaximoLength() {
            return 8;
        }
    }
}
