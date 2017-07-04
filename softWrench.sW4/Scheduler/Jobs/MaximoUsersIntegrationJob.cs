using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleInjector;
using softWrench.sW4.Data.Entities.SyncManagers;
using cts.commons.simpleinjector.Core.Order;
using softWrench.sW4.Util;

namespace softWrench.sW4.Scheduler.Jobs {
    public class MaximoUsersIntegrationJob : ASwJob {
        private List<IUserSyncManager> _syncManagers;
        private readonly Container _container;


        public MaximoUsersIntegrationJob(Container container) {
            _container = container;
        }

        private void GetSyncManagers() {
            var syncManagers = new List<IUserSyncManager>(_container.GetAllInstances<IUserSyncManager>());
            OrderComparator<IUserSyncManager>.Sort(syncManagers);
            _syncManagers = syncManagers;
        }

        public override string Name() {
            return "Maximo users integration";
        }

        public override string Description() {
            return "Job to integrate maximo users";
        }

        public override string Cron() {
            return "0 0/15/30/45 * * * ?";
        }

        public override async Task ExecuteJob() {
            if (_syncManagers == null) {
                GetSyncManagers();
            }
            foreach (var userSyncManager in _syncManagers) {
                await userSyncManager.Sync();
            }
        }


        public override bool RunAtStartup() {
            return false;
        }
    }
}
