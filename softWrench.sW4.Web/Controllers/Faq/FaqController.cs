using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.IO;
using softWrench.sW4.Metadata;

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

        public FileStreamResult Pdf(string faqId) {
            var workStream = new MemoryStream();
            var path = MetadataProvider.GlobalProperty("faqfspath", true) + faqId;
            if (!Directory.Exists(path)) {
                return null;
            }
            var fileEntries = Directory.GetFiles(path);
            var pdfs =fileEntries.Where(f => f.EndsWith("pdf"));
            var enumerable = pdfs as string[] ?? pdfs.ToArray();
            if (!enumerable.Any() || enumerable.Count()> 1) {
                return null;
            }
            var byteInfo = System.IO.File.ReadAllBytes(enumerable.First());
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;
            Response.AppendHeader("content-disposition", "inline; filename=file.pdf");
            return new FileStreamResult(workStream, "application/pdf");
        }



    }
}
