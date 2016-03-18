using log4net;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using r = softWrench.sW4.Util.ReflectionUtil;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using System.Text;
using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.WS.Internal;


namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class AttachmentHandler {

        private static readonly ILog Log = LogManager.GetLogger(typeof(AttachmentHandler));

        private readonly MaxPropValueDao _maxPropValueDao = new MaxPropValueDao();
        private readonly DataSetProvider _dataSetProvider = DataSetProvider.GetInstance();
        private static readonly AttachmentDao AttachmentDao = new AttachmentDao();

        /// <summary>
        /// url specifying where the attachments could be downloaded from maximo in the http mode
        /// </summary>
        private string _baseMaximoURL;
        /// <summary>
        /// Path where the files are stored in the maximo´s server fs, must be removed from the url path
        /// </summary>
        private string _baseMaximoPath;


        //        public delegate byte[] Base64Delegate(string attachmentData);

        /// <summary>
        /// Used for parsing the base64 string from the html input="file" element
        /// </summary>
        //        public static Base64Delegate InputFile = delegate(string attachmentAsString) {
        //            var indexOf = attachmentAsString.IndexOf(',');
        //            var base64String = attachmentAsString.Substring(indexOf + 1);
        //            return System.Convert.FromBase64String(base64String);
        //        };

        public void HandleAttachmentAndScreenshot(MaximoOperationExecutionContext maximoTemplateData) {
            // Used to get user's current local time for screenshot
            var user = SecurityFacade.CurrentUser();
            // Entity structure contain all the attachment data
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var maximoObj = maximoTemplateData.IntegrationObject;
            // Attachment from a newly created ticket or work order
            var data = entity.GetUnMappedAttribute("newattachment");
            var path = entity.GetUnMappedAttribute("newattachment_path");
            if (!string.IsNullOrWhiteSpace(data) && !string.IsNullOrWhiteSpace(path)) {
                var attachmentParam = new AttachmentDTO() {
                    Data = data,
                    Path = path
                };
                AddAttachment(maximoObj, attachmentParam);
            }
            // Screenshot
            var screenshot = entity.GetUnMappedAttribute("newscreenshot");
            if (!string.IsNullOrWhiteSpace(screenshot)) {
                var screenshotParam = new AttachmentDTO() {
                    Data = screenshot,
                    Path = "screen" + DateTime.Now.ToUserTimezone(user).ToString("yyyyMMdd") + ".png"
                };
                AddAttachment(maximoObj, screenshotParam);
            }
            // Attachments that are found in the composition list details
            var attachments = entity.GetRelationship("attachment");
            if (attachments != null) {
                // this will only filter new attachments
                foreach (var attachment in ((IEnumerable<CrudOperationData>)attachments).Where(a => a.Id == null)) {
                    var docinfo = (CrudOperationData)attachment.GetRelationship("docinfo");
                    var title = attachment.GetAttribute("document").ToString();
                    var desc = docinfo != null && docinfo.Fields["description"] != null ? docinfo.Fields["description"].ToString() : "";
                    var content = new AttachmentDTO() {
                        Title = title,
                        Data = attachment.GetUnMappedAttribute("newattachment"),
                        Path = attachment.GetUnMappedAttribute("newattachment_path"),
                        Description = desc
                    };
                    if (content.Data != null) {
                        AddAttachment(maximoObj, content);
                    }
                }
            }
        }

        /// <summary>
        /// Add attachment to the MIF object structure
        /// </summary>
        /// <param name="maximoObj">maximo integratio object</param>
        /// <param name="attachment">attachment object</param>
        public void AddAttachment(object maximoObj, AttachmentDTO attachment) {
            var user = SecurityFacade.CurrentUser();
            // Exit function - do not add attachment
            if (string.IsNullOrEmpty(attachment.Data)) {
                return;
            }
            // Check if file was rich text file - needed to convert it to word document.
            if (attachment.Path.ToLower().EndsWith("rtf")) {
                var bytes = Convert.FromBase64String(attachment.Data);
                var decodedString = Encoding.UTF8.GetString(bytes);
                var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);
                bytes = Encoding.UTF8.GetBytes(compressedScreenshot);
                attachment.Data = Convert.ToBase64String(bytes);
                attachment.Path = attachment.Path.Substring(0, attachment.Path.Length - 3) + "doc";
            }

            // Exit function - if attachment size exceed specification
            if (!Validate(attachment.Path, attachment.Data)) {
                return;
            }
            var docLink = ReflectionUtil.InstantiateSingleElementFromArray(maximoObj, "DOCLINKS");
            w.SetValue(docLink, "ADDINFO", true);
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEBY", user.Login, "reportedby");
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEBY", user.Login, "reportedby");
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "SITEID", user.SiteId);
            w.CopyFromRootEntity(maximoObj, docLink, "ORGID", user.OrgId);
            w.SetValue(docLink, "URLTYPE", "FILE");
            w.SetValue(docLink, "URLNAME", attachment.Path);
            w.SetValue(docLink, "UPLOAD", true);
            w.SetValue(docLink, "DOCTYPE", "Attachments");
            w.SetValue(docLink, "DOCUMENT", attachment.Title ?? FileUtils.GetNameFromPath(attachment.Path, GetMaximoLength()));
            w.SetValue(docLink, "DESCRIPTION", attachment.Description ?? string.Empty);

            HandleAttachmentDataAndPath(attachment.Data, docLink, attachment.Path);
        }

        private void CommonCode(object maximoObj, object docLink, InMemoryUser user, AttachmentDTO attachment) {
            w.SetValue(docLink, "ADDINFO", true);
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEBY", user.Login, "reportedby");
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEBY", user.Login, "reportedby");
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "SITEID", user.SiteId);
            w.CopyFromRootEntity(maximoObj, docLink, "ORGID", user.OrgId);
            w.SetValue(docLink, "URLTYPE", "FILE");
            w.SetValue(docLink, "URLNAME", attachment.Path);
            Validate(attachment.Path, attachment.Data);

            w.SetValue(docLink, "UPLOAD", true);
            w.SetValue(docLink, "DOCTYPE", "Attachments");
            //w.SetValue(docLink, "DOCUMENT", FileUtils.GetNameFromPath(attachmentPath, GetMaximoLength()));
            w.SetValue(docLink, "DOCUMENT", attachment.Title ?? FileUtils.GetNameFromPath(attachment.Path, GetMaximoLength()));
            w.SetValue(docLink, "DESCRIPTION", attachment.Description ?? String.Empty);
        }

        public static bool Validate(string attachmentPath, string attachmentData) {
            var allowedFiles = ApplicationConfiguration.AllowedFilesExtensions;

            if (attachmentPath != null && attachmentPath.IndexOf('.') != -1) {
                var extension = attachmentPath.Substring(attachmentPath.LastIndexOf('.') + 1).ToLower();
                if (!allowedFiles.Any(s => s.Equals(extension, StringComparison.OrdinalIgnoreCase))) {
                    throw new Exception(string.Format("Invalid Attachment extension. Accepted extensions are: {0}.", string.Join(",", allowedFiles)));
                }
            }

            var maxAttSizeInBytes = ApplicationConfiguration.MaxAttachmentSize * 1024 * 1024;
            Log.InfoFormat("Attachment size: {0}", attachmentData.Length);
            if (attachmentData != null && attachmentData.Length > maxAttSizeInBytes) {
                var attachmentLength = attachmentData.Length / 1024 / 1024;
                throw new Exception(String.Format(
                    "Attachment is too large ({0} MB). Max attachment size is {1} MB.", attachmentLength, ApplicationConfiguration.MaxAttachmentSize));
            }

            return true;
        }

        public static void ValidateNotEmpty(string b64PartOnly) {
            if (b64PartOnly.Equals("data:")) {
                throw InvalidAttachmentException.BlankFileNotAllowed();
            }
        }

        protected virtual void HandleAttachmentDataAndPath(string attachmentData, object docLink, string attachmentPath) {
            w.SetValue(docLink, "DOCUMENTDATA", FileUtils.ToByteArrayFromHtmlString(attachmentData));
        }

        protected virtual int GetMaximoLength() {
            return 100;
        }

        [NotNull]
        public static string BuildParsedURLName(IDictionary<string, object> attachmentDataMap) {
            var docInfoURL = "";
            if (attachmentDataMap.ContainsKey("urlname")) {
                //either comes from the application itself, or else, the composition
                docInfoURL = (string)attachmentDataMap["urlname"];
            } else {
                docInfoURL = (string)attachmentDataMap["docinfo_.urlname"];
            }



            var lastIndexOf = docInfoURL.LastIndexOf("\\", StringComparison.Ordinal);
            if (lastIndexOf != -1) {
                //SWWEB-2125
                attachmentDataMap["#parsedurl"] = docInfoURL.Substring(lastIndexOf + 1);
            } else {
                attachmentDataMap["#parsedurl"] = docInfoURL;
            }
            return (string)attachmentDataMap["#parsedurl"];
        }


        public Tuple<byte[], string> DownloadViaHttpById(string docinfoId) {
            var file = AttachmentDao.ById(docinfoId);
            var fileName = (string)file.GetAttribute("document");
            var docinfoURL = (string)file.GetAttribute("urlname");
            var finalURL = GetFileUrl(docinfoURL);
            if (finalURL == null) {
                return null;
            }

            using (var client = new WebClient()) {
                try {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    var fileBytes = client.DownloadData(finalURL);
                    if (docinfoURL.Contains(".")) {
                        var extension = docinfoURL.Substring(docinfoURL.LastIndexOf(".") + 1);

                        // Attach extension to the file name for file association; however, if the extension exist, then don't add extension
                        //                        if (!fileName.ToLower().EndsWith(extension)) {
                        //                            fileName = String.Format("{0}.{1}", fileName, extension);
                        //                        }
                        fileName = BuildParsedURLName(file.Attributes);

                    }
                    return Tuple.Create(fileBytes, fileName);
                } catch (Exception exception) {
                    Log.ErrorFormat("Error Attachment Handler: {0} - {1}", exception.Message, exception.InnerException == null ? "No Internal Error Message" : exception.InnerException.Message);
                    return null;
                }
            }
        }

        public Tuple<byte[], string> DownloadViaParentWS(string id, string parentId, string parentApplication, string parentSchemaId) {

            // Get the parent entity executing a FindById operation in the respective WS
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
                .Application(parentApplication)
                .ApplyPolicies(new ApplicationMetadataSchemaKey(parentSchemaId), user, ClientPlatform.Web, null);
            var response = _dataSetProvider.LookupDataSet(parentApplication, applicationMetadata.Schema.SchemaId).Execute(applicationMetadata, new JObject(), parentId, OperationConstants.CRUD_FIND_BY_ID, false, null);

            var parent = response.ResultObject;
            if (parent != null) {
                var attachments = r.GetProperty(parent, "DOCLINKS") as IEnumerable;
                foreach (var attachment in attachments) {
                    var attachmentId = w.GetRealValue(attachment, "DOCINFOID").ToString();
                    if (id.Equals(attachmentId)) {

                        var fileBytes = w.GetRealValue(attachment, "DOCUMENTDATA") as byte[];
                        var fileName = w.GetRealValue(attachment, "DESCRIPTION") as string;

                        return Tuple.Create(fileBytes, fileName);
                    }
                }
            }

            return null;
        }

        public string GetFileUrl(string docInfoURL) {
            if (_baseMaximoURL == null) {
                BuildMaximoURL();
            }

            Log.DebugFormat("Setting _baseMaximoPath to {0}", _baseMaximoPath);
            Log.DebugFormat("Setting _baseMaximoURL to {0}", _baseMaximoURL);

            Log.DebugFormat("Setting docInfoURL to {0}", docInfoURL);

            if (_baseMaximoPath.Contains("<PATH>")) {
                // Use regular expression to replace remove the starting string - ? prevents it from going greedy and getting all words matching symbol and ^ requires the expression to be at the beginning
                var regExpression = "^" + _baseMaximoPath.Replace("<PATH>", ".*?");
                Regex strRegex = new Regex(regExpression, RegexOptions.None);

                docInfoURL = strRegex.Replace(docInfoURL, "");
            }

            Log.DebugFormat("Updated docInfoURL to {0}", docInfoURL);

            docInfoURL = docInfoURL.Replace("\\", "/");

            var finalURL = _baseMaximoURL != null && _baseMaximoURL.EndsWith("/") ? String.Format("{0}{1}", _baseMaximoURL.Remove(_baseMaximoURL.Length - 1), docInfoURL) : String.Format("{0}{1}", _baseMaximoURL, docInfoURL);

            Log.DebugFormat("Final URL attachment: {0}", finalURL);

            return finalURL;
        }

        /// <summary>
        /// On Mea environment there´s no maxpropvalue table, and the path is stored in a doclink.properties file, 
        /// under C:\Maximo\applications\maximo\properties\doclink.properties. - Please provide 'maximodoclinkspath' and 'maximourldoclinkspath'
        /// 
        /// On Mif, its stored in  maxpropvalue with propname mxe.doclink.path01.
        /// </summary>
        private void BuildMaximoURL() {
            var rawValue = _maxPropValueDao.GetValue("mxe.doclink.path01");
            var valueArr = rawValue.Split('=');
            _baseMaximoPath = valueArr[0].Trim();
            _baseMaximoURL = valueArr[1].Trim();

            // override existing value file is located on a different server - reusing exisitng property field name
            _baseMaximoPath = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksPath) ?? _baseMaximoPath;
            _baseMaximoURL = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksURLPath) ?? _baseMaximoURL;
        }

        public AttachmentDao Dao() {
            return AttachmentDao;
        }

    }
}
