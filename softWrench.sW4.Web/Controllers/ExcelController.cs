using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using log4net;
using Quartz.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;
using System.Web.Http;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;
using softwrench.sw4.Shared2.Util;
using SpreadsheetLight;

namespace softWrench.sW4.Web.Controllers {

    public class ExcelController : FileDownloadController {

        private readonly IContextLookuper _contextLookuper;
        private readonly DataController _dataController;
        private readonly ExcelUtil _excelUtil;
        private readonly MaximoConnectorEngine _maximoEngine;
        private readonly IConfigurationFacade _configurationFacade;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ExcelController));

        public ExcelController(IContextLookuper contextLookuper, DataController dataController, ExcelUtil excelUtil, MaximoConnectorEngine maximoEngine, IConfigurationFacade configurationFacade) {
            _contextLookuper = contextLookuper;
            _dataController = dataController;
            _excelUtil = excelUtil;
            _maximoEngine = maximoEngine;
            _configurationFacade = configurationFacade;
        }
        /// <summary>
        ///  Builds the excel file considering the application, the schemakey, and the searchDTO to use.
        ///  
        ///  Since the excel export does not take pagination into consideration, we might need to perform multiple "chunk"queries in a row to avoid database timeouts. 
        /// 
        /// Each of these queries, will be responsible for a different "count range" limits (0-1000, 1001-2000, ...); 
        /// 
        /// they will be performed on a different thread each, and the results will be assembled together in the end.
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="key"></param>
        /// <param name="searchDto"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public FileContentResult Export(string application, [FromUri]ApplicationMetadataSchemaKey key,
            [FromUri] PaginatedSearchRequestDto searchDto, string module) {

            var before = Stopwatch.StartNew();
            var before2 = Stopwatch.StartNew();

            searchDto.PageSize = searchDto.TotalCount + 1;
            var ctx = _contextLookuper.LookupContext();
            ctx.Module = module;

            var user = SecurityFacade.CurrentUser();
            var applicationMetadata = MetadataProvider
              .Application(application)
              .ApplyPolicies(key, user, ClientPlatform.Web);

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
            var count = _maximoEngine.Count(entityMetadata, searchDto);
            var maxChunk = _configurationFacade.Lookup<Int32>(ConfigurationConstants.MaxQuerySizeChunk);

            var tempResultMap = new SortedDictionary<Int32, IReadOnlyList<AttributeHolder>>();
            var schema = applicationMetadata.Schema;

            var needChunk = count > maxChunk;

            SLDocument excelFile;

            if (needChunk) {
                //in that case, we need to do the queries using multiple threads, to avoid a out of memory error or timeout.
                var numberOfThreads = (count / maxChunk) + 1;
                var tasks = new Task[numberOfThreads];
                //if the chunk is 1000 --> each page shall bring 1000 results
                searchDto.PageSize = maxChunk;
                for (var i = 0; i < numberOfThreads; i++) {
                    var chunkContext = new ContextHolderWithSearchDto(ctx, searchDto, i);
                    //just copying the variable to the thread executions
                    tasks[i] = Task.Factory.NewThread(c => {
                        var innerchunkContext = (ContextHolderWithSearchDto)c;
                        Quartz.Util.LogicalThreadContext.SetData("context", innerchunkContext.Context);
                        SingleExecution(innerchunkContext.Dto, schema, tempResultMap, entityMetadata, innerchunkContext.PageNumber);
                    }, chunkContext);
                }
                Task.WaitAll(tasks);
                var rows = new List<AttributeHolder>();
                foreach (var item in tempResultMap) {
                    rows.AddRange(item.Value);
                }
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "finished gathering export excel data"));
                excelFile = _excelUtil.ConvertGridToExcel(user, schema, rows);
            } else {
                SingleExecution(searchDto, schema, tempResultMap, entityMetadata, 0);
                excelFile = _excelUtil.ConvertGridToExcel(user, schema, tempResultMap[0]);
                Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "finished gathering export excel data"));
            }
            var stream = new MemoryStream();
            excelFile.SaveAs(stream);
            stream.Close();

            Log.Info(LoggingUtil.BaseDurationMessageFormat(before2, "finished export excel data"));

            var fileName = GetFileName(application, key.SchemaId) + ".xls";
            var result = new FileContentResult(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet) {
                FileDownloadName = (string)StringUtil.FirstLetterToUpper(fileName)
            };
            return result;
        }

        class ContextHolderWithSearchDto {
            readonly ContextHolder context;
            readonly PaginatedSearchRequestDto dto;
            readonly Int32 pageNumber;

            public ContextHolderWithSearchDto(ContextHolder context, PaginatedSearchRequestDto dto, int pageNumber) {
                this.context = context;
                this.dto = ReflectionUtil.DeepClone(dto);
                this.pageNumber = pageNumber;
            }

            public ContextHolder Context {
                get { return context; }
            }

            public PaginatedSearchRequestDto Dto {
                get { return dto; }
            }

            public int PageNumber {
                get { return pageNumber; }
            }
        }

        private void SingleExecution(PaginatedSearchRequestDto searchDto, ApplicationSchemaDefinition schema, IDictionary<int, IReadOnlyList<AttributeHolder>> tempResultMap, SlicedEntityMetadata entityMetadata, int pageNumber) {
            var before = Stopwatch.StartNew();
            var applicationCompositionSchemata = new Dictionary<string, ApplicationCompositionSchema>();
            searchDto.PageNumber = pageNumber + 1;
            if (searchDto.CompositionsToFetch != null && searchDto.CompositionsToFetch.Count > 0) {
                var allCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(schema);
                // Only fetch the compositions schemas if indicated on searchDTO;
                foreach (var compositionSchema in allCompositionSchemas) {
                    if (searchDto.CompositionsToFetch.Contains(compositionSchema.Key)) {
                        applicationCompositionSchemata.Add(compositionSchema.Key, compositionSchema.Value);
                    }
                }
            }

            var readOnlyList = _maximoEngine.Find(entityMetadata, searchDto, applicationCompositionSchemata);
            Log.Debug(LoggingUtil.BaseDurationMessageFormat(before, "finished gathering chunk {0}", pageNumber));
            tempResultMap.Add(pageNumber, readOnlyList);
        }

        public string GetFileName(string application, string schemaId) {
            if (application != "asset") {
                return application + "Export";
            }
            if (schemaId == "categories") {
                return "AssetCategoriesExport";
            }
            if (schemaId == "assetlistreport") {
                return "AssetListExport";
            }
            return "AssetExport";
        }
    }
}