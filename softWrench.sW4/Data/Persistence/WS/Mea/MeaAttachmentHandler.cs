using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using System.IO;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softWrench.sW4.Data.Persistence.WS.Mea {
    public class MeaAttachmentHandler : AttachmentHandler {
        public MeaAttachmentHandler(MaximoHibernateDAO maxDAO, DataSetProvider dataSetProvider, AttachmentDao dao, IConfigurationFacade facade) : base(maxDAO, dataSetProvider, dao, facade)
        {
        }

        /// <summary>
        /// On Mea there´s no way to save the attachment directly from a ws. Hence, we need to save it to the filesystem.
        /// 
        /// The path used, will then be used on the ws to point to the right location of the file
        /// 
        /// </summary>
        /// <param name="attachmentData"></param>
        /// <param name="docLink"></param>
        /// <param name="attachmentPath"></param>
        /// <param name="binaryData"></param>
        ///
        //TODO: code to delete wrongly created files, if ws fails
        protected override void HandleAttachmentDataAndPath(string attachmentData,
            object docLink, string attachmentPath, byte[] binaryData) {
            
            var pathVariable = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksPath);
            var pathToSave = pathVariable + attachmentPath;
            //TODO: Handle exceptions

            var bytes = binaryData ?? FileUtils.ToByteArrayFromHtmlString(attachmentData);
            File.WriteAllBytes(pathToSave, bytes);
            WsUtil.SetValue(docLink, "URLNAME", pathToSave);
            WsUtil.SetValue(docLink, "NEWURLNAME", pathToSave);
        }

        protected override int GetMaximoLength() {
            return 8;
        }
    }
}
