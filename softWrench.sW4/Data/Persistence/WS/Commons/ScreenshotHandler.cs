using log4net;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;
using System;
using System.Text;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class ScreenshotHandler {

        protected AttachmentHandler _attachmentHandler;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ScreenshotHandler));

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
                
                // TODO: Is this necessary, this is a repeat of the same process in attachment. 
                Validate(screenshotName, screenshotString);

                var screenshot = new AttachmentDTO() {
                    Title = FileUtils.GetNameFromPath(screenshotName),
                    Path = screenshotName,
                    Data = screenshotString
                };

                _attachmentHandler.AddAttachment(maximoObj, screenshot);
            }
        }

        public static bool Validate(string screenshotPath, string screenshotData) {
            
            var maxSSSizeInBytes = ApplicationConfiguration.MaxScreenshotSize * 1024 * 1024;
            Log.InfoFormat("Screenshot size: {0}", screenshotData.Length);
            if (screenshotData != null && screenshotData.Length > maxSSSizeInBytes) {
                var screenshotLength = screenshotData.Length / 1024 / 1024;
                throw new Exception(String.Format(
                    "Screenshot is too large ({0} MB). Max screenshot size is {1} MB).", screenshotLength, ApplicationConfiguration.MaxAttachmentSize));
            }           

            return true;
        }
    }
}
