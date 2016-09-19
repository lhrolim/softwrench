using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Scheduler;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Controllers.Routing;
using softWrench.sW4.Web.Models.SchedulerSetup;

namespace softWrench.sW4.Web.Controllers.SchedulerSetup {
    public class SchedulerController : ApiController {
        private static JobManager _jobManager;

        public SchedulerController(JobManager jobManager) {
            _jobManager = jobManager;
        }

        [HttpGet]
        [SPFRedirect("Scheduler", "_headermenu.schedulersetup")]
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

        public async Task<GenericResponseResult<List<SchedulerSetupModel>>> Get(string name, string jobCommand) {
            var jobCommandEnum = (JobCommandEnum)Enum.Parse(typeof(JobCommandEnum), jobCommand);
            await _jobManager.ManageJobByCommand(name, jobCommandEnum);
            return GetResponse(jobCommandEnum);
        }

        public async Task<GenericResponseResult<List<SchedulerSetupModel>>> Get(string name, string jobCommand, string cron) {
            var jobCommandEnum = (JobCommandEnum)Enum.Parse(typeof(JobCommandEnum), jobCommand);
            await _jobManager.ManageJobByCommand(name, (JobCommandEnum)Enum.Parse(typeof(JobCommandEnum), jobCommand), cron);
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
