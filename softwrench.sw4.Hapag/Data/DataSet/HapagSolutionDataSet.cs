using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagSolutionDataSet : BaseApplicationDataSet {
       
        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dataResponse = base.GetApplicationDetail(application, user, request);
            var lang = !string.IsNullOrEmpty(request.Lang) ? request.Lang : "E";
            dataResponse.ResultObject.Attributes["faqid"] = request.Faqid;
            BuildAttachments(request.Faqid, dataResponse);


            return dataResponse;
        }


        internal void BuildAttachments(string folderName, IApplicationResponse dataResponse) {
            var resultObject = ((ApplicationDetailResult)dataResponse).ResultObject;
            resultObject.Attributes["attachment_"] = null;
            var path = MetadataProvider.GlobalProperty("faqfspath", true) + folderName;
            if (!Directory.Exists(path)) {
                resultObject.SetAttribute("hasattachment", false);
                return;
            }
            var fileEntries = Directory.GetFiles(path);
            var attachments = new List<Dictionary<string, object>>();
            if (fileEntries.Length == 0) {
                resultObject.SetAttribute("hasattachment", false);
                return;
            }

            var pdfs =fileEntries.Where(f => f.EndsWith("pdf"));

            var enumerable = pdfs as string[] ?? pdfs.ToArray();
            if (!enumerable.Any() || enumerable.Count() > 1) {
                resultObject.SetAttribute("hasattachment", false);
                resultObject.SetAttribute("hasinvalidattachment", true);
                return;
            }

            resultObject.SetAttribute("hasattachment", true);
        }

        public override string ApplicationName() {
            return "solution";
        }
    }
}
