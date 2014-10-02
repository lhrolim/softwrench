using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class ScreenshotHandler {

        protected AttachmentHandler _attachmentHandler;

        public ScreenshotHandler(AttachmentHandler attachmentHandler) {
            _attachmentHandler = attachmentHandler;
        }

        public void HandleScreenshot(object maximoObj, string screenshotString, string screenshotName, ApplicationMetadata applicationMetadata) {

            if (!String.IsNullOrWhiteSpace(screenshotString) && !String.IsNullOrWhiteSpace(screenshotName)) {

                if (screenshotName.ToLower().EndsWith("rtf")) {
                    var bytes = Convert.FromBase64String(screenshotString);
                    var decodedString = Encoding.UTF8.GetString(bytes);
                    var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);

                    var convertedScreeshot = RTFUtil.ConvertToHTML(compressedScreenshot);

                    bytes = Encoding.UTF8.GetBytes(convertedScreeshot);
                    screenshotString = Convert.ToBase64String(bytes);
                    screenshotName = screenshotName.Substring(0, screenshotName.Length - 3) + "html";
                }

                _attachmentHandler.HandleAttachments(maximoObj, screenshotString, screenshotName, applicationMetadata);
            }
        }
    }
}
