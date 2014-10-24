using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.controller {
    public class BatchController : ApiController {

        private readonly SWDBHibernateDAO _dao;

        public BatchController(SWDBHibernateDAO dao) {
            _dao = dao;
        }



        [HttpPost]
        public void Create(string application, string schema, string alias,string ids) {
            var userId = SecurityFacade.CurrentUser().DBId;
            var batch = new Batch {
                Alias = alias,
                Application = application,
                Schema = schema,
                CreationDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UserId = userId,
                ItemIds = ids,
            };
            _dao.Save(batch);
        }
    }
}
