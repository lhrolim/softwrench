using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.Core;
using NHibernate.Util;
using softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Scheduler.Jobs {
    // ReSharper disable once InconsistentNaming
    public class R0042ExtractorJob : ASwJob {
        private const string FirstDayOfMonth = "0 0 0 1 * ?";
        private const int PageSize = 100;

        private readonly EntityRepository _entityRepository;
        protected readonly SWDBHibernateDAO DAO;
        protected readonly IConfigurationFacade ConfigFacade;

        private new readonly ILog Log = LogManager.GetLogger(typeof(R0042ExtractorJob));

        public R0042ExtractorJob(EntityRepository entityRepository, IConfigurationFacade configFacade, SWDBHibernateDAO dao) {
            _entityRepository = entityRepository;
            ConfigFacade = configFacade;
            DAO = dao;
        }


        public override string Name() {
            return "R0042 Asset Extractor";
        }

        public override string Description() {
            return "Store all asset values into database for further comparison";
        }

        public override string Cron() {
            return FirstDayOfMonth;
        }

        public override void ExecuteJob() {
            var lowerRowstamp = ConfigFacade.Lookup<long>(ConfigurationConstants.R0042Rowstamp);
            var now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, 1);
            var lastMonth = DateUtil.BeginOfDay(now.AddMonths(-1));


            var beginOfMonth = DateUtil.BeginOfDay(now);
            var compMetadata = MetadataProvider.Application("asset");
            var schema = compMetadata.Schemas()[new ApplicationMetadataSchemaKey("R0042Export")];
            var slicedMetadata = MetadataProvider.SlicedEntityMetadata(schema.GetSchemaKey(), "asset");
            var dto = new PaginatedSearchRequestDto { PageSize = PageSize };

            var needsMore = true;
            var initOfBatch = true;
            var i = 1;

            while (needsMore) {
                Log.InfoFormat("R0042: fetching first {0} items restricted to rowstamp {1}".Fmt(i * PageSize, lowerRowstamp));
                var searchEntityResult = FetchMore(dto, lastMonth, slicedMetadata, lowerRowstamp, initOfBatch);
                IList<R0042AssetHistory> items = ConvertItems(searchEntityResult.ResultList, beginOfMonth);
                if (!items.Any()) {
                    break;
                }
                DAO.BulkSave(items);
                var greatestRowstamp = items[items.Count - 1].Rowstamp;

                //there´s at least one extra item, but could be thousands
                needsMore = items.Count == PageSize;
                i++;
                if (greatestRowstamp.HasValue) {
                    ConfigFacade.SetValue(ConfigurationConstants.R0042Rowstamp, greatestRowstamp);
                    Log.InfoFormat("R0042: updating rowstamp to {0}".Fmt(greatestRowstamp.Value));
                    lowerRowstamp = greatestRowstamp.Value;
                }

                initOfBatch = false;
            }

        }

        private IList<R0042AssetHistory> ConvertItems(IList<Dictionary<string, object>> resultList, DateTime beginOfMonth) {
            IList<R0042AssetHistory> items = new List<R0042AssetHistory>();
            foreach (var dbItem in resultList) {
                var asset = new R0042AssetHistory() {
                    ExtractionDate = beginOfMonth
                };
                asset.AssetId = dbItem["assetid"].ToString();
                asset.Assetnum = dbItem["assetnum"].ToString();
                asset.ItcName = dbItem["primaryuser_.hlagdisplayname"] as string;
                asset.UserId = dbItem["aucisowner_.hlagdisplayname"] as string;
                asset.LocDescription = dbItem["location_.description"] as string;
                asset.Department = dbItem["assetglaccount_.displaycostcenter"] as string;
                asset.Floor = dbItem["location_.floor"] as string;
                asset.Room = dbItem["location_.room"] as string;
                asset.SerialNum = dbItem["serialnum"] as string;
                asset.EosDate = dbItem["assetspeceosdate_.alnvalue"] as string;
                asset.Usage = dbItem["usage"] as string;
                asset.MacAddress = dbItem["assetspecmacaddress_.alnvalue"] as string;
                asset.ChangeDate = dbItem["changedate"] as DateTime?;
                asset.Status = dbItem["status"].ToString();
                asset.Rowstamp = Int64.Parse(dbItem["rowstamp"].ToString());
                items.Add(asset);
            }

            return items;
        }

        private EntityRepository.SearchEntityResult FetchMore(PaginatedSearchRequestDto dto, DateTime lastMonth,
            SlicedEntityMetadata slicedMetadata, long? lowerRowstamp, Boolean initofBatch) {
            dto.AppendSearchEntry("status", "!=DECOMMISSIONED");
            dto.AppendSearchEntry("changedate", ">=" + lastMonth.ToShortDateString());
            if (initofBatch) {
                dto.AppendSearchEntry("rowstamp", ">" + lowerRowstamp);
            } else {
                dto.AppendSearchEntry("rowstamp", ">=" + lowerRowstamp);
            }

            dto.SearchSort = "rowstamp asc";
            dto.QueryAlias = "R0042";
            var searchEntityResult = _entityRepository.GetAsRawDictionary(slicedMetadata, dto, false);
            return searchEntityResult;
        }


        public override bool IsScheduled {
            get; set;
        }
        public override bool RunAtStartup() {
            return ApplicationConfiguration.IsLocal();
        }
    }
}
