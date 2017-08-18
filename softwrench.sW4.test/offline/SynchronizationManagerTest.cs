using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.services;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.offline {

    [TestClass]
    public class SynchronizationManagerTest : BaseOtbMetadataTest {

        private SynchronizationManager _syncManager;

        private readonly InMemoryUser _user = InMemoryUser.TestInstance("swadmin");

        private readonly Mock<ISWDBHibernateDAO> _swdbMock = TestUtil.CreateMock<ISWDBHibernateDAO>();
        private readonly Mock<EntityRepository> _entityRepository = TestUtil.CreateMock<EntityRepository>();
        private readonly Mock<IContextLookuper> _contextLookuper = TestUtil.CreateMock<IContextLookuper>();
        private readonly Mock<IEventDispatcher> _eventDispatcher = TestUtil.CreateMock<IEventDispatcher>();
        private readonly Mock<SyncChunkHandler> _syncChunkManager = TestUtil.CreateMock<SyncChunkHandler>();
        private readonly Mock<IConfigurationFacade> _configFacade = TestUtil.CreateMock<IConfigurationFacade>();
        private readonly Mock<RedisManager> _redisManager = TestUtil.CreateMock<RedisManager>();

        [TestInitialize]
        public override void Init() {
            base.Init();
            TestUtil.ResetMocks(_swdbMock, _entityRepository, _contextLookuper, _eventDispatcher, _syncChunkManager,
                _configFacade, _redisManager);
            _syncManager = new SynchronizationManager(null, _entityRepository.Object, _contextLookuper.Object, _eventDispatcher.Object,
                _swdbMock.Object, _syncChunkManager.Object, _configFacade.Object, _redisManager.Object);
            _user.OrgId = "EAGLESA";
            _user.SiteId = "1803";

            _configFacade.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(100);
            _configFacade.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxAssociationThreads, null)).ReturnsAsync(4);
            _user.Genericproperties[FirstSolarConstants.FacilitiesProp] = new List<string> { "ACS", "AES" };
            _redisManager.Setup(r => r.IsAvailable()).Returns(true);
        }

        //        [TestMethod]
        public async Task TestInitialLoadSimulatingSingleApp() {
            var dto = new sw4.offlineserver.dto.association.AssociationSynchronizationRequestDto { InitialLoad = true, ApplicationsToFetch = new List<string> { "location" } };


            var locationSyncSchema = MetadataProvider.Application("location").Schemas()[new ApplicationMetadataSchemaKey("@sync")];

            var lookupDTO = new RedisLookupDTO { Schema = locationSyncSchema, GlobalLimit = 100 };
            var result = new RedisLookupResult<DataMap> { Schema = locationSyncSchema };
            result.Chunks.Add(new RedisResultDTO<DataMap>(lookupDTO.BuildKeys().First() + ";chunk:1", GenerateListOfDataMaps("location", "locationuid", 100)));
            _redisManager.Setup(r => r.Lookup<DataMap>(lookupDTO)).ReturnsAsync(result);

            await _syncManager.GetAssociationData(_user, dto);



        }

        [TestMethod]
        public void ApplicationsFromDatabaseToFetchAllCacheCompletedNotOverFlown() {
            var apps = OffLineMetadataProvider.FetchAssociationApps(_user, true);

            var dto = new sw4.offlineserver.dto.association.AssociationSynchronizationResultDto(1000);
            foreach (var app in apps) {
                if (app.ApplicationName != "assignment") {
                    dto.AssociationData[app.ApplicationName] =
                        new sw4.offlineserver.dto.association.AssociationDataDto {
                            HasMoreCachedEntries = false,
                            CompleteCacheEntries = new Dictionary<string,CacheRoundtripStatus> { {"fakecache",new CacheRoundtripStatus {Complete = true} }}
                        };
                }
            }
            var results = _syncManager.DatabaseApplicationsToCollect(true,dto, apps, false);
            var completeApplicationMetadataDefinitions = results as IList<CompleteApplicationMetadataDefinition> ?? results.ToList();

            Assert.AreEqual(3, completeApplicationMetadataDefinitions.Count());
            Assert.AreEqual("assignment", completeApplicationMetadataDefinitions[0].ApplicationName);
            Assert.AreEqual("labor", completeApplicationMetadataDefinitions[1].ApplicationName);
            Assert.AreEqual("laborcraftrate", completeApplicationMetadataDefinitions[2].ApplicationName);
        }

        [TestMethod]
        public void ApplicationsFromDatabaseToFetchAllCacheCompletedOverFlown() {
            var apps = OffLineMetadataProvider.FetchAssociationApps(_user, true);

            var dto = new sw4.offlineserver.dto.association.AssociationSynchronizationResultDto(1000);
            foreach (var app in apps) {
                if (app.ApplicationName != "assignment") {
                    dto.AssociationData[app.ApplicationName] =
                        new sw4.offlineserver.dto.association.AssociationDataDto {
                            HasMoreCachedEntries = false,
                            CompleteCacheEntries = new Dictionary<string, CacheRoundtripStatus> { { "fakecache" + app.ApplicationName, new CacheRoundtripStatus { Complete = true } } }
                        };
                }
            }
            var results = _syncManager.DatabaseApplicationsToCollect(true, dto, apps, true);
            var completeApplicationMetadataDefinitions = results as IList<CompleteApplicationMetadataDefinition> ?? results.ToList();

            Assert.AreEqual(1, completeApplicationMetadataDefinitions.Count());
        }


        [TestMethod]
        public void CacheMissNonOverFlowScenario() {
            var apps = OffLineMetadataProvider.FetchAssociationApps(_user, true);

            var dto = new sw4.offlineserver.dto.association.AssociationSynchronizationResultDto(1000);
            foreach (var app in apps) {
                if (app.ApplicationName == "offlineasset"){
                    dto.AssociationData[app.ApplicationName] =
                        new sw4.offlineserver.dto.association.AssociationDataDto {
                            HasMoreCachedEntries = false,
                            CacheMiss = true,
                            CompleteCacheEntries = new Dictionary<string, CacheRoundtripStatus> { { "fakecache" + app.ApplicationName, new CacheRoundtripStatus { Complete = true } } }
                        };
                }

                else if (app.ApplicationName != "assignment") {
                    dto.AssociationData[app.ApplicationName] =
                        new sw4.offlineserver.dto.association.AssociationDataDto {
                            HasMoreCachedEntries = false,
                            CompleteCacheEntries = new Dictionary<string, CacheRoundtripStatus> { { "fakecache" + app.ApplicationName, new CacheRoundtripStatus { Complete = true } } }
                        };
                }
            }
            var results = _syncManager.DatabaseApplicationsToCollect(true, dto, apps, false);
            var completeApplicationMetadataDefinitions = results as IList<CompleteApplicationMetadataDefinition> ?? results.ToList();

            Assert.IsTrue(completeApplicationMetadataDefinitions.Any(a=> a.ApplicationName.Equals("offlineasset")));
            Assert.IsTrue(completeApplicationMetadataDefinitions.Any(a=> a.ApplicationName.Equals("assignment")));
        }




        private List<DataMap> GenerateListOfDataMaps(string application, string uidName, int count, int offSet = 0) {
            var results = new List<DataMap>();
            for (var i = 1; i < count + 1; i++) {
                var dict = new Dictionary<string, object>();
                dict[uidName] = i + offSet;
                dict["rowstamp"] = 1L;
                var dm = DataMap.GetInstanceFromDictionary(application, dict, true);
                dm.Id = (i + offSet).ToString();
                results.Add(dm);
            }
            return results;
        }


        public override string GetClientName() {
            return "firstsolar";
        }

    }
}
