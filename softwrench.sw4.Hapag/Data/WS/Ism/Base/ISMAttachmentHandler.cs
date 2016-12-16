using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.WS.Ism.Base {
    class ISMAttachmentHandler {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ISMAttachmentHandler));


        private static List<Attachment> GetAttachments(CrudOperationData jsonObject) {
            var attachmentString = jsonObject.GetAttribute("newattachment") as string;
            var attachmentPath = jsonObject.GetAttribute("newattachment_path") as string;
            var attachmentList = new List<Attachment>();
            if (attachmentString != null && AttachmentHandler.Validate(attachmentPath, attachmentString)) {
                var b64PartOnly = FileUtils.GetB64PartOnly(attachmentString);
                AttachmentHandler.ValidateNotEmpty(b64PartOnly);

                Log.DebugFormat("adding attachment. Name: {0}, Content:{1}", attachmentPath, attachmentString);

                attachmentList.Add(new Attachment {
                    AttachmentName = attachmentPath,
                    Attachment1 = b64PartOnly
                });
            }
            return attachmentList;
        }

        private static IEnumerable<Attachment> DoHandleAttachment(CrudOperationData jsonObject, object webServiceObject) {
            var attachmentList = GetAttachments(jsonObject);

            var screenshotString = jsonObject.GetAttribute("newscreenshot") as string;
            var screenshotName = jsonObject.GetAttribute("newscreenshot_path") as string;
            HandleScreenshots(screenshotString, screenshotName, attachmentList);
            
            return attachmentList;
        }

        private static void AddWorkLogEnttry(object webServiceObject) {
            var activity = new Activity {
                ActionLogSummary = "New Attachment",
                type = "WorkLog",
                UserID = SecurityFacade.CurrentUser().MaximoPersonId,
                LogDateTimeSpecified = true,
                LogDateTime = DateTime.Now.FromServerToRightKind(),
                ActivityType = "CLIENTNOTE"
            };
            ((ServiceIncident)webServiceObject).Activity = ArrayUtil.Push(((ServiceIncident)webServiceObject).Activity,
                activity);
        }
        
        private static void HandleScreenshots(string screenshotString, string screenshotName, List<Attachment> attachmentList) {

            if (String.IsNullOrWhiteSpace(screenshotString) || String.IsNullOrWhiteSpace(screenshotName)) {
                Log.DebugFormat("screnshot not found. name:{0}, content{1}", screenshotName, screenshotString);
                return;
            }
            if (screenshotName.ToLower().EndsWith("rtf")) {
                //converting rtf to doc to handle IE9 scenario
                screenshotString = ConvertRtfToDoc(screenshotString, ref screenshotName);
            }

            ScreenshotHandler.Validate(screenshotName, screenshotString);

            attachmentList.Add(new Attachment {
                AttachmentName = screenshotName,
                Attachment1 = screenshotString
            });
        }

        private static string ConvertRtfToDoc(string screenshotString, ref string screenshotName) {
            var bytes = Convert.FromBase64String(screenshotString);
            var decodedString = Encoding.UTF8.GetString(bytes);
            var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);
            bytes = Encoding.UTF8.GetBytes(compressedScreenshot);
            screenshotString = Convert.ToBase64String(bytes);
            screenshotName = screenshotName.Substring(0, screenshotName.Length - 3) + "doc";
            return screenshotString;
        }

        public static void HandleAttachmentsForCreation(CrudOperationData entity, ServiceIncident maximoTicket) {
            maximoTicket.Attachment = DoHandleAttachment(entity, maximoTicket).ToArray();
        }

        public static void HandleAttachmentsForCreation(CrudOperationData entity, ChangeRequest maximoTicket) {
            maximoTicket.Attachment = DoHandleAttachment(entity, maximoTicket).ToArray();
        }

        public static void HandleAttachmentsForUpdate(CrudOperationData entity, object maximoTicket) {
            var maximoAttachments = entity.GetRelationship("attachment_");
            var attachmentList = new List<Attachment>();
            //on ie9 the screenshot data come on the root entity. Look at ApplicationController#PopulateInputsInJson and screenshotService#handleRichTextBoxSubmit
            var screenshotString = entity.GetAttribute("newscreenshot") as string;
            var screenshotName = entity.GetAttribute("newscreenshot_path") as string;
            if (screenshotString != null && screenshotName != null) {
                Log.DebugFormat("handling screenshot for update");
                HandleScreenshots(screenshotString, screenshotName, attachmentList);
            }
            foreach (var maximoAttachment in (IEnumerable<CrudOperationData>)maximoAttachments) {
                var attachmentsAdded = DoHandleAttachment(maximoAttachment, maximoTicket);
                attachmentList.AddRange(attachmentsAdded);
            }

            if (attachmentList.Any() && maximoTicket is ServiceIncident) {
                AddWorkLogEnttry(maximoTicket);
            }

            if (maximoTicket is ServiceIncident) {
                ((ServiceIncident)maximoTicket).Attachment = attachmentList.ToArray();
            } else if (maximoTicket is ChangeRequest) {
                ((ChangeRequest)maximoTicket).Attachment = attachmentList.ToArray();
            }
        }

    }
}
