using System.Diagnostics;
using System.IO;
using System.Web.Mvc;
using log4net;
using softWrench.sW4.Data.Pagination;
using softwrench.sw4.Hapag.Data;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using System.Web.Http;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;
using softwrench.sw4.Shared2.Util;

namespace softWrench.sW4.Web.Controllers {

    public class ExcelController : FileDownloadController {

        private readonly IContextLookuper _contextLookuper;
        private readonly DataController _dataController;
        private readonly ExcelUtil _excelUtil;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ExcelController));

        public ExcelController(IContextLookuper contextLookuper, DataController dataController, ExcelUtil excelUtil, AssetRamControlWhereClauseProvider assetRamControlWhereClauseProvider) {
            _contextLookuper = contextLookuper;
            _dataController = dataController;
            _excelUtil = excelUtil;
        }

        public FileContentResult Export(string application, [FromUri]ApplicationMetadataSchemaKey key,
            [FromUri] PaginatedSearchRequestDto searchDTO, string module) {


            searchDTO.PageSize = searchDTO.TotalCount + 1;
            if (module != null) {
                _contextLookuper.LookupContext().Module = module;
            }


            var before = Stopwatch.StartNew();
            var before2 = Stopwatch.StartNew();

            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
              .Application(application)
              .ApplyPolicies(key, user, ClientPlatform.Web);


            var dataResponse = _dataController.Get(application, new DataRequestAdapter {
                Key = key,
                SearchDTO = searchDTO
            });
            Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "finished gathering export excel data"));
            var excelBytes = _excelUtil.ConvertGridToExcel(user, applicationMetadata.Schema, ((ApplicationListResult)dataResponse).ResultObject);

            Log.Info(LoggingUtil.BaseDurationMessageFormat(before2, "finished export excel data"));
            var fileName = GetFileName(application, key.SchemaId) + ".xls";
            var result = new FileContentResult(excelBytes, System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = (string)StringUtil.FirstLetterToUpper(fileName)
            };
            return result;
        }


        public string GetFileName(string application, string schemaId) {
            if (application != "asset") {
                return application + "Export";
            }
            if (schemaId == "categories") {
                return "AssetCategoriesExport";
            }
            if (schemaId == "exportallthecolumns") {
                return "AssetListExport";
            }
            return "AssetExport";
        }
    }
}