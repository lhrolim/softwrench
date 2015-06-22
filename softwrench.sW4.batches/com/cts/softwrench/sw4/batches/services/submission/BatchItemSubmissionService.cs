using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.controller;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.exception;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.report;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchItemSubmissionService : ISingletonComponent {

        private const string MissingConverter = "missing batch submission converter for application {0}";
        private const string MultipleBatchConverters = "Multiple batch converters where found for application {0} schema {1}";

        private readonly ISWDBHibernateDAO _dao;
        private readonly MaximoConnectorEngine _maximoEngine;
        private IEnumerable<IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>> _converters;
        private readonly IContextLookuper _contextLookuper;
        private readonly BatchReportEmailService _batchReportEmailService;
        private readonly ProblemManager _problemManager;


        public BatchItemSubmissionService(ISWDBHibernateDAO dao, MaximoConnectorEngine maximoEngine, IContextLookuper contextLookuper, BatchReportEmailService batchReportEmailService,
            BatchSubmissionProvider submissionProvider, BatchConfigurerProvider configurerProvider, ProblemManager problemManager) {
            _dao = dao;
            _maximoEngine = maximoEngine;
            _contextLookuper = contextLookuper;
            _batchReportEmailService = batchReportEmailService;
            _problemManager = problemManager;
        }

        public IEnumerable<IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>> Converters {
            get {
                if (_converters != null) {
                    return _converters;
                }
                _converters = SimpleInjectorGenericFactory.Instance.GetObjectsOfType<IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>>(typeof(IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>));
                return _converters;
            }
        }

        public Batch Submit(Batch batch, BatchOptions options) {
            var submissionData = BuildSubmissionData(batch);
            var reportKey = batch.RemoteId;
            _contextLookuper.SetMemoryContext(reportKey, batch);
            foreach (var itemToSubmit in submissionData.ItemsToSubmit) {
                try {
                    _maximoEngine.Execute(itemToSubmit.CrudData);
                    batch.SuccessItems.Add(itemToSubmit.OriginalItem.RemoteId);
                } catch (Exception e) {
                    if (options.GenerateProblems) {
                        //                        _problemManager.Register();
                        //                        var problem = new BatchItemProblem {
                        //                            DataMapJsonAsString = itemToSubmit.OriginalLine.ToString(),
                        //                            ErrorMessage = e.Message,
                        //                            ItemId = itemToSubmit.CrudData.Id,
                        //                            Report = report
                        //                        };
                        //                        problem = _dao.Save(problem);
                        //                        if (report.ProblemItens == null) {
                        //                            report.ProblemItens = new HashedSet<BatchItemProblem>();
                        //                        }
                        //                        report.ProblemItens.Add(problem);
                        //                        _dao.Save(report);
                    } else {
                        throw e;
                    }
                }
            }
            if (options.GenerateReport) {
                //                _contextLookuper.RemoveFromMemoryContext(reportKey);
                //                _dao.Save(report);
                //                report.OriginalMultiItemBatch.Status = BatchStatus.COMPLETE;
                //                _dao.Save(report.OriginalMultiItemBatch);
                //                if (options.SendEmail) {
                //                    _batchReportEmailService.SendEmail(report);
                //                }
            }
            //TODO implement for synchronous result
            return batch;
        }

        private BatchSubmissionData BuildSubmissionData(Batch batch) {

            var submissionData = new BatchSubmissionData();
            var jArray = new JArray();
            var user = SecurityFacade.CurrentUser();
            foreach (var item in batch.Items) {
                jArray.Add(item.DataMapJSonObject);

                var applicationMetadata = user.CachedSchema(item.Application, new ApplicationMetadataSchemaKey(item.Schema, SchemaMode.None, ClientPlatform.Mobile));
                var entityMetadata = MetadataProvider.Entity(applicationMetadata.Entity);

                var crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata, applicationMetadata, item.DataMapJSonObject, null);
                var wrapper = new OperationWrapper(crudOperationData, item.Operation);

                submissionData.AddItem(new BatchSubmissionItem {
                    CrudData = wrapper,
                    OriginalLine = item.DataMapJSonObject,
                    OriginalItem = item
                });

                submissionData.RemainingArray.Add(item.DataMapJSonObject);
                submissionData.RemainingIds.Add(item.ItemId);
            }
            return submissionData;
        }

    }
}
