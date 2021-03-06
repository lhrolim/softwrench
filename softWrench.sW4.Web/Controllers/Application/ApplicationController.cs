﻿using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Controllers.Routing;
using softWrench.sW4.Web.Models.Application;
using softWrench.sW4.Web.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using SpreadsheetLight;

namespace softWrench.sW4.Web.Controllers.Application {
    [System.Web.Mvc.Authorize]
    public class ApplicationController : Controller {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationController));


        private readonly DataController _dataController;
        private readonly ExcelUtil _excelUtil;
        private readonly IConfigurationFacade _facade;
        private readonly I18NResolver _i18NResolver;
        private readonly NextSchemaRouter _nextSchemaRouter = new NextSchemaRouter();
        private readonly IContextLookuper _contextLookuper;

        public ApplicationController(DataController dataController, ExcelUtil excelUtil, IConfigurationFacade facade, I18NResolver i18NResolver, IContextLookuper contextLookuper) {
            _dataController = dataController;
            _excelUtil = excelUtil;
            _facade = facade;
            _i18NResolver = i18NResolver;
            _contextLookuper = contextLookuper;
        }

        public ActionResult Index(string application, string popupmode, [FromUri]DataRequestAdapter request) {
            var user = SecurityFacade.CurrentUser();
            var app = MetadataProvider.Application(application);
            var schemas = app.Schemas();
            ApplicationSchemaDefinition appSchema;
            if (schemas.TryGetValue(request.Key, out appSchema)) {
                //todo apply security
                var dataResponse = _dataController.Get(application, request);
                var model = new ApplicationModel(application, appSchema.SchemaId, request.Key.Mode.ToString().ToLower(), appSchema.Title, dataResponse);
                TempData["model"] = model;
                return RedirectToAction("Index", "Generic",
                    new {
                        includeUrl = "/Content/Controller/Application.html",
                        title = appSchema.Title,
                        popupmode = popupmode
                    });
            }
            throw new InvalidOperationException(String.Format("schema {0} not found", request.Key));
        }

        public ActionResult Input([NotNull] string application, string json, ClientPlatform platform, [NotNull] string currentSchemaKey, string nextSchemaKey) {

            var user = SecurityFacade.CurrentUser();

            Log.Info("receiving ie9 attachment request");

            var currentSchema = _nextSchemaRouter.GetSchemaKeyFromString(application, currentSchemaKey, platform);
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(currentSchema, user, ClientPlatform.Web);
            var queryStrings = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
            var popupmode = queryStrings["popupmode"];
            var datamap = JObject.Parse(json);
            var entityId = datamap[applicationMetadata.Schema.IdFieldName].ToString();

            try {
                PopulateFilesInJson(datamap, String.IsNullOrWhiteSpace(entityId));
                PopulateInputsInJson(datamap);
                var mockMaximo = MockingUtils.IsMockingMaximoModeActive(datamap);

                IApplicationResponse response;
                if (String.IsNullOrWhiteSpace(entityId)) {
                    Log.DebugFormat("redirecting to datacontroller post with datamap " + datamap);
                    response = _dataController.Post(application, datamap, platform, currentSchemaKey, nextSchemaKey, mockMaximo);
                } else {
                    Log.DebugFormat("redirecting to datacontroller put");
                    response = _dataController.Put(application, entityId, datamap, platform, currentSchemaKey,
                        nextSchemaKey, mockMaximo);
                }
                return RedirectToAction("RedirectToAction", "Home", BuildParameter(entityId, application, platform, response, popupmode, applicationMetadata));
            } catch (Exception e) {
                var rootException = ExceptionUtil.DigRootException(e);
                //TODO: handle error properly
                Log.Error(rootException, e);
                return RedirectToAction("RedirectToAction", "Home", BuildErrorParameter(entityId, application, platform, popupmode, applicationMetadata, e));
            }
        }

        private object BuildErrorParameter(string entityid, string application, ClientPlatform platform, string popupmode,
            ApplicationMetadata applicationMetadata, Exception e) {
            return new {
                application = application,
                popupmode = popupmode ?? "none",
                queryString = BuildQueryString(null, entityid, application, platform, applicationMetadata, popupmode),
                message = "Error: " + e.Message,
                messageType = "error"
            };
        }

        private object BuildParameter(string entityid, string application, ClientPlatform platform, IApplicationResponse response,
            string popupmode, ApplicationMetadata applicationMetadata) {
            object parameters;
            if (response is ActionRedirectResponse) {
                var actionResponse = (ActionRedirectResponse)response;
                var queryString = actionResponse.Parameters == null
                    ? null
                    : String.Join("&", actionResponse.Parameters.ToArray());
                parameters = new {
                    actionToRedirect = actionResponse.Action,
                    controllerToRedirect = actionResponse.Controller,
                    queryString = queryString,
                    popupmode = popupmode ?? "none",
                    message = response.SuccessMessage
                };
            } else {
                parameters = new {
                    application = application,
                    popupmode = popupmode ?? "none",
                    queryString = BuildQueryString(response, entityid, application, platform, applicationMetadata, popupmode),
                    message = response.SuccessMessage
                };
            }
            return parameters;
        }

        private void PopulateFilesInJson(JObject datamap, bool isInsert) {
            if (!Request.Files.Any()) {
                Log.DebugFormat("No Files found");
                return;
            }

            foreach (string fileKey in Request.Files.AllKeys) {

                var file = Request.Files[fileKey];
                if (String.IsNullOrWhiteSpace(file.FileName)) {
                    Log.DebugFormat("returning, as filename is null.key:{0} file {1}", fileKey, file);
                    continue;
                }

                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(file.InputStream)) {
                    fileData = binaryReader.ReadBytes(file.ContentLength);
                }
                var fileB64 = Convert.ToBase64String(fileData);
                var fileName = new FileInfo(file.FileName).Name;

                var formattedAttachmentString = fileB64;//FileUtils.GetFormattedAttachmentString(fileB64, file.ContentType);
                if (isInsert) {
                    datamap.Remove(fileKey);
                    datamap.Add(fileKey, formattedAttachmentString);
                    datamap.Remove(fileKey + "_path");
                    datamap.Add(fileKey + "_path", fileName);
                } else {
                    var attachmentList = datamap["attachment_"] as JArray;
                    if (attachmentList == null) {
                        datamap["attachment_"] = new JArray();
                    }
                    if (attachmentList != null) {
                        var attachment = new JObject();
                        Log.DebugFormat("adding file for update. Key: {0} path: {1} content {2} ", fileKey, fileName, formattedAttachmentString);
                        attachment.Add(fileKey, formattedAttachmentString);
                        attachment.Add(fileKey + "_path", fileName);
                        attachmentList.Add(attachment);
                    }
                }
            }
        }

        private void PopulateInputsInJson(JObject datamap) {
            var keysToBeIgnored = new List<String>(new string[] { "application", "json", "platform", "currentSchemaKey", "nextSchemaKey" }); // parameters of "/Application/Input" action
            var keysToBeAdded = new List<String>(Request.Form.AllKeys).Where(x => !keysToBeIgnored.Contains(x)).ToList();

            foreach (var key in keysToBeAdded) {
                datamap.Remove(key);
                datamap.Add(key, Request.Form[key]);
                Log.DebugFormat("adding key {0} to datamap. content {1}", key, Request.Form[key]);
            }
        }

        private String BuildQueryString(IApplicationResponse response, string id, string application, ClientPlatform platform, ApplicationMetadata applicationMetadata, string popupmode) {

            var title = _i18NResolver.I18NSchemaTitle(applicationMetadata.Schema);

            var sb = new StringBuilder();

            ApplicationSchemaDefinition schema;
            if (response is ApplicationDetailResult) {
                schema = response.Schema;
                WebAPIUtil.AppendToQueryString(sb, "id", ((ApplicationDetailResult)response).Id);
            } else {
                if (!String.IsNullOrWhiteSpace(id)) {
                    WebAPIUtil.AppendToQueryString(sb, "id", id);
                }
                schema = applicationMetadata.Schema;
            }
            WebAPIUtil.AppendToQueryString(sb, "popupmode", popupmode);
            WebAPIUtil.AppendToQueryString(sb, "key.schemaId", schema.SchemaId);
            WebAPIUtil.AppendToQueryString(sb, "key.platform", schema.Platform);
            WebAPIUtil.AppendToQueryString(sb, "key.mode", schema.Mode);
            WebAPIUtil.AppendToQueryString(sb, "title", title);            

            return sb.ToString();
        }

        private static SLDocument _excelFile;

        public static void SetExcelFile(SLDocument excelFile) {
            _excelFile = excelFile;
        }

        public void ExportToExcel(string fileName) {
            if (_excelFile == null) {
                return;
            }
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xlsx");
            _excelFile.SaveAs(Response.OutputStream);
            Response.End();
        }

        public FileStreamResult DownloadFile(string fileName, string contentType, string path, bool setFileNameWithDate = false) {
            string content;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream)) {
                content = streamReader.ReadToEnd();
            }

            if (setFileNameWithDate) {
                var fileNameAux = fileName.Split('.');
                var customName = fileNameAux[0] + "_" + DateTime.Now.ToString("G");
                fileName = fileNameAux.Length == 2 ? "{0}{1}".Fmt(customName, "." + fileNameAux[1]) : customName;
            }

            var byteArray = Encoding.ASCII.GetBytes(content);
            var stream = new MemoryStream(byteArray);

            return File(stream, contentType, fileName);
        }

    }
}
