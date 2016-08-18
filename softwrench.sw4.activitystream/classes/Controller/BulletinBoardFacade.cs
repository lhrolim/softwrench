using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.activitystream.classes.Controller.Jobs;
using softwrench.sw4.activitystream.classes.Model;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Scheduler;

namespace softwrench.sw4.activitystream.classes.Controller {
    public class BulletinBoardFacade : ISingletonComponent, ISWEventListener<ConfigurationChangedEvent> {

        private const string QueryActiveBulletinboard = @"select bulletinboard.* from bulletinboard as bulletinboard 
                                                            where (bulletinboard.status='APPROVED' and 
                                                                    bulletinboard.postdate <= :datetimenow and 
                                                                    expiredate > :datetimenow)";

        private static readonly BulletinBoardStream ActiveStream = new BulletinBoardStream();

        private readonly IMaximoHibernateDAO _dao;
        private readonly MultiTenantCustomerWhereBuilder _whereBuilder;
        private readonly IConfigurationFacade _configurationFacade;
        private readonly JobManager _jobManager;

        public BulletinBoardFacade(IMaximoHibernateDAO dao, IConfigurationFacade configurationFacade, JobManager jobManager) {
            _dao = dao;
            _configurationFacade = configurationFacade;
            _jobManager = jobManager;
            _whereBuilder = new MultiTenantCustomerWhereBuilder();
        }

        public void UpdateInMemoryBulletinBoard() {
            var multitenancyWhereClause = _whereBuilder.BuildWhereClause("bulletinboard");

            var query = string.IsNullOrEmpty(multitenancyWhereClause)
                ? QueryActiveBulletinboard
                : QueryActiveBulletinboard + string.Format(" and ({0})", multitenancyWhereClause);

            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("datetimenow", DateTime.Now));

            var messages = _dao.FindByNativeQuery(query, parameters)
                .Cast<IDictionary<string, object>>()
                .Select(BulletinBoard.FromDictionary)
                .ToList();

            ActiveStream.Stream = messages;
        }

        public IReadOnlyList<BulletinBoard> GetActiveBulletinBoards() {
            return ActiveStream.Stream.OrderByDescending(b => b.PostDate).ToList().AsReadOnly();
        }

        public bool BulletinBoardEnabled {
            get {
                return _configurationFacade.Lookup<bool>(ConfigurationConstants.BulletinBoard.Enabled);
            }
        }

        public long BulletinBoardJobRefreshRate {
            get {
                return _configurationFacade.Lookup<long>(ConfigurationConstants.BulletinBoard.JobRefreshRate);
            }
        }

        public long BulletinBoardUiRefreshRate {
            get {
                return _configurationFacade.Lookup<long>(ConfigurationConstants.BulletinBoard.UiRefreshRate);
            }
        }

        public string GetBulletinBoardUpdateJobCron(long? refreshRate = null) {
            var rate = refreshRate ?? BulletinBoardJobRefreshRate;
            return string.Format("0 */{0} * ? * *", rate);
        }

        public void HandleEvent(ConfigurationChangedEvent eventToDispatch) {
            switch (eventToDispatch.ConfigKey) {
                case ConfigurationConstants.BulletinBoard.Enabled:
                    // handle enable config change
                    var enabled = "true".EqualsIc(eventToDispatch.CurrentValue);
                    BulletinBoardEnabledChanged(enabled);
                break;
                case ConfigurationConstants.BulletinBoard.JobRefreshRate:
                    // handle job refresh rate config change by actually changing the job's cron expression
                    var refreshRate = long.Parse(eventToDispatch.CurrentValue);
                    SetBulletinBoardJobCron(refreshRate);
                break;
                default:
                return;
            }
        }

        private void SetBulletinBoardJobCron(long refreshRate) {
            var newCron = GetBulletinBoardUpdateJobCron(refreshRate);
            _jobManager.ManageJobByCommand(BulletinBoardStreamJob.JobName, JobCommandEnum.ChangeCron, newCron);
        }

        private void BulletinBoardEnabledChanged(bool enabled) {
            if (enabled) { // refresh stream
                UpdateInMemoryBulletinBoard();
            } else { // flush stream
                ActiveStream.Stream = Enumerable.Empty<BulletinBoard>().ToList();
            }
        }
    }
}