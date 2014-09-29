using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers.Application;
using softWrench.sW4.Web.Controllers.Routing;
using softWrench.sW4.Web.Models.Faq;
using softWrench.sW4.Web.SPF;
using softWrench.sW4.Web.Util;
using SpreadsheetLight;

namespace softWrench.sW4.Web.Controllers {
    [System.Web.Http.Authorize]
    public class ExportApiController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ExportApiController));


        private readonly DataController _dataController;
        private readonly ExcelUtil _excelUtil;
        private readonly IContextLookuper _contextLookuper;

        public ExportApiController(DataController dataController, ExcelUtil excelUtil, IContextLookuper contextLookuper) {
            _dataController = dataController;
            _excelUtil = excelUtil;
            _contextLookuper = contextLookuper;
        }

        [System.Web.Http.HttpGet]
        public IGenericResponseResult SetExcelFile(string application, [FromUri]ApplicationMetadataSchemaKey key, [FromUri] PaginatedSearchRequestDto searchDTO, string module) {
            searchDTO.PageSize = searchDTO.TotalCount + 1;
            if (module != null) {
                _contextLookuper.LookupContext().Module = module;
            }
            var dataResponse = _dataController.Get(application,
                                                      new DataRequestAdapter {
                                                          Key = key,
                                                          SearchDTO = searchDTO
                                                      });
            var excelFile = _excelUtil.ConvertGridToExcel(application, key, (ApplicationListResult)dataResponse);
            ApplicationController.SetExcelFile(excelFile);
            return new GenericResponseResult<HttpStatusCode>(HttpStatusCode.OK);
        }

    }
}
