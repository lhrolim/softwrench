using softWrench.sW4.Scheduler;

namespace softwrench.sw4.activitystream.classes.Controller.Jobs {
    public class BulletinBoardStreamJob : ASwJob {

        public const string JobName = "Bulletin Board Update";

        private readonly BulletinBoardFacade _bulletinBoardFacade;

        public BulletinBoardStreamJob(BulletinBoardFacade bulletinBoardFacade) {
            _bulletinBoardFacade = bulletinBoardFacade;
        }

        public override string Name() {
            return "Bulletin Board Update";
        }

        public override string Description() {
            return "Updates In-Memory cache of active bulletinboards";
        }

        public override string Cron() {
            return _bulletinBoardFacade.GetBulletinBoardUpdateJobCron();
        }

        public override void ExecuteJob() {
            if (_bulletinBoardFacade.BulletinBoardEnabled) {
                _bulletinBoardFacade.UpdateInMemoryBulletinBoard();
            }
        }

        public override bool RunAtStartup() {
            return true;
        }
    }
}