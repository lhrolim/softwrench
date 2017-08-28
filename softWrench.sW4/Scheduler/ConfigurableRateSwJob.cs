﻿using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.api.classes.configuration;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Scheduler.Interfaces;

namespace softWrench.sW4.Scheduler {
    public abstract class ConfigurableRateSwJob : ASwJob, ISWEventListener<ConfigurationChangedEvent> {

        private readonly IConfigurationFacade _configurationFacade;
        private readonly JobManager _jobManager;

        protected ConfigurableRateSwJob(IConfigurationFacade configurationFacade, JobManager jobManager) {
            _configurationFacade = configurationFacade;
            _jobManager = jobManager;
        }

        public override string Cron() {
            return GetCron();
        }

        protected string GetCron(long? refreshRate = null) {
            var rate = refreshRate ?? _configurationFacade.Lookup<long>(JobConfigKey);
            return string.Format("0 */{0} * ? * *", rate);
        }

        public async void HandleEvent(ConfigurationChangedEvent eventToDispatch) {
            if (eventToDispatch.ConfigKey.EqualsIc(JobConfigKey)) {
                var refreshRate = long.Parse(eventToDispatch.CurrentValue);
                await UpdateJob(refreshRate);
            }
        }

        public override void OnJobSchedule() {
            if (!IsEnabled) {
                throw new SwJobException("Cannot start {0} cause no {1} was set. Check Configuration Application", Name(), JobConfigKey);
            }
        }

        private async Task UpdateJob(long refreshRate) {
            var newCron = GetCron(refreshRate);
            await _jobManager.ManageJobByCommand(Name(), JobCommandEnum.ChangeCron, newCron);
        }
        /// <summary>
        /// The configuration Key that controls the rate of the job execution
        /// </summary>
        protected abstract string JobConfigKey {
            get;
        }

    }
}
