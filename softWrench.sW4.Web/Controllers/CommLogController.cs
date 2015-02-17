using System.IO;
using System.Web.Http;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Web.Controllers {

    public class CommLogController : ApiController {

        private readonly SWDBHibernateDAO _swdbDao;

        public CommLogController(SWDBHibernateDAO dao) {
            _swdbDao = dao;
        }

        [HttpPost]
        public void UpdateReadFlag(string application, string applicationItemId, int userId, int commlogId) {
            var newCommEntry = new MaxCommReadFlag() {
                Application = application,
                ApplicationItemId = applicationItemId,
                CommlogId = commlogId,
                UserId = userId,
                ReadFlag = true
            };
            _swdbDao.Save(newCommEntry);
        }
    }
}
