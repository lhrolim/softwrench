using System.IO;
using System.Web.Mvc;
using cts.commons.portable.Util;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Security.Context;
using System.Web.Http;
using cts.commons.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Web.Util;
using softWrench.sW4.Security.Services;

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

        public FileContentResult Export(string application, [FromUri]ApplicationMetadataSchemaKey key,
            [FromUri] PaginatedSearchRequestDto searchDTO, string module) {


            searchDTO.PageSize = searchDTO.TotalCount + 1;
            if (module != null) {
                _contextLookuper.LookupContext().Module = module;
            }
            var dataResponse = _dataController.Get(application, new DataRequestAdapter {
                Key = key,
                SearchDTO = searchDTO
            });

            var loggedInUser = SecurityFacade.CurrentUser();

            return DoExport(application, key, (ApplicationListResult) dataResponse, loggedInUser);
        }


        private FileContentResult DoExport(string application, ApplicationMetadataSchemaKey key,ApplicationListResult dataResponse, InMemoryUser loggedInUser) {

            var excelFile = _excelUtil.ConvertGridToExcel(application, dataResponse, loggedInUser);
            var stream = new MemoryStream();
            excelFile.SaveAs(stream);
            stream.Close();
            var fileName = GetFileName(application, key.SchemaId) + ".xls";
            var result = new FileContentResult(CompressionUtil.Compress(stream.ToArray()),
                System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = (string)StringUtil.FirstLetterToUpper(fileName)
            };
            Response.AddHeader("Content-encoding", "gzip");
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

