using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.web.Formatting;
using Newtonsoft.Json;
using softwrench.sw4.activitystream.classes.Controller.Jobs;
using softwrench.sw4.activitystream.classes.Model;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Scheduler;
using ServerSentEvent4Net;

namespace softwrench.sw4.activitystream.classes.Controller {
    public class BulletinBoardFacade : ISingletonComponent, ISWEventListener<ConfigurationChangedEvent> {

        private const string QueryActiveBulletinboard = @"select bulletinboard.* from bulletinboard as bulletinboard 
                                                            where (bulletinboard.status='APPROVED' and 
                                                                    bulletinboard.postdate <= :datetimenow and 
                                                                    expiredate > :datetimenow)";

        private static readonly Lazy<BulletinBoardStream> LazyActiveStream = new Lazy<BulletinBoardStream>(() => new BulletinBoardStream());
        private static readonly Lazy<ServerSentEvent> LazyEventSource = new Lazy<ServerSentEvent>(() => {
            var sse = new ServerSentEvent(1, false, 5 * 60 * 1000);
            sse.SubscriberAdded += SSE_SubscriberChanged;
            sse.SubscriberRemoved += SSE_SubscriberChanged;
            return sse;
        });

        private readonly IMaximoHibernateDAO _dao;
        private readonly MultiTenantCustomerWhereBuilder _whereBuilder;
        private readonly IConfigurationFacade _configurationFacade;
        private readonly JobManager _jobManager;
        private readonly JsonMediaTypeFormatter _formatter;

        public BulletinBoardFacade(IMaximoHibernateDAO dao, IConfigurationFacade configurationFacade, JobManager jobManager, ISWJsonFormatter jsonFormatter) {
            _dao = dao;
            _configurationFacade = configurationFacade;
            _jobManager = jobManager;
            _whereBuilder = new MultiTenantCustomerWhereBuilder();
            _formatter = (JsonMediaTypeFormatter)jsonFormatter;
        }

        private static ServerSentEvent EventSource {
            get {
                return LazyEventSource.Value;
            }
        }

        private static BulletinBoardStream ActiveStream {
            get {
                return LazyActiveStream.Value;
            }
        }

        /// <summary>
        /// Updates in-memory state and broadcasts the new state to all subscribers (<see cref="AddBulletinBoardUpdateSubscriber"/>)
        /// </summary>
        public async Task UpdateInMemoryBulletinBoard() {
            var multitenancyWhereClause = _whereBuilder.BuildWhereClause("bulletinboard");

            var query = string.IsNullOrEmpty(multitenancyWhereClause)
                ? QueryActiveBulletinboard
                : QueryActiveBulletinboard + string.Format(" and ({0})", multitenancyWhereClause);

            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("datetimenow", DateTime.Now));
            var lists = await _dao.FindByNativeQueryAsync(query, parameters);
            var messages = lists
                .Cast<IDictionary<string, object>>()
                .Select(BulletinBoard.FromDictionary)
                .ToList();

            // update in-memory stream
            ActiveStream.Stream = messages;

            // broadcast new memory state to all SSE subscribers
            var bulletinBoardsState = GetActiveBulletinBoardsState();
            var stateMessage = JsonConvert.SerializeObject(bulletinBoardsState, Formatting.None, _formatter.SerializerSettings);
            EventSource.Send(stateMessage);
        }

        public IReadOnlyList<BulletinBoard> GetActiveBulletinBoards() {
            return ActiveStream.Stream.OrderByDescending(b => b.PostDate).ToList().AsReadOnly();
        }

        public BulletinBoardResponse GetActiveBulletinBoardsState() {
            var messages = GetActiveBulletinBoards();
            return new BulletinBoardResponse(messages.ToList());
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

        /// <summary>
        /// Subscribes the request to the BulletinBoard update SSE i.e. responds with an open stream connection response.
        /// The response will receive the in-memory state (<see cref="GetActiveBulletinBoardsState"/>) whenever it is updated:
        /// - 'message' event: will have the current state as json, sent every time the state is updated
        /// - 'subscriber:count' event: will have the number of subscribers, sent every time a subscriber is added and removed  
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResponseMessage AddBulletinBoardUpdateSubscriber(HttpRequestMessage request) {
            var response = EventSource.AddSubscriber(request);
            return response;
        }

        private static void SSE_SubscriberChanged(object sender, SubscriberEventArgs args) {
            EventSource.Send(args.SubscriberCount.ToString(), "subscriber:count");
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