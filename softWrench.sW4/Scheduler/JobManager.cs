using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Common.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using softWrench.sW4.Scheduler.Interfaces;
using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Scheduler {
    public class JobManager : ISWEventListener<ApplicationStartedEvent> {

        public const string JobUser = "swjobuser";
        public static EventLog Logger = new EventLog("SoftWrench");
        private ILog _log;
        private IScheduler _scheduler;
        private ISchedulerFactory _sf;
        private const string GroupName = "SoftWrenchJobGroup";
        private static IEnumerable<ISwJob> _jobs;
        private readonly SimpleInjectorJobFactory _jobFactory;
        public void Stop() {
            _scheduler.Standby();
        }

        public void Resume() {
            _scheduler.ResumeAll();
        }

        public void Shutdown() {
            _scheduler.Shutdown(true);
            Logger.WriteEntry("Job manager finished with success.", EventLogEntryType.Information);
        }

        public static IEnumerable<ISwJob> GetJobs() {
            return _jobs;
        }

        private static void GetAllJobs(IScheduler scheduler) {
            var jobGroups = scheduler.GetJobGroupNames();
            var triggerGroups = scheduler.GetTriggerGroupNames();

            foreach (var group in jobGroups) {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys) {
                    var detail = scheduler.GetJobDetail(jobKey);
                    var triggers = scheduler.GetTriggersOfJob(jobKey);
                    foreach (var trigger in triggers) {
                        Debug.WriteLine(group);
                        Debug.WriteLine(jobKey.Name);
                        Debug.WriteLine(detail.Description);
                        Debug.WriteLine(trigger.Key.Name);
                        Debug.WriteLine(trigger.Key.Group);
                        Debug.WriteLine(trigger.GetType().Name);
                        Debug.WriteLine(scheduler.GetTriggerState(trigger.Key));
                        var nextFireTime = trigger.GetNextFireTimeUtc();
                        if (nextFireTime.HasValue) {
                            Debug.WriteLine(
                                TimeZone.CurrentTimeZone.ToLocalTime(nextFireTime.Value.DateTime).ToString());
                        }

                        var previousFireTime = trigger.GetPreviousFireTimeUtc();
                        if (previousFireTime.HasValue) {
                            Debug.WriteLine(
                                TimeZone.CurrentTimeZone.ToLocalTime(previousFireTime.Value.DateTime).ToString());
                        }
                    }
                }
            }
        }

        public JobManager(IEnumerable<ISwJob> jobs, SimpleInjectorJobFactory jobFactory) {
            Logger.Source = "SoftWrench Job Engine";
            _jobs = jobs;
            _sf = new StdSchedulerFactory();
            _jobFactory = jobFactory;
        }

        public void StartJobs() {
            try {
                _log = LogManager.GetLogger(typeof(JobManager));

                _log.Info("------- Initializing ----------------------");

                var runTime = DateBuilder.EvenMinuteDate(DateTimeOffset.UtcNow);

                var jobs = GetJobs();

                foreach (var job in jobs) {

                    if (job.IsEnabled) {
                        _scheduler = _sf.GetScheduler();

                        _log.Info("------- Initialization Complete -----------");

                        IJobDetail jobDetail = new JobDetailImpl(job.Name(), GroupName, job.GetType());

                        var trigger =
                            (ICronTrigger)TriggerBuilder.Create()
                                .WithIdentity(job.Name() + "Trigger", GroupName)
                                .WithCronSchedule(job.Cron())
                                .Build();

                        _scheduler.ScheduleJob(jobDetail, trigger);
                        _log.Info(string.Format("{0} will run at: {1}", jobDetail.Key, runTime.ToString("r")));
                    }
                }
                _scheduler.JobFactory = _jobFactory;
                _scheduler.Start();

                _log.Info("------- Started Scheduler -----------------");
            } catch (Exception e) {
                _log.Error("error starting jobs", e);
                throw;
            }
        }

        #region Job Command

        public delegate void JobHandler(ISwJob job);

        public async Task ManageJobByCommand(string name, JobCommandEnum jobCommand, string cronExpression = null) {
            try {
                var jobs = GetJobs();

                foreach (var job in jobs) {
                    if (job.Name() != name) continue;

                    _sf = new StdSchedulerFactory();
                    _scheduler = _sf.GetScheduler();

                    switch (jobCommand) {
                        case JobCommandEnum.Execute:
                        Debug.WriteLine("executing job");
                        if (job.IsEnabled) {
                            await job.ExecuteJob();
                            break;
                        }
                        throw new Exception("Job {0} is disabled and cannot be started".Fmt(job.Name()));

                        case JobCommandEnum.Pause:
                        PauseJob(job);
                        break;

                        case JobCommandEnum.Schedule:
                        ScheduleJob(job);
                        break;

                        case JobCommandEnum.ChangeCron:
                        ChangeCron(job, cronExpression);
                        break;
                    }

                }
            } catch (Exception e) {
                _log.Error("error managing jobs", e);
                throw;
            }
        }

        private void PauseJob(ISwJob job) {
            var groupToPause = _scheduler.GetJobGroupNames();

            foreach (var group in groupToPause) {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys) {
                    var detail = _scheduler.GetJobDetail(jobKey);
                    if (detail.JobType != job.GetType()) continue;
                    var triggers = _scheduler.GetTriggersOfJob(jobKey);
                    foreach (var triggerPause in triggers) {
                        if (_scheduler.GetTriggerState(triggerPause.Key) != TriggerState.Normal) continue;
                        _scheduler.PauseTrigger(triggerPause.Key);
                        _scheduler.PauseJob(detail.Key);
                    }
                }
            }
        }

        private void ScheduleJob(ISwJob job) {
            var groupToResume = _scheduler.GetJobGroupNames();
            job.OnJobSchedule();
            foreach (var group in groupToResume) {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys) {
                    var detail = _scheduler.GetJobDetail(jobKey);
                    if (detail.JobType != job.GetType()) continue;
                    var triggers = _scheduler.GetTriggersOfJob(jobKey);
                    foreach (var triggerResume in triggers) {
                        if (_scheduler.GetTriggerState(triggerResume.Key) != TriggerState.Paused) continue;
                        _scheduler.ResumeTrigger(triggerResume.Key);
                        _scheduler.ResumeJob(detail.Key);
                    }
                }
            }
        }

        private void ChangeCron(ISwJob job, string cronExpression) {
            var groupToChangeCron = _scheduler.GetJobGroupNames();

            foreach (var group in groupToChangeCron) {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(@group);
                var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys) {
                    var detail = _scheduler.GetJobDetail(jobKey);
                    if (detail.JobType != job.GetType()) continue;
                    var triggers = _scheduler.GetTriggersOfJob(jobKey);
                    foreach (var triggerResume in triggers) {

                        var trigger = (ICronTrigger)TriggerBuilder.Create()
                            .WithIdentity(job.Name() + "Trigger", GroupName)
                            .WithCronSchedule(cronExpression)
                            .Build();

                        var currentState = _scheduler.GetTriggerState(triggerResume.Key);
                        _scheduler.RescheduleJob(triggerResume.Key, trigger);
                        if (currentState != TriggerState.Paused) continue;
                        _scheduler.PauseTrigger(trigger.Key);
                        _scheduler.PauseJob(detail.Key);
                    }
                }
            }
        }

        #endregion

        #region Jobs Info

        public struct JobsInfo {
            public string Name {
                get; set;
            }
            public string Description {
                get; set;
            }
            public string Cron {
                get; set;
            }
            public bool IsScheduled {
                get; set;
            }
        }

        public List<JobsInfo> GetJobsInfo() {
            var jobs = GetJobs();
            var jobsInfoList = new List<JobsInfo>();

            _sf = new StdSchedulerFactory();
            _scheduler = _sf.GetScheduler();

            foreach (var job in jobs) {

                var groupToResume = _scheduler.GetJobGroupNames();
                var cronExpression = string.Empty;
                foreach (var group in groupToResume) {
                    var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                    var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                    foreach (var jobKey in jobKeys) {
                        var detail = _scheduler.GetJobDetail(jobKey);
                        if (detail.JobType != job.GetType()) continue;
                        var triggers = _scheduler.GetTriggersOfJob(jobKey);
                        foreach (var trigger in triggers) {
                            var triggerResume = (ICronTrigger)trigger;
                            if (_scheduler.GetTriggerState(triggerResume.Key) == TriggerState.Paused) {
                                job.IsScheduled = false;
                            } else if (_scheduler.GetTriggerState(triggerResume.Key) == TriggerState.Normal) {
                                job.IsScheduled = true;
                            }
                            cronExpression = triggerResume.CronExpressionString;
                        }
                    }
                }

                var jobsInfo = new JobsInfo() {
                    Name = job.Name(),
                    Description = job.Description(),
                    Cron = cronExpression,
                    IsScheduled = job.IsScheduled
                };

                jobsInfoList.Add(jobsInfo);
            }
            return jobsInfoList;
        }

        #endregion

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            StartJobs();
        }
    }
}
