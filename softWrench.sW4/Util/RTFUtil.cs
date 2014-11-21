using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using XDesigner.RTF;

namespace softWrench.sW4.Util {
    class RTFUtil {

        private static readonly ILog Log = LogManager.GetLogger(typeof(RTFUtil));

        public static string ConvertRtfToHtml(string screenshotString, ref string screenshotName) {
            var before = Stopwatch.StartNew();
            var bytes = Convert.FromBase64String(screenshotString);
            var decodedString = Encoding.UTF8.GetString(bytes);
            var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);
            var convertedScreeshot = RTFUtil.ConvertToHTML(compressedScreenshot);

            bytes = Encoding.UTF8.GetBytes(convertedScreeshot);
            screenshotString = Convert.ToBase64String(bytes);
            screenshotName = screenshotName.Substring(0, screenshotName.Length - 3) + "html";
            var message = LoggingUtil.BaseDurationMessageFormat(before, "PERFORMANCE - Done RTL HTML conversion.");
            LoggingUtil.PerformanceLog.Debug(message);
            Log.Debug(message);
            return screenshotString;
        }


        public static string ConvertToHTML(string rtf) {
            var before = Stopwatch.StartNew();
            var doc = new RTFDomDocument();
            doc.LoadRTFText(rtf);

            var htmlDoc = new StringBuilder();

            htmlDoc.Append("<html><head></head><body>");
            htmlDoc.Append(ConvertElementsToHTML(doc.Elements));
            htmlDoc.Append("</body></html>");
            return htmlDoc.ToString();
        }

        private static string ConvertElementsToHTML(RTFDomElementList elements) {
            var strElements = new StringBuilder();
            foreach (var element in elements) {
                strElements.Append(ConvertElementToHTML((RTFDomElement)element));
            }
            return strElements.ToString();
        }


        private static string ConvertElementToHTML(RTFDomElement element) {
            var strElement = new StringBuilder();

            if (element is RTFDomParagraph) {
                strElement.Append("<p>");
                strElement.Append(ConvertElementsToHTML(((RTFDomParagraph)element).Elements));
                strElement.Append("</p>");
            } else if (element is RTFDomText) {
                strElement.Append("<label>");
                strElement.Append(((RTFDomText)element).Text);
                strElement.Append("</label>");
            } else if (element is RTFDomImage) {
                strElement.Append("<img src=\"data:image/png;base64,");
                strElement.Append(((RTFDomImage)element).Base64Data);
                strElement.Append("\"></img>");
            }
            return strElement.ToString();
        }
    }
}
