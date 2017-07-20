using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using log4net;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Scheduler;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.jobs {
    public class FirstSolarLocationCacheJob : ASwJob {
        private const string FacilitiesQuery = @"select location, siteid, orgid from omworkgroup";

        private readonly IMaximoHibernateDAO _dao;
        private readonly IRedisManager _redisManager;
        private readonly EntityRepository _repository;
        private readonly IConfigurationFacade _configFacade;
        private readonly ILog _log = LogManager.GetLogger(typeof(FirstSolarLocationCacheJob));

        public FirstSolarLocationCacheJob(IMaximoHibernateDAO dao, IRedisManager redisManager, EntityRepository repository, IConfigurationFacade configFacade) {
            _dao = dao;
            _redisManager = redisManager;
            _repository = repository;
            _configFacade = configFacade;
        }

        public override string Name() {
            return "First Solar Location Cache";
        }

        public override string Description() {
            return "Caches the locations on redis.";
        }

        public override string Cron() {
            return "0 0 * * * ?";
        }

        public override bool RunAtStartup() {
            return false;
        }

        public override async Task ExecuteJob() {
            if (!_redisManager.IsAvailable()) {
                return;
            }

            var schema = MetadataProvider.Schema("offlinelocation", "list", ClientPlatform.Mobile);
            var locationEntity = MetadataProvider.SlicedEntityMetadata(schema);
            var chunkLimit = await _configFacade.LookupAsync<int>(OfflineConstants.MaxDownloadSize);

            await InnerExecute(locationEntity, schema, chunkLimit);
        }

        private async Task InnerExecute(EntityMetadata entity, ApplicationSchemaDefinition schema, int chunkLimit) {
            _log.Debug("First Solar location cache fire and forget started.");

            var facilities = await GetAllFacilities();
            if (!facilities.Any()) {
                _log.Debug("First Solar location cache fire and forget ended with no facilities.");
                return;
            }

            facilities.ForEach((facility) => {
                FetchLocations(facility, entity, schema, chunkLimit);
            });

            _log.Debug("First Solar location cache fire and forget ended.");
        }

        private void FetchLocations(FsFacility fsFacility, EntityMetadata entity, ApplicationSchemaDefinition schema, int chunkLimit) {
            var searchDto = new PaginatedSearchRequestDto {
                PageSize = chunkLimit,
                PageNumber = 1,
                SearchSort = entity.IdFieldName + " asc",
                WhereClause = " (location.siteid = '{0}' and location.orgid = '{1}' and location.location like '{2}%') ".Fmt(fsFacility.SiteId, fsFacility.OrgId, fsFacility.Facility)
            };

            var lookupDTO = new RedisLookupDTO {
                Schema = schema,
                IsOffline = true,
                GlobalLimit = chunkLimit
            };
            lookupDTO.ExtraKeys.Add("siteid", fsFacility.SiteId);
            lookupDTO.ExtraKeys.Add("orgid", fsFacility.OrgId);
            lookupDTO.ExtraKeys.Add("facilities", fsFacility.Facility.ToLower());

            var rowstamp = 0L;

            var descriptors = AsyncHelper.RunSync(() => _redisManager.GetDescriptors(lookupDTO));
            if (descriptors.Any()) {
                rowstamp = descriptors.First().MaxRowstamp;
            }

            var result = AsyncHelper.RunSync(() => _repository.Get(entity, rowstamp, searchDto));

            if (!result.Any()) {
                return;
            }

            var dataMaps = result.Select(datamap => {
                datamap.Application = "offlinelocation";
                datamap.Id = datamap["locationsid"] + "";
                var nullableRowstamp = datamap["rowstamp"] as long?;
                long currentRowstamp = 0;
                if (nullableRowstamp != null) {
                    currentRowstamp = (long)nullableRowstamp;
                }
                if (currentRowstamp > rowstamp) {
                    rowstamp = currentRowstamp;
                }
                return new JSONConvertedDatamap(datamap);
            }).ToList();

            AsyncHelper.RunSync(() => _redisManager.InsertIntoCache(lookupDTO, new RedisInputDTO<JSONConvertedDatamap>(dataMaps)));
        }

        private async Task<List<FsFacility>> GetAllFacilities() {
            var facilities = new List<FsFacility>();
            var result = await _dao.FindByNativeQueryAsync(FacilitiesQuery);
            result.ForEach((row) => {
                if (!row.ContainsKey("location") || !row.ContainsKey("siteid") || !row.ContainsKey("orgid")) {
                    return;
                }
                var facility = new FsFacility() {
                    Facility = row["location"],
                    SiteId = row["siteid"],
                    OrgId = row["orgid"]
                };
                if (!facilities.Contains(facility)) {
                    facilities.Add(facility);
                }
            });
            return facilities;
        }

        public class FsFacility {
            public string Facility { get; set; }
            public string SiteId { get; set; }
            public string OrgId { get; set; }

            protected bool Equals(FsFacility other) {
                return string.Equals(Facility, other.Facility) && string.Equals(SiteId, other.SiteId) && string.Equals(OrgId, other.OrgId);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((FsFacility)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (Facility != null ? Facility.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (SiteId != null ? SiteId.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (OrgId != null ? OrgId.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}
