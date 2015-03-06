using System;
using System.Collections.Generic;
using System.Web.Http;
using cts.commons.web.Attributes;
using softWrench.sW4.Data.API;
using softWrench.sW4.Scheduler;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Controllers.Routing;
using softWrench.sW4.Web.Models.SchedulerSetup;
using softWrench.sW4.Web.SPF;

namespace softWrench.sW4.Web.Controllers.SchedulerSetup {
    public class SchedulerController : ApiController {
        private static JobManager _jobManager;

        public SchedulerController(JobManager jobManager) {
            _jobManager = jobManager;
        }

        [HttpGet]
        [SPFRedirect("Scheduler Setup", "_headermenu.schedulersetup")]
        public GenericResponseResult<List<SchedulerSetupModel>> Index() {
            var listToUse = GetMocketList();
            return new GenericResponseResult<List<SchedulerSetupModel>>(listToUse);
        }

        public static List<SchedulerSetupModel> GetMocketList() {
            var jobsInfoList = _jobManager.GetJobsInfo();
            var mocketList = new List<SchedulerSetupModel>();

            var id = 1;
            foreach (var jobInfo in jobsInfoList) {
                mocketList.Add(new SchedulerSetupModel(id, jobInfo.Name, jobInfo.Description, jobInfo.Cron, true, jobInfo.IsScheduled));
                id++;
            }
            return mocketList;
        }

        public GenericResponseResult<List<SchedulerSetupModel>> Get(string name, string jobCommand) {
            var jobCommandEnum = (JobCommandEnum)Enum.Parse(typeof(JobCommandEnum), jobCommand);
            _jobManager.ManageJobByCommand(name, jobCommandEnum);
            return GetResponse(jobCommandEnum);
        }

        public GenericResponseResult<List<SchedulerSetupModel>> Get(string name, string jobCommand, string cron) {
            var jobCommandEnum = (JobCommandEnum)Enum.Parse(typeof(JobCommandEnum), jobCommand);
            _jobManager.ManageJobByCommand(name, (JobCommandEnum)Enum.Parse(typeof(JobCommandEnum), jobCommand), cron);
            return GetResponse(jobCommandEnum);
        }

        private static GenericResponseResult<List<SchedulerSetupModel>> GetResponse(JobCommandEnum jobCommand) {
            var response = new GenericResponseResult<List<SchedulerSetupModel>> {
                ResultObject = GetMocketList(),
                SuccessMessage = SuccessMessageHandler.FillSuccessMessage(jobCommand)
            };
            return response;
        }
    }
}
