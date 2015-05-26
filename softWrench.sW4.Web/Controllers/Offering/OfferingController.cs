using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers.Offering {
    public class OfferingController : Controller {
        //
        // GET: /Faq/

        private readonly I18NResolver _i18NResolver;

        public OfferingController(I18NResolver i18NResolver) {
            this._i18NResolver = i18NResolver;
        }


        public FileStreamResult Pdf(string optionSelected) {
            var translatedName = _i18NResolver.I18NValue("offering.hasinternet." + optionSelected, optionSelected);
            var workStream = new MemoryStream();
            var path = MetadataProvider.GlobalProperty("offeringpdfpath", true);
            if (!Directory.Exists(path)) {
                return null;
            }
            var fileEntries = Directory.GetFiles(path);
            var pdf = fileEntries.FirstOrDefault(f => f.EndsWith(translatedName + ".pdf",StringComparison.OrdinalIgnoreCase));
            if (pdf==null) {
                return null;
            }
            var byteInfo = System.IO.File.ReadAllBytes(pdf);
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;
            Response.AppendHeader("content-disposition", "inline; filename=file.pdf");
            return new FileStreamResult(workStream, "application/pdf");
        }



    }
}
