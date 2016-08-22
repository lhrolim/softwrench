using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using Quartz.Util;
using softwrench.sw4.tgcs.classes.com.cts.tgcs.connector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Scheduler.Interfaces;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration {

    public class ToshibaSRSyncJob : ASwJob, ISWEventListener<ConfigurationChangedEvent> {

        private readonly IConfigurationFacade _configurationFacade;
        private readonly JobManager _jobManager;
        private readonly RestEntityRepository _restentityRepository;
        private readonly EntityRepository _entityRepository;
        private readonly SlicedEntityMetadata _slicedEntityMetadata;
        private readonly EntityMetadata _metadata;
        private readonly MaximoConnectorEngine _maximoConnectorEngine;
        private readonly ApplicationMetadata _applicationMetadata;

        public ToshibaSRSyncJob(IConfigurationFacade configurationFacade, JobManager jobManager, RestEntityRepository restEntityRepository, EntityRepository entityRepository, MaximoConnectorEngine maximoConnectorEngine) {
            _configurationFacade = configurationFacade;
            _jobManager = jobManager;
            _restentityRepository = restEntityRepository;
            _entityRepository = entityRepository;
            _maximoConnectorEngine = maximoConnectorEngine;
            _metadata = MetadataProvider.Entity("sr");

            _restentityRepository.KeyName = "ism";

            _applicationMetadata = MetadataProvider.Application("servicerequest").StaticFromSchema("editdetail");
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
        public override void ExecuteJob() {
            var startDate = _configurationFacade.Lookup<DateTime?>(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate);
            if (startDate == null) {
                //playing safe, shouldn´t happen, cause job would be disabled
                return;
            }
            var srs = GetISMUpdates(startDate.Value);
            if (!srs.Any()) {
                if (DateTime.Now > startDate) {
                    _configurationFacade.SetValue(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate, DateTime.Now);
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
            var ourSrs = _entityRepository.Get(_slicedEntityMetadata, dto);


            var maxThreads = _configurationFacade.Lookup<int>(ToshibaConfigurationRegistry.ToshibaSyncMaximoThreads);
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxThreads };

            Log.InfoFormat("{0} updates found invoking services", ourSrs.Count);

            Parallel.ForEach(ourSrs, options, sr => InvokeStatusChangeWS(sr, ismData));

            _configurationFacade.SetValue(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate, srs[0].GetStringAttribute("statusdate"));

        }

        private void InvokeStatusChangeWS(DataMap sr, IReadOnlyDictionary<object, Tuple<string, string>> ismData) {
            //To enforce correct user in case of problems
            LogicalThreadContext.SetData("user", "swjobuser");

            IDictionary<string, object> attributes = new Dictionary<string, object>();

            var ismTicketUid = sr.GetStringAttribute("ismticketuid");
            var ticketUid = sr.GetStringAttribute("ticketuid");
            var ticketid = sr.GetAttribute("ticketid");
            var status = sr.GetStringAttribute("status");

            var ismTuple = ismData[ismTicketUid];
            if (status.EqualsIc(ismTuple.Item1)) {
                Log.WarnFormat("ignoring update of ticket {0} cause the status match at {1}", ticketid, status);
                return;
            }

            attributes.Add("ticketuid", ticketUid);
            attributes.Add("ticketid", ticketid);
            attributes.Add("siteid", sr.GetAttribute("siteid"));

            attributes.Add("status", ismTuple.Item1);
            if (ismTuple.Item2 != null) {
                attributes.Add("itdclosedate", ismTuple.Item2);
            }
            attributes.Add("jobMode", true);
            var crudOperationData = new CrudOperationData(ticketUid, attributes, new Dictionary<string, object>(), _metadata,
                _applicationMetadata);

            Log.DebugFormat("updating status of ticket {0} from {1} to {2}", ticketid, status, ismTuple.Item1);

            var statusData = new ToshibaChangeStatusTicketHandler.ToshibaStatusData {
                CrudData = crudOperationData,
                NewStatus = ismTuple.Item1,
                CloseDate = DateUtil.Parse(ismTuple.Item2),
                JobMode = true,
            };

            _maximoConnectorEngine.Execute(new OperationWrapper(statusData, "ChangeStatus"));
        }

        private IReadOnlyList<DataMap> GetISMUpdates(DateTime startDate) {
            var dateAsString = startDate.ToString(DateUtil.MaximoDefaultIntegrationFormat);
            Log.InfoFormat("fetching updates from ism since date {0}", dateAsString);

            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("statusdate", ">" + dateAsString);
            dto.AppendProjectionFields("status", "itdclosedate", "ticketid", "ticketuid", "statusdate");
            dto.SearchSort = "statusdate";

            return _restentityRepository.Get(_metadata, dto);
        }

        #region JobSetup


        public string GetCron(long? refreshRate = null) {
            var rate = refreshRate ?? _configurationFacade.Lookup<long>(ToshibaConfigurationRegistry.ToshibaSyncRefreshrate);
            return string.Format("0 */{0} * ? * *", rate);
        }

        public override string Cron() {
            return GetCron();
        }

        public override string Description() {
            return "Syncs the sr status out of ISM support Maximo into Softlayer Maximo";
        }


        public override string Name() {
            return "Toshiba SR Sync Job";
        }

        public override bool RunAtStartup() {
            return true;
        }

        public void HandleEvent(ConfigurationChangedEvent eventToDispatch) {
            if (eventToDispatch.ConfigKey.EqualsIc(ToshibaConfigurationRegistry.ToshibaSyncRefreshrate)) {
                var refreshRate = long.Parse(eventToDispatch.CurrentValue);
                UpdateJob(refreshRate);
            }
        }

        private void UpdateJob(long refreshRate) {
            var newCron = GetCron(refreshRate);
            _jobManager.ManageJobByCommand(Name(), JobCommandEnum.ChangeCron, newCron);
        }


        public override bool IsEnabled {
            get {
                //only execute it if there´s a starting date to limit the amount of data
                //TODO: check null possibility, and dates...
                return _configurationFacade.Lookup<DateTime?>(ToshibaConfigurationRegistry.ToshibaSyncSrStatusDate) != null;
            }
        }

        public override void OnJobSchedule() {
            if (!IsEnabled) {
                throw new SwJobException("Cannot start {0} cause no {1} was set. Check Configuration Application", Name(), ToshibaConfigurationRegistry.ToshibaSyncRefreshrate);
            }
        }

        #endregion
    }
}
