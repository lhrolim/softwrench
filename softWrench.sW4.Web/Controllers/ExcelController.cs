using System.IO;
using System.Web.Mvc;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Security.Context;
using System.Web.Http;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Web.Util;

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
            [FromUri] PaginatedSearchRequestDto searchDTO, string module, string fileName) {


            searchDTO.PageSize = searchDTO.TotalCount + 1;
            if (module != null) {
                _contextLookuper.LookupContext().Module = module;
            }
            var dataResponse = _dataController.Get(application, new DataRequestAdapter {
                Key = key,
                SearchDTO = searchDTO
            });
            var excelFile = _excelUtil.ConvertGridToExcel(application, key, (ApplicationListResult)dataResponse);
            var stream = new MemoryStream();
            excelFile.SaveAs(stream);
            stream.Close();
            var excelFileExtension = ".xls";
            if (!fileName.EndsWith(excelFileExtension)) {
                fileName += excelFileExtension;
            }
            var result = new FileContentResult(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = fileName
            };
            return result;
        }
    }
}

