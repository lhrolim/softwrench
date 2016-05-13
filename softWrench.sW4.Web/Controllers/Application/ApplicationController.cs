using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softWrench.sW4.Data.API.Response;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Util;
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
using System.IO.Compression;

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
        [ValidateInput(false)]
        public ActionResult Input([NotNull] string application, string json, ClientPlatform platform, [NotNull] string currentSchemaKey, string nextSchemaKey) {

            var user = SecurityFacade.CurrentUser();

            Log.Info("receiving ie9 attachment request");

            var currentSchema = SchemaUtil.GetSchemaKeyFromString(currentSchemaKey, platform);
            var applicationMetadata = MetadataProvider
                .Application(application)
                .ApplyPolicies(currentSchema, user, ClientPlatform.Web);
            var queryStrings = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
            var popupmode = queryStrings["popupmode"];

            try {
                var datamap = JObject.Parse(json);
                var entityId = datamap[applicationMetadata.Schema.IdFieldName].ToString();


                PopulateFilesInJson(datamap, String.IsNullOrWhiteSpace(entityId));
                PopulateInputsInJson(datamap);
                var mockMaximo = MockingUtils.IsMockingMaximoModeActive(datamap);

                IApplicationResponse response;
                var operationRequest = new OperationDataRequest {
                    ApplicationName = application,
                    CurrentSchemaKey = currentSchemaKey,
                    RouteParametersDTO = new RouterParametersDTO() {
                        NextSchemaKey = nextSchemaKey
                    },
                    Id = entityId,
                    MockMaximo = mockMaximo,
                    Platform = platform
                };
                if (String.IsNullOrWhiteSpace(entityId)) {
                    Log.DebugFormat("redirecting to datacontroller post with datamap " + datamap);
                    response = _dataController.Post(new JsonRequestWrapper {
                        Json = datamap,
                        RequestData = operationRequest
                    });
                } else {
                    Log.DebugFormat("redirecting to datacontroller put");
                    response = _dataController.Put(new JsonRequestWrapper {
                        Json = datamap,
                        RequestData = operationRequest
                    });
                }
                return RedirectToAction("RedirectToAction", "Home", BuildParameter(application, platform, response, popupmode, applicationMetadata));
            } catch (Exception e) {
                var rootException = ExceptionUtil.DigRootException(e);
                //TODO: handle error properly
                Log.Error(rootException, e);
                return RedirectToAction("RedirectToAction", "Home", BuildErrorParameter(application, platform, popupmode, applicationMetadata, e));
            }
        }

        private object BuildErrorParameter(string application, ClientPlatform platform, string popupmode,
            ApplicationMetadata applicationMetadata, Exception e) {
            return new {
                application = application,
                popupmode = popupmode ?? "none",
                queryString = BuildQueryString(null, application, platform, applicationMetadata),
                message = "Error: " + e.Message
            };
        }

        private object BuildParameter(string application, ClientPlatform platform, IApplicationResponse response,
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
                    queryString = BuildQueryString(response, application, platform, applicationMetadata),
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
                        if (attachmentList.Any()) {
                            //workaround for ie9 COM-SW-56
                            attachment = (JObject)attachmentList[0];
                            attachment.Remove(fileKey);
                            attachment.Remove(fileKey + "_path");
                        } else {
                            attachmentList.Add(attachment);
                        }
                        attachment.Add(fileKey, formattedAttachmentString);
                        attachment.Add(fileKey + "_path", fileName); ;

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

        private String BuildQueryString(IApplicationResponse response, string application, ClientPlatform platform, ApplicationMetadata applicationMetadata) {

            var title = _i18NResolver.I18NSchemaTitle(applicationMetadata.Schema);

            var sb = new StringBuilder();

            ApplicationSchemaDefinition schema;
            if (response is ApplicationDetailResult) {
                schema = response.Schema;
                WebAPIUtil.AppendToQueryString(sb, "id", ((ApplicationDetailResult)response).Id);
            } else if (response is BlankApplicationResponse) {
                schema = applicationMetadata.Schema;
                WebAPIUtil.AppendToQueryString(sb, "id", ((BlankApplicationResponse)response).Id);
            } else {
                schema = applicationMetadata.Schema;
            }
            WebAPIUtil.AppendToQueryString(sb, "key.schemaId", schema.SchemaId);
            WebAPIUtil.AppendToQueryString(sb, "key.platform", schema.Platform);
            WebAPIUtil.AppendToQueryString(sb, "title", title);

            return sb.ToString();
        }


        public void ExportToExcel(string application, [FromUri]ApplicationMetadataSchemaKey key, [FromUri] PaginatedSearchRequestDto searchDTO, string module, string fileName) {
            searchDTO.PageSize = searchDTO.TotalCount + 1;
            if (module != null) {
                _contextLookuper.LookupContext().Module = module;
            }
            var dataResponse = _dataController.Get(application,
                                                      new DataRequestAdapter {
                                                          Key = key,
                                                          SearchDTO = searchDTO
                                                      });
            var loggedInUser = SecurityFacade.CurrentUser();

            var excelFile = _excelUtil.ConvertGridToExcel(application, key, (ApplicationListResult)dataResponse, loggedInUser);
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename={0}.xlsx".Fmt(fileName));
            excelFile.SaveAs(Response.OutputStream);
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

        /// <summary>
        /// Downloads a zip file containing the log files.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="contentType">The content type</param>
        /// <param name="path">the file path</param>
        /// <param name="setFileNameWithDate">flag to append date with file name.</param>
        /// <returns>A <see cref="FileContentResult"/> result.</returns>
        public FileContentResult DownloadLogFilesZipBundle(string fileName, string contentType, string path, bool setFileNameWithDate = false) {
            var zipFileName = Path.GetTempPath() + "LogFiles_" + DateTime.UtcNow.Ticks + ".zip";
            using (var zipFile = new FileStream(zipFileName, FileMode.Create)) {
                using (var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Update)) {
                    for (int i = 0; i <= 5; i++) {
                        var filePath = path;

                        if (i != 0) { filePath = filePath + "." + i.ToString(); }

                        if (System.IO.File.Exists(filePath)) {
                            string content;
                            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var streamReader = new StreamReader(fileStream)) {
                                content = streamReader.ReadToEnd();
                            }

                            var fileNameAux = fileName.Split('.');
                            var customName = setFileNameWithDate ? fileNameAux[0] + string.Format("_{0}_", i) + DateTime.Now.ToString("MMM-dd-yyyy") : fileNameAux[0] + string.Format("_{0}", i);
                            var tempName = string.Format("{0}{1}", customName, ".txt");

                            var byteArray = Encoding.ASCII.GetBytes(content);
                            var stream = new MemoryStream(byteArray);

                            this.AddFileToZip(tempName, stream, zipArchive);
                        }
                    }
                }
            }

            var contentBuffer = GetBytesFromFile(zipFileName);
            System.IO.File.Delete(zipFileName);

            return File(contentBuffer, contentType, fileName);
        }

        /// <summary>
        /// Adds the file to zip.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileStream">file stream.</param>
        /// <param name="zipArchive">The zip archive.</param>
        private void AddFileToZip(string fileName, Stream fileStream, ZipArchive zipArchive) {
            var entry = zipArchive.CreateEntry(fileName);
            using (var writer = entry.Open()) {
                fileStream.CopyTo(writer);
            }
        }

        /// <summary>
        /// Gets the bytes from file.
        /// </summary>
        /// <param name="zipFileName">Name of the zip file.</param>
        /// <returns>An array of bytes.</returns>
        private static byte[] GetBytesFromFile(string zipFileName) {
            byte[] contentBuffer;
            using (var zipStream = new FileStream(zipFileName, FileMode.Open)) {
                contentBuffer = new byte[zipStream.Length];
                zipStream.Position = 0;
                zipStream.Read(contentBuffer, 0, contentBuffer.Length);
            }

            return contentBuffer;
        }

    }
}
