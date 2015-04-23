using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagSolutionDataSet : MaximoApplicationDataSet {
        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dataResponse = base.GetApplicationDetail(application, user, request);
            var lang = !string.IsNullOrEmpty(request.Lang) ? request.Lang : "E";
            BuildAttachments(request.Faqid, dataResponse);
            return dataResponse;
        }


        internal void BuildAttachments(string folderName, IApplicationResponse dataResponse) {
            ((ApplicationDetailResult)dataResponse).ResultObject.Attributes["attachment_"] = null;
            var path = MetadataProvider.GlobalProperty("faqfspath", true) + folderName;
            if (!Directory.Exists(path)) {
                return;
            }
            var fileEntries = Directory.GetFiles(path);

            var attachments = new List<Dictionary<string, object>>();
            foreach (var fileEntry in fileEntries) {
                var file = new FileInfo(fileEntry);
                var attachment = new Dictionary<string, object> { { "#path", fileEntry }, { "document", file.Name } };
                attachments.Add(attachment);
            }
            ((ApplicationDetailResult)dataResponse).ResultObject.Attributes["attachment_"] = attachments;
        }

        public override string ClientFilter()
        {
            return "hapag";
        }

        public override string ApplicationName() {
            return "solution";
        }
    }
}
