using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.integration;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.batch.api.services;
using softwrench.sw4.tgcs.classes.com.cts.tgcs.connector;
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

    public class ToshibaSRSyncJob : ConfigurableRateSwJob {

        private readonly IConfigurationFacade _configurationFacade;
        private readonly RestEntityRepository _restentityRepository;
        private readonly EntityRepository _entityRepository;
        private readonly SlicedEntityMetadata _slicedEntityMetadata;
        private readonly EntityMetadata _metadata;
        private readonly ApplicationMetadata _applicationMetadata;
        private readonly IBatchSubmissionService _batchService;

        public ToshibaSRSyncJob(IConfigurationFacade configurationFacade, JobManager jobManager, RestEntityRepository restEntityRepository, EntityRepository entityRepository, IBatchSubmissionService batchService) : base(configurationFacade, jobManager) {
            _configurationFacade = configurationFacade;
            _restentityRepository = restEntityRepository;
            _entityRepository = entityRepository;
            _batchService = batchService;
            _metadata = MetadataProvider.Entity("sr");

            _restentityRepository.KeyName = "ism";

            var application = MetadataProvider.Application("servicerequest", false);
            if (!ApplicationConfiguration.IsClient("tgcs")) {
                return;
            }
            _applicationMetadata = application.StaticFromSchema("editdetail");
            _slicedEntityMetadata = MetadataProvider.SlicedEntityMetadata(_applicationMetadata);
        }

        /// <summary>
        /// Executes the following steps:
        /// 
        /// 1) Goes to ISM Maximo using the rest API and bring any SRs in which the status date is after the last sync date config
        /// 2) For this list pick the ones which exist on our side (softlayer)
        /// 3) From each of these update their status on SoftLayer Maximo by running a Web-Service call
        /// 4) update current sync date config
        /// 
        /// </summary>
        public override async Task ExecuteJob() {
            var startDate = _configurationFacade.Lookup<DateTime?>(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate);
            if (startDate == null) {
                //playing safe, shouldn´t happen, cause job would be disabled
                return;
            }
            var srs = await GetISMUpdates(startDate.Value);
            if (!srs.Any()) {
                if (DateTime.Now > startDate) {
                    await _configurationFacade.SetValue(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate, DateTime.Now);
                    Log.InfoFormat("no updates found, finishing job execution, marking current date");
                } else {
                    Log.InfoFormat("no updates found, finishing job execution");
                }
                return;
            }

            var ismData = srs.ToDictionary(s => s.GetAttribute("ticketuid"), s => new Tuple<string, string>(s.GetStringAttribute("status"), s.GetStringAttribute("itdclosedate")));
            var inQuery = BaseQueryUtil.GenerateInString(srs, "ticketuid");
            var dto = new PaginatedSearchRequestDto();
            dto.AppendWhereClauseFormat("ismticketuid in ({0})", inQuery);
            var ourSrs = await _entityRepository.Get(_slicedEntityMetadata, dto);


            Log.InfoFormat("{0} updates found invoking services", ourSrs.Count);

            var operationWrappers = ourSrs.Select(s => BuildOperationWrapper(s, ismData)).Where(i => i != null).ToList();

            _batchService.SubmitTransientBatch(new TransientBatchOperationData() {
                OperationWrappers = operationWrappers,
                AppMetadata = _applicationMetadata,

                BeforeWSExecution = delegate (IOperationWrapper wrapper) {
                    var ticketid = wrapper.GetStringAttribute("ticketid");
                    var status = wrapper.GetStringAttribute("status");
                    var newStatus = ((ToshibaChangeStatusTicketHandler.ToshibaStatusData)(wrapper.GetOperationData)).NewStatus;
                    Log.DebugFormat("updating status of ticket {0} from {1} to {2}", ticketid, status, newStatus);
                    return true;
                },

                BatchOptions = new BatchOptions {
                    MaxThreadsProperty = ToshibaConfigurationRegistry.ToshibaSyncMaximoThreads,
                    ProblemKey = "ism.sr.statussync",
                }
            });


            await _configurationFacade.SetValue(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate, srs[0].GetStringAttribute("statusdate"));

        }

        private OperationWrapper BuildOperationWrapper(DataMap sr, IReadOnlyDictionary<object, Tuple<string, string>> ismData) {

            var ismTicketUid = sr.GetStringAttribute("ismticketuid");
            var ticketUid = sr.GetStringAttribute("ticketuid");
            var ticketid = sr.GetAttribute("ticketid");
            var status = sr.GetStringAttribute("status");

            var ismTuple = ismData[ismTicketUid];

            if (status.EqualsIc(ismTuple.Item1)) {
                Log.WarnFormat("ignoring update of ticket {0} cause the status already match at {1}", ticketid, status);
                return null;
            }

            var crudOperationData = new CrudOperationData(ticketUid, sr, new Dictionary<string, object>(), _metadata,
                _applicationMetadata);

            var statusData = new ToshibaChangeStatusTicketHandler.ToshibaStatusData {
                CrudData = crudOperationData,
                NewStatus = ismTuple.Item1,
                CloseDate = DateUtil.Parse(ismTuple.Item2),
                ProblemData = new OperationProblemData("ism.sr.statussync")
            };

            return new OperationWrapper(statusData, "ChangeStatus");
        }

        private async Task<IReadOnlyList<DataMap>> GetISMUpdates(DateTime startDate) {
            var dateAsString = startDate.ToString(DateUtil.MaximoDefaultIntegrationFormat);
            Log.InfoFormat("fetching updates from ism since date {0}", dateAsString);

            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("statusdate", ">" + dateAsString);
            dto.AppendProjectionFields("status", "itdclosedate", "ticketid", "ticketuid", "statusdate");
            dto.SearchSort = "statusdate";

            return await _restentityRepository.Get(_metadata, dto);
        }

        #region JobSetup






        public override string Description() {
            return "Syncs the sr status out of ISM support Maximo into Softlayer Maximo";
        }


        public override string Name() {
            return "Toshiba SR Sync Job";
        }

        public override bool RunAtStartup() {
            return true;
        }


        public override bool IsEnabled {
            get {
                //only execute it if there´s a starting date to limit the amount of data
                return _configurationFacade.Lookup<DateTime?>(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate) != null;
            }
        }



        #endregion

        protected override string JobConfigKey {
            get {
                return ToshibaConfigurationRegistry.ToshibaSRSyncRefreshrate;
            }
        }
    }
}
