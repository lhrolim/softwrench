using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using r = softWrench.sW4.Util.ReflectionUtil;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;


namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class AttachmentHandler : ISingletonComponent, ISWEventListener<RefreshMetadataEvent> {

        private static readonly ILog Log = LogManager.GetLogger(typeof(AttachmentHandler));

        private readonly MaxPropValueDao _maxPropValueDao = new MaxPropValueDao();
        private readonly DataSetProvider _dataSetProvider;

        public AttachmentDao AttachmentDao { get; }

        /// <summary>
        /// url specifying where the attachments could be downloaded from maximo in the http mode
        /// </summary>
        private string _baseMaximoURL;
        /// <summary>
        /// Path where the files are stored in the maximo´s server fs, must be removed from the url path
        /// </summary>
        private string _baseMaximoPath;

        private readonly MaximoHibernateDAO _maxDAO;

        public AttachmentHandler(MaximoHibernateDAO maxDAO, DataSetProvider dataSetProvider, AttachmentDao attachmentDao) {
            _maxDAO = maxDAO;
            _dataSetProvider = dataSetProvider;
            AttachmentDao = attachmentDao;
        }


        //        public delegate byte[] Base64Delegate(string attachmentData);

        /// <summary>
        /// Used for parsing the base64 string from the html input="file" element
        /// </summary>
        //        public static Base64Delegate InputFile = delegate(string attachmentAsString) {
        //            var indexOf = attachmentAsString.IndexOf(',');
        //            var base64String = attachmentAsString.Substring(indexOf + 1);
        //            return System.Convert.FromBase64String(base64String);
        //        };

        [Transactional(DBType.Maximo)]
        public virtual void HandleAttachmentAndScreenshot(MaximoOperationExecutionContext maximoTemplateData) {
            // Used to get user's current local time for screenshot
            var user = SecurityFacade.CurrentUser();
            // Entity structure contain all the attachment data
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var maximoObj = maximoTemplateData.IntegrationObject;
            // Attachment from a newly created ticket or work order
            var data = entity.GetUnMappedAttribute("newattachment");
            var path = entity.GetUnMappedAttribute("newattachment_path");
            if (!string.IsNullOrWhiteSpace(data) && !string.IsNullOrWhiteSpace(path)) {
                var mainattachments = BuildAttachments(path, data);
                try {
                    mainattachments.ForEach(attachment => AddAttachment(maximoObj, attachment));
                } catch (MaximoException e) {
                    throw new MaximoException("Could not attach image file. Please contact support about 'Installation Task [SWWEB-2156]'", e, ExceptionUtil.DigRootException(e));
                }
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
                    var title = attachment.GetAttribute("document").ToString();
                    var docinfo = (CrudOperationData)attachment.GetRelationship("docinfo",true);
                    var desc = docinfo != null && !string.IsNullOrEmpty(docinfo.GetStringAttribute("description")) ? docinfo.GetStringAttribute("description") : null;

                    data = attachment.GetUnMappedAttribute("newattachment");
                    path = attachment.GetUnMappedAttribute("newattachment_path");
                    var offlinehash = attachment.GetUnMappedAttribute("#offlinehash");
                    var filter = attachment.GetUnMappedAttribute("#filter");
                    var mainattachments = BuildAttachments(path, data, title, desc, offlinehash);
                    if (!string.IsNullOrEmpty(filter)) {
                        mainattachments.ForEach(attch => attch.Filter = filter);
                    }
                    try {
                        mainattachments.ForEach(attch => AddAttachment(maximoObj, attch));
                    } catch (MaximoException e) {
                        throw new MaximoException(
                            "Could not attach image file. Please contact support about 'Installation Task [SWWEB-2156]'",
                            e, ExceptionUtil.DigRootException(e));
                    }
                }

                foreach (var attachment in ((IEnumerable<CrudOperationData>)attachments).Where(a => a.ContainsAttribute("#deleted"))) {
                    _maxDAO.ExecuteSql("delete from doclinks where doclinksid = ?", attachment.GetAttribute("doclinksid"));
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
            if (string.IsNullOrEmpty(attachment.Data) && attachment.BinaryData == null) {
                return;
            }
            // Check if file was rich text file - needed to convert it to word document.
            if (attachment.Path.ToLower().EndsWith("rtf")) {
                var bytes = attachment.BinaryData ?? Convert.FromBase64String(attachment.Data);
                var decodedString = Encoding.UTF8.GetString(bytes);
                var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);
                attachment.BinaryData = Encoding.UTF8.GetBytes(compressedScreenshot);
                attachment.Path = attachment.Path.Substring(0, attachment.Path.Length - 3) + "doc";
            }

            // Exit function - if attachment size exceed specification
            if (!Validate(attachment.Path, attachment.Data, attachment.BinaryData)) {
                return;
            }

            var url = attachment.Path;

            if (ApplicationConfiguration.Is76()) {
                //due to a bug on Maximo 7.6 where attachemtns with spaces saved with inconsistent naming
                //https://controltechnologysolutions.atlassian.net/browse/SWWEB-2616
                url = url.Replace(" ", "_");
            }


            var docLink = ReflectionUtil.InstantiateSingleElementFromArray(maximoObj, "DOCLINKS");
            w.SetValue(docLink, "ADDINFO", true);
            w.SetValue(docLink, "CREATEBY", user.MaximoPersonId);
            w.SetValue(docLink, "CHANGEBY", user.MaximoPersonId);

            //TODO: remove these lines
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEBY", user.MaximoPersonId, "reportedby", true);
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEBY", user.MaximoPersonId, "reportedby", true);

            w.CopyFromRootEntity(maximoObj, docLink, "CREATEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "SITEID", user.SiteId);
            w.CopyFromRootEntity(maximoObj, docLink, "ORGID", user.OrgId);
            w.SetValue(docLink, "URLTYPE", "FILE");
            w.SetValue(docLink, "URLNAME", url);
            w.SetValue(docLink, "UPLOAD", true);
            w.SetValue(docLink, "DOCTYPE", "Attachments");
            if (attachment.OffLineHash != null) {
                //for offline solution
                w.SetValue(docLink, "URLPARAM1", attachment.OffLineHash);
            }else if (attachment.Filter != null) {
                //for fs workpackage solution
                w.SetValue(docLink, "URLPARAM1", attachment.Filter);
            }
            w.SetValue(docLink, "DOCUMENT", FileUtils.Truncate(attachment.Title, GetMaximoLength()) ?? FileUtils.GetNameFromPath(attachment.Path, GetMaximoLength()));
            w.SetValue(docLink, "DESCRIPTION", attachment.Description ?? string.Empty);

            if (attachment.DocumentInfoId != null) {
                w.SetValue(docLink, "URLNAME", attachment.ServerPath);
                w.SetValue(docLink, "NEWURLNAME", attachment.ServerPath);
            } else {
                HandleAttachmentDataAndPath(attachment.Data, docLink, attachment.Path, attachment.BinaryData);
            }
        }


        public static bool Validate(string attachmentPath, string attachmentData, byte[] binaryData = null) {
            var allowedFiles = ApplicationConfiguration.AllowedFilesExtensions;

            if (attachmentPath != null && attachmentPath.IndexOf('.') != -1) {
                var extension = attachmentPath.Substring(attachmentPath.LastIndexOf('.') + 1).ToLower();
                if (!allowedFiles.Any(s => s.Equals(extension, StringComparison.OrdinalIgnoreCase))) {
                    throw new Exception(string.Format("Invalid Attachment extension. Accepted extensions are: {0}.", string.Join(",", allowedFiles)));
                }
            }

            var maxAttSizeInBytes = ApplicationConfiguration.MaxAttachmentSize * 1024 * 1024;
            var size = attachmentData == null ? (binaryData == null ? 0 : binaryData.Length) : attachmentData.Length;
            Log.InfoFormat("Attachment size: {0}", size);
            if (size > maxAttSizeInBytes) {
                var mbSize = size / 1024 / 1024;
                throw new Exception(string.Format(
                    "Attachment is too large ({0} MB). Max attachment size is {1} MB.", mbSize, ApplicationConfiguration.MaxAttachmentSize));
            }

            return true;
        }

        public static void ValidateNotEmpty(string b64PartOnly) {
            if (b64PartOnly.Equals("data:")) {
                throw InvalidAttachmentException.BlankFileNotAllowed();
            }
        }

        protected virtual void HandleAttachmentDataAndPath(string attachmentData, object docLink, string attachmentPath, byte[] binaryData) {
            if (docLink is IRestObjectWrapper) {
                var base64String = attachmentData != null ? FileUtils.GetB64PartOnly(attachmentData) : Convert.ToBase64String(binaryData);
                w.SetValue(docLink, "DOCUMENTDATA", base64String);
            } else {
                var bytes = binaryData ?? FileUtils.ToByteArrayFromHtmlString(attachmentData);
                w.SetValue(docLink, "DOCUMENTDATA", bytes);
            }
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


        public async Task<Tuple<byte[], string>> DownloadViaHttpById(string docinfoId) {
            var file = await AttachmentDao.ById(docinfoId);
            var fileName = (string)file.GetAttribute("document");
            var docinfoURL = (string)file.GetAttribute("urlname");
            var finalURL = await GetFileUrl(docinfoURL);
            if (finalURL == null) {
                return null;
            }

            using (var client = new WebClient()) {
                try {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
        | SecurityProtocolType.Tls11
        | SecurityProtocolType.Tls12
        | SecurityProtocolType.Ssl3;

                    var fileBytes = await client.DownloadDataTaskAsync(finalURL);
                    if (docinfoURL.Contains(".")) {
                        var extension = docinfoURL.Substring(docinfoURL.LastIndexOf(".", StringComparison.Ordinal) + 1);

                        // Attach extension to the file name for file association; however, if the extension exist, then don't add extension
                        //                        if (!fileName.ToLower().EndsWith(extension)) {
                        //                            fileName = String.Format("{0}.{1}", fileName, extension);
                        //                        }
                        fileName = BuildParsedURLName(file);

                    }
                    return Tuple.Create(fileBytes, fileName);
                } catch (Exception exception) {
                    Log.ErrorFormat("Error Attachment Handler: {0} - {1}", exception.Message, exception.InnerException == null ? "No Internal Error Message" : exception.InnerException.Message);
                    return null;
                }
            }
        }

        public async Task<Tuple<byte[], string>> DownloadViaHttpByIdReturningMime(string docInfoId) {
            var file = await AttachmentDao.ById(docInfoId);
            var fileName = (string)file.GetAttribute("document");
            var docinfoURL = (string)file.GetAttribute("urlname");
            var finalURL = await GetFileUrl(docinfoURL);
            if (finalURL == null) {
                return null;
            }


            using (var client = new HttpClient()) {
                try {
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) => true;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;
                    var response = await client.GetAsync(finalURL);
                    var filetype = response.Content.Headers.ContentType.MediaType;
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return Tuple.Create(fileBytes, filetype);
                } catch (Exception exception) {
                    Log.ErrorFormat("Error Attachment Handler: {0} - {1}", exception.Message, exception.InnerException == null ?
                        "No Internal Error Message" : exception.InnerException.Message);
                    return null;
                }
            }
        }


        public async Task<Tuple<byte[], string>> DownloadViaParentWS(string id, string parentId, string parentApplication, string parentSchemaId) {

            // Get the parent entity executing a FindById operation in the respective WS
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
                .Application(parentApplication)
                .ApplyPolicies(new ApplicationMetadataSchemaKey(parentSchemaId), user, ClientPlatform.Web, null);
            var response = await _dataSetProvider.LookupDataSet(parentApplication, applicationMetadata.Schema.SchemaId).Execute(applicationMetadata, new JObject(), parentId, OperationConstants.CRUD_FIND_BY_ID, false, null);

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

        public async Task<string> GetFileUrl(string docInfoURL) {
            if (_baseMaximoURL == null) {
                await BuildMaximoURL();
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
        /// Builds a list of attachmentdtos from path and string data that are for multiple files.
        /// </summary>
        /// <param name="paths">file paths concatenated by a ','</param>
        /// <param name="data">base64 encoded file data concatenated by a ','</param>
        /// <param name="title">file titles concatenated by a ','</param>
        /// <param name="desc">file descriptions concatenated by a ','</param>
        /// <param name="offlinehash">offlinehash used to identify attachment in offline env concatenated by a ','</param>
        /// <returns></returns>
        public List<AttachmentDTO> BuildAttachments(string paths, string data, string title = null, string desc = null, string offlinehash = null) {
            if (string.IsNullOrWhiteSpace(paths) || string.IsNullOrWhiteSpace(data)) {
                return new List<AttachmentDTO>();
            }
            var attachmentsData = data.Split(',');
            var attachmentsPath = paths.Split(',');
            string[] attachmentsTitle = null;
            if (title != null) {
                attachmentsTitle = title.Split(',');
            }
            string[] attachmentsDesc = null;
            if (desc != null) {
                attachmentsDesc = desc.Split(',');
            }
            string[] attachmnetsOfflinehash = null;
            if (offlinehash != null) {
                attachmnetsOfflinehash = offlinehash.Split(',');
            }

            //whether or not the data:application/pdf;base64, prefix was already stripped
            var pureBase64String = !data.StartsWith("data:");

            var dtos = new List<AttachmentDTO>(attachmentsPath.Length);
            for (int i = 0, j = 0; i < attachmentsPath.Length; i++, j += 2) {
                var attachmentTitle = attachmentsTitle != null ? attachmentsTitle[i] : null;
                var attachmentDesc = attachmentsDesc != null ? attachmentsDesc[i] : null;
                var attachmentOfflinehash = attachmnetsOfflinehash != null ? attachmnetsOfflinehash[i] : null;
                AttachmentDTO dto = new AttachmentDTO() {
                    Path = attachmentsPath[i],
                    Title = attachmentTitle,
                    OffLineHash = attachmentOfflinehash,
                    Description = attachmentDesc,
                    DocumentInfoId = null
                };

                if (pureBase64String) {
                    dto.Data = attachmentsData[j];
                } else {
                    //on that case there´ll be a , between the base64 prefix and the real data, such as data:application/pdf;base64,asafasdfasdfa==
                    //therefore we need to join the , splitted string back
                    if (attachmentsData.Length <= j + 1) {
                        throw new Exception("One or more selected files contain no data and connot be added as attachments.");
                    }
                    dto.Data = attachmentsData[j] + ',' + attachmentsData[j + 1];
                }


                dtos.Add(dto);
            }
            return dtos;
        }

        /// <summary>
        /// On Mea environment there´s no maxpropvalue table, and the path is stored in a doclink.properties file, 
        /// under C:\Maximo\applications\maximo\properties\doclink.properties. - Please provide 'maximodoclinkspath' and 'maximourldoclinkspath'
        /// 
        /// On Mif, its stored in  maxpropvalue with propname mxe.doclink.path01.
        /// </summary>
        private async
        /// <summary>
        /// On Mea environment there´s no maxpropvalue table, and the path is stored in a doclink.properties file, 
        /// under C:\Maximo\applications\maximo\properties\doclink.properties. - Please provide 'maximodoclinkspath' and 'maximourldoclinkspath'
        /// 
        /// On Mif, its stored in  maxpropvalue with propname mxe.doclink.path01.
        /// </summary>
        Task
BuildMaximoURL() {
            var rawValue = await _maxPropValueDao.GetValue("mxe.doclink.path01");
            var valueArr = rawValue.Split('=');
            _baseMaximoPath = valueArr[0].Trim();
            _baseMaximoURL = valueArr[1].Trim();

            // override existing value file is located on a different server - reusing exisitng property field name
            _baseMaximoPath = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksPath) ?? _baseMaximoPath;
            _baseMaximoURL = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksURLPath) ?? _baseMaximoURL;
        }

        public void HandleEvent(RefreshMetadataEvent eventToDispatch) {
            _baseMaximoURL = null;
        }
    }
}
