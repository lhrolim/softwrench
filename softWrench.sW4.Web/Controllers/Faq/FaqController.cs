using System.Web.Mvc;
using System.IO;

namespace softWrench.sW4.Web.Controllers.Faq {
    public class FaqController : Controller {
        //
        // GET: /Faq/

        public FileResult Download(string id) {
            string fileName = null;
            var fileBytes = GetFile(id, ref fileName);
            var result = new FileContentResult(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = fileName
            };
            return result;
        }

        private byte[] GetFile(string s, ref string fileName) {
            var fs = System.IO.File.OpenRead(s);
            var file = new FileInfo(s);
            fileName = file.Name;
            var data = new byte[fs.Length];
            var br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new IOException(s);
            return data;
        }
    }
}
