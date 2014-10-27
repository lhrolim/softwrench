using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.controller {
    public class BatchController : ApiController {

        private readonly SWDBHibernateDAO _dao;

        public BatchController(SWDBHibernateDAO dao) {
            _dao = dao;
        }



        [HttpPost]
        public IGenericResponseResult Create(string application, string schema, string alias, JObject jsonIds) {
            //TODO: add id checkings on server side
            var userId = SecurityFacade.CurrentUser().DBId;
            var batch = new Batch {
                Alias = alias,
                Application = application,
                Schema = schema,
                Status = "INPROG",
                CreationDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UserId = userId,
                ItemIds = jsonIds["ids"].ToString(),
            };
            var saved = _dao.Save(batch);
            return new GenericResponseResult<Batch>(saved);
        }

        public void Update(Int32 batchId, JObject datamap) {
            var batch = _dao.FindByPK<Batch>(typeof(Batch), batchId);
            if (batch == null) {
                throw BatchException.BatchNotFound(batchId);
            }
            batch.DataMapJsonAsString = datamap["datamap"].ToString();
            batch.UpdateDate = DateTime.Now;
            _dao.Save(batch);
        }

    }
}
