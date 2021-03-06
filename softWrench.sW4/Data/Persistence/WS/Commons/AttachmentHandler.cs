﻿using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
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

        public void HandleAttachments(object maximoObj, string attachmentData, string attachmentPath, ApplicationMetadata applicationMetadata) {

            if (!String.IsNullOrWhiteSpace(attachmentData) && !String.IsNullOrWhiteSpace(attachmentPath)) {

                var user = SecurityFacade.CurrentUser();
                if (String.IsNullOrEmpty(attachmentData)) {
                    return;
                }
                var docLink = ReflectionUtil.InstantiateSingleElementFromArray(maximoObj, "DOCLINKS");
                CommonCode(maximoObj, docLink, user, attachmentPath, attachmentData);
                HandleAttachmentDataAndPath(attachmentData, docLink, attachmentPath);

            }
        }

        private void CommonCode(object maximoObj, object docLink, InMemoryUser user, string attachmentPath, string attachmentData) {
            w.SetValue(docLink, "ADDINFO", true);
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEBY", user.Login, "reportedby");
            w.CopyFromRootEntity(maximoObj, docLink, "CREATEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEBY", user.Login, "reportedby");
            w.CopyFromRootEntity(maximoObj, docLink, "CHANGEDATE", DateTime.Now.FromServerToRightKind());
            w.CopyFromRootEntity(maximoObj, docLink, "SITEID", user.SiteId);
            w.CopyFromRootEntity(maximoObj, docLink, "ORGID", user.OrgId);
            w.SetValue(docLink, "URLTYPE", "FILE");
            w.SetValue(docLink, "URLNAME", attachmentPath);
            Validate(attachmentPath, attachmentData);

            w.SetValue(docLink, "UPLOAD", true);
            w.SetValue(docLink, "DOCTYPE", "Attachments");
            w.SetValue(docLink, "DOCUMENT", FileUtils.GetNameFromPath(attachmentPath, GetMaximoLength()));
            w.SetValue(docLink, "DESCRIPTION", attachmentPath);
        }

        public static bool Validate(string attachmentPath, string attachmentData) {
            var allowedFiles = ApplicationConfiguration.AllowedFilesExtensions;

            if (attachmentPath != null && attachmentPath.IndexOf('.') != -1 &&
                !allowedFiles.Contains(attachmentPath.Substring(attachmentPath.LastIndexOf('.') + 1).ToLower())) {
                throw new Exception(String.Format(
                    "Invalid Attachment extension. Accepted extensions are: {0}.", String.Join(",", allowedFiles)));
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
            return 20;
        }



        public Tuple<Byte[], string> DownloadViaHttpById(string docinfoId) {
            var file = AttachmentDao.ById(docinfoId);
            var fileName = (string)file.GetAttribute("document");
            var docinfoURL = (string)file.GetAttribute("urlname");
            var finalURL = GetFileUrl(docinfoURL);
            if (finalURL == null) {
                return null;
            }
            using (var client = new WebClient()) {
                try {
                    var fileBytes = client.DownloadData(finalURL);
                    return Tuple.Create(fileBytes, fileName);
                } catch (Exception) {
                    return null;
                }
            }
        }

        public Tuple<Byte[], string> DownloadViaParentWS(string id, string parentId, string parentApplication, string parentSchemaId) {

            // Get the parent entity executing a FindById operation in the respective WS
            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
                .Application(parentApplication)
                .ApplyPolicies(new ApplicationMetadataSchemaKey(parentSchemaId), user, ClientPlatform.Web);
            var response = _dataSetProvider.LookupAsBaseDataSet(parentApplication).Execute(applicationMetadata, new JObject(), parentId, OperationConstants.CRUD_FIND_BY_ID);

            var parent = response.ResultObject;
            if (parent != null) {
                var attachments = r.GetProperty(parent, "DOCLINKS") as IEnumerable;
                foreach (var attachment in attachments) {
                    var attachmentId = w.GetRealValue(attachment, "DOCINFOID").ToString();
                    if (id.Equals(attachmentId)) {

                        var fileBytes = w.GetRealValue(attachment, "DOCUMENTDATA") as byte[];
                        var fileName = BuildFileName(w.GetRealValue(attachment, "URLNAME") as String);

                        return Tuple.Create(fileBytes, fileName);
                    }
                }
            }

            return null;
        }

        public static string BuildFileName(string urlName) {
            //HAP-1041
            var idx = urlName.LastIndexOf("/", StringComparison.Ordinal);
            if (idx == -1) {
                idx = urlName.LastIndexOf("\\", StringComparison.Ordinal);
            }
            if (idx == -1) {
                Log.WarnFormat("couldn´t locate filename out of url property {0}", urlName);
                return null;
            }
            return urlName.Substring(idx + 1);
        }

        public string GetFileUrl(String docInfoURL) {
            if (_baseMaximoURL == null) {
                BuildMaximoURL();
            }
            if (docInfoURL.StartsWith("\\")) {
                docInfoURL = "C:" + docInfoURL;
            }
            String pattern = "^[A-Z]\\:.*";
            bool check = Regex.IsMatch(docInfoURL, pattern);
            if (check && _baseMaximoPath.Contains("<PATH>")) {
                //docInfoURL = docInfoURL.Substring(1);
                docInfoURL = docInfoURL.Replace(":", "<PATH>");
            }


            if (!docInfoURL.StartsWith(_baseMaximoPath)) {
                return null;
            }

            docInfoURL = docInfoURL.Remove(0, _baseMaximoPath.Length);
            docInfoURL = docInfoURL.Replace("\\", "/");
            if (docInfoURL.StartsWith("/")) {
                docInfoURL = docInfoURL.Substring(1);
            }
            var finalURL = String.Format("{0}{1}", _baseMaximoURL, docInfoURL);
            return finalURL;
        }

        /// <summary>
        /// On Mea environment there´s no maxpropvalue table, and the path is stored in a doclink.properties file, 
        /// under C:\Maximo\applications\maximo\properties\doclink.properties.
        /// 
        /// On Mif, its stored in  maxpropvalue with propname mxe.doclink.path01.
        /// </summary>
        private void BuildMaximoURL() {
            if (ApplicationConfiguration.IsMif()) {
                var rawValue = _maxPropValueDao.GetValue("mxe.doclink.path01");
                var valueArr = rawValue.Split('=');
                _baseMaximoPath = valueArr[0].Trim();
                _baseMaximoURL = valueArr[1].Trim();
            } else {
                _baseMaximoPath = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksPath);
                _baseMaximoURL = MetadataProvider.GlobalProperty(ApplicationMetadataConstants.MaximoDocLinksURLPath);
            }
            if (!_baseMaximoURL.EndsWith("/")) {
                _baseMaximoURL = _baseMaximoURL + "/";
            }
        }


    }
}
