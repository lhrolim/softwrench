using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.PDF {
    public class PdfService : ISingletonComponent {

        public byte[] HtmlToPdf(string htmlContent, string title) {
            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
            htmlToPdf.CustomWkHtmlArgs = "--title \"" + title + "\" --page-size Letter --viewport-size \"1280x1660\" --margin-left 7.5";
            return htmlToPdf.GeneratePdf(htmlContent);
        }
    }
}
