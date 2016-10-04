using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using cts.commons.portable.Util;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Security.Context;
using System.Web.Http;
using cts.commons.web.Controller;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softWrench.sW4.Web.Controllers {

    public class ExcelController : FileDownloadController {

        private readonly IContextLookuper _contextLookuper;
        private readonly DataController _dataController;
        private readonly ExcelUtil _excelUtil;

        public ExcelController(IContextLookuper contextLookuper, DataController dataController, ExcelUtil excelUtil) {
            _contextLookuper = contextLookuper;
            _dataController = dataController;
            _excelUtil = excelUtil;
        }

        public async Task<FileContentResult> Export(string application, [FromUri]ApplicationMetadataSchemaKey key,
            [FromUri] PaginatedSearchRequestDto searchDTO, string module) {


            searchDTO.PageSize = searchDTO.TotalCount + 1;
            if (module != null) {
                var context = _contextLookuper.LookupContext();
                context.Module = module;
                _contextLookuper.AddContext(context);
            }
            var dataResponse = await _dataController.Get(application, new DataRequestAdapter {
                Key = key,
                SearchDTO = searchDTO
            });

            var loggedInUser = SecurityFacade.CurrentUser();
            var fileName = GetFileName(application, key.SchemaId) + ".xls";
            return DoExport(fileName, (ApplicationListResult) dataResponse, loggedInUser);
        }

        //TODO: move to some sort of commons package
        protected FileContentResult DoExport(string fileName, ApplicationListResult dataResponse, InMemoryUser loggedInUser) {

            var excelFile = _excelUtil.ConvertGridToExcel(dataResponse, loggedInUser);
            using (var stream = new MemoryStream()) {
                excelFile.SaveAs(stream);
                stream.Close();
                var result = new FileContentResult(CompressionUtil.Compress(stream.ToArray()),
                    System.Net.Mime.MediaTypeNames.Application.Octet) {
                    FileDownloadName = (string)StringUtil.FirstLetterToUpper(fileName)
                };
                Response.AddHeader("Content-encoding", "gzip");
                return result;
            }
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

