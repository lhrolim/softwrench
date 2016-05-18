using System;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.controller {
    public class BatchController : ApiController {


        private readonly SWDBHibernateDAO _dao;
        private readonly MultiItemBatchSubmissionService _submissionService;


        public BatchController(SWDBHibernateDAO dao, MultiItemBatchSubmissionService submissionService) {
            _dao = dao;
            _submissionService = submissionService;
        }


        [HttpPost]
        public IGenericResponseResult Create(string application, string schema, string alias, JObject jsonIds) {
            //TODO: add id checkings on server side
            var userId = SecurityFacade.CurrentUser().DBId;
            var batch = new MultiItemBatch {
                Alias = alias,
                Application = application,
                Schema = schema,
                Status = BatchStatus.INPROG,
                CreationDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UserId = userId,
                ItemIds = jsonIds["ids"].ToString(),
            };
            var saved = _dao.Save(batch);
            return new GenericResponseResult<MultiItemBatch>(saved);
        }

        public void Update(Int32 batchId, JObject datamap) {
            var batch = _dao.FindByPK<MultiItemBatch>(typeof(MultiItemBatch), batchId);
            if (batch == null) {
                throw BatchException.BatchNotFound(batchId);
            }
            batch.DataMapJsonAsString = datamap["datamap"].ToString();
            batch.UpdateDate = DateTime.Now;
            _dao.Save(batch);
        }

        public void Submit(Int32 batchId, JObject datamap) {
            var batch = _dao.FindByPK<MultiItemBatch>(typeof(MultiItemBatch), batchId);
            if (batch == null) {
                throw BatchException.BatchNotFound(batchId);
            }

            _submissionService.Submit(batch, datamap);


        }

        [SPFRedirect(URL = "BatchReport")]
        public IGenericResponseResult ViewReport(Int32 batchId) {
            var batchReport = _dao.FindSingleByQuery<BatchReport>(BatchReport.ByBatchId, batchId);
            if (batchReport == null) {
                throw BatchException.BatchReportNotFound(batchId);
            }
            return new GenericResponseResult<BatchReport>(batchReport);
        }

    }
}
