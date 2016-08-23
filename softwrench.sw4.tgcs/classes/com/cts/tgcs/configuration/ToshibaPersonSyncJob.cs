using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.api.classes.integration;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.batch.api.services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration {

    public class ToshibaPersonSyncJob : ConfigurableRateSwJob {

        private readonly IConfigurationFacade _configurationFacade;
        private readonly RestEntityRepository _restentityRepository;
        private readonly EntityRepository _entityRepository;
        private readonly SlicedEntityMetadata _slicedEntityMetadata;
        private readonly EntityMetadata _metadata;
        private readonly IBatchSubmissionService _batchSubmissionService;
        private readonly ApplicationMetadata _applicationMetadata;

        public ToshibaPersonSyncJob(IConfigurationFacade configurationFacade, JobManager jobManager, RestEntityRepository restEntityRepository, EntityRepository entityRepository, IBatchSubmissionService batchSubmissionService) : base(configurationFacade, jobManager) {
            _configurationFacade = configurationFacade;
            _restentityRepository = restEntityRepository;
            _entityRepository = entityRepository;
            _batchSubmissionService = batchSubmissionService;
            _metadata = MetadataProvider.Entity("person");

            _restentityRepository.KeyName = "ism";

            var application = MetadataProvider.Application("person", false);
            if (!ApplicationConfiguration.IsClient("tgcs")) {
                return;
            }
            _applicationMetadata = application.StaticFromSchema("detail");
            _slicedEntityMetadata = MetadataProvider.SlicedEntityMetadata(_applicationMetadata);
        }

        /// <summary>
        /// Executes the following steps:
        /// 
        /// 1) Goes to ISM Maximo using the rest API and bring any person entries which the personuid are greater than the latest stored job execution
        /// 2) Check whether these persons are not yet present under softlayer by checking their personid (not personuid!)
        /// 3) Create the missing entries
        /// 4) update current sync personuid date
        /// 
        ///  NOTE: This job relies on the personuid sequentially incremented, and won´t bring any updates on an existing person
        /// 
        /// </summary>
        public override void ExecuteJob() {
            var minPersonUid = _configurationFacade.Lookup<long?>(ToshibaConfigurationRegistry.ToshibaSyncPersonUId);
            if (minPersonUid == null) {
                //playing safe, shouldn´t happen, cause job would be disabled
                return;
            }
            var ismPersonEntries = GetISMUpdates(minPersonUid);
            if (ApplicationConfiguration.IsLocal()) {
                RandomizeLocalPersonIds(ismPersonEntries);
            }

            if (!ismPersonEntries.Any()) {
                Log.InfoFormat("no updates found, finishing job execution");
                return;
            }

            var missingSoftlayerEntries = FindMissingEntries(ismPersonEntries);

            var biggestPersonUId = ismPersonEntries[0].GetStringAttribute("personuid");

            if (!missingSoftlayerEntries.Any()) {
                Log.InfoFormat("All entries are already present at SoftLayer side finishing job execution");
                _configurationFacade.SetValue(ToshibaConfigurationRegistry.ToshibaSyncPersonUId, biggestPersonUId);
                return;
            }

            Log.InfoFormat("creating {0} new person entries", missingSoftlayerEntries.Count());


            _batchSubmissionService.SubmitTransientBatch(new TransientBatchOperationData {
                Datamaps = missingSoftlayerEntries,
                OperationName = OperationConstants.CRUD_CREATE,
                AppMetadata = _applicationMetadata,
                BeforeWSExecution = delegate (IOperationWrapper wrapper) {
                    Log.DebugFormat("creating person {0} on Softlayer", wrapper.GetStringAttribute("personid"));
                    return true;
                },
                BatchOptions = new BatchOptions {
                    MaxThreadsProperty = ToshibaConfigurationRegistry.ToshibaSyncMaximoThreads,
                    ProblemKey = "ism.sr.personsync",
                }

            });

            Log.DebugOrInfoFormat("updating personuid to {0} ", biggestPersonUId);
            _configurationFacade.SetValue(ToshibaConfigurationRegistry.ToshibaSyncPersonUId, biggestPersonUId);


        }
        /// <summary>
        /// so that we can test easier locally, pointing to itself
        /// </summary>
        /// <param name="ismPersonEntries"></param>
        private static void RandomizeLocalPersonIds(IReadOnlyList<DataMap> ismPersonEntries) {
            foreach (var ismPersonEntry in ismPersonEntries) {
                var rnd = new Random();
                ismPersonEntry.SetAttribute("personid", ismPersonEntry.GetStringAttribute("personid") + rnd.Next(1, 3));
                ismPersonEntry.SetAttribute("PRIMARYEMAIL", ismPersonEntry.GetStringAttribute("PRIMARYEMAIL") + rnd.Next(1, 3));
            }
        }

        private IList<DataMap> FindMissingEntries(IReadOnlyList<DataMap> ismPersonEntries) {
            var inQuery = BaseQueryUtil.GenerateInString(ismPersonEntries, "personid");
            var dto = new PaginatedSearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("personid"));
            dto.AppendWhereClauseFormat("personid in ({0})", inQuery);

            var personEntriesWeHave =
                _entityRepository.Get(_slicedEntityMetadata, dto).Select(r => r.GetStringAttribute("personid"));

            var personEntriesWeDoNotHave =
                ismPersonEntries.Where(r => !personEntriesWeHave.Contains(r.GetStringAttribute("personid")));
            return personEntriesWeDoNotHave.ToList();
        }


        private IReadOnlyList<DataMap> GetISMUpdates(long? storedPersonUid) {

            Log.InfoFormat("fetching person updates from ism since id: {0}", storedPersonUid);

            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("personuid", ">" + storedPersonUid);
            dto.AppendSearchEntry("status", "ACTIVE");
            dto.SearchSort = "personuid";

            return _restentityRepository.Get(_metadata, dto);
        }

        #region JobSetup


        public override string Description() {
            return "Syncs the person entries out of ISM support Maximo into Softlayer Maximo";
        }

        public override string Name() {
            return "Toshiba Person Sync Job";
        }

        public override bool RunAtStartup() {
            return ApplicationConfiguration.IsLocal();
        }

        protected override string JobConfigKey {
            get {
                return ToshibaConfigurationRegistry.ToshibaPersonSyncRefreshrate;
            }
        }


        public override bool IsEnabled {
            get {
                return _configurationFacade.Lookup<long?>(ToshibaConfigurationRegistry.ToshibaSyncPersonUId) != null;
            }
        }


        #endregion
    }
}
