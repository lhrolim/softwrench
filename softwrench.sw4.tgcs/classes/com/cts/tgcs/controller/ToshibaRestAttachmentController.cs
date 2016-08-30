using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using cts.commons.web.Controller;
using softWrench.sW4.Data.Persistence.WS.Rest;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.controller {

    [Authorize]
    public class ToshibaRestAttachmentController : FileDownloadController {

        [HttpGet]
        public FileContentResult Download(string doclinksid, string weburl, string document) {
            var baseUrl = new Uri(MaximoRestUtils.GetRestBaseUrl("ism"));
            // setting host as defined in properties: weburl.Host might be an unreachable ip instead of a domain
            var downloadUrl = (new UriBuilder(weburl) { Host = baseUrl.Host}).Uri;
            var downloadHeaders = MaximoRestUtils.GetMaximoHeaders("ism");

            using (var client = new WebClient()) {
                foreach (var name in downloadHeaders.Keys.Where(k => k != "Content-Type")) {
                    client.Headers.Add(name, downloadHeaders[name]);
                }
                var fileContent = client.DownloadData(downloadUrl);
                return new FileContentResult(fileContent, System.Net.Mime.MediaTypeNames.Application.Octet) {
                    FileDownloadName = document
                };
            }
        }

    }
}