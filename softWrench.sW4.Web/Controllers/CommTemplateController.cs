using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softWrench.sW4.Web.Controllers {
    public class CommTemplateController : ApiController {

        private readonly MaximoHibernateDAO _maximoDao;

        public CommTemplateController(MaximoHibernateDAO dao) {
            _maximoDao = dao;
        }

        [HttpPost]
        public IApplicationResponse MergeTemplateDefinition(string templateId, string applicationName, string applicationId) {
            var commtemplate = _maximoDao.FindByNativeQuery(
                string.Format("SELECT * FROM COMMTEMPLATE WHERE templateid = '{0}' and objectname = '{1}'", templateId, applicationName));

            return new BlankApplicationResponse();
        }
    }
}
