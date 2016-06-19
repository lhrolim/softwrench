using System;
using System.Collections.Generic;
using cts.commons.persistence;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.batch.api.services;
using softwrench.sW4.audit.classes.Services.Batch;
using softwrench.sW4.audit.classes.Services.Batch.Data;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchItemSubmissionService : ISingletonComponent {


        private readonly ISWDBHibernateDAO _dao;
        private readonly MaximoConnectorEngine _maximoEngine;
        private IEnumerable<IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>> _converters;
        private readonly IContextLookuper _contextLookuper;
        private readonly ProblemManager _problemManager;
        private readonly AuditPostBatchHandlerProvider _auditPostBatchHandlerProvider;


        public BatchItemSubmissionService(ISWDBHibernateDAO dao, MaximoConnectorEngine maximoEngine, IContextLookuper contextLookuper, ProblemManager problemManager, AuditPostBatchHandlerProvider auditPostBatchHandlerProvider) {
            _dao = dao;
            _maximoEngine = maximoEngine;
            _contextLookuper = contextLookuper;
            _problemManager = problemManager;
            _auditPostBatchHandlerProvider = auditPostBatchHandlerProvider;
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
            var user = SecurityFacade.CurrentUser();
            _contextLookuper.SetMemoryContext(batch.RemoteId, batch, true);

            var mockMaximo = _contextLookuper.LookupContext().MockMaximo;

            if (!mockMaximo) {
                var auditPostBatchHandler = _auditPostBatchHandlerProvider.LookupItem(batch.Application, "detail",
                    ApplicationConfiguration.ClientName);

                foreach (var itemToSubmit in submissionData.ItemsToSubmit) {
                    var originalItem = itemToSubmit.OriginalItem;
                    try {
                        var result = _maximoEngine.Execute(itemToSubmit.CrudData);
                        batch.TargetResults.Add(result);
                        if (originalItem.RemoteId != null) {
                            batch.SuccessItems.Add(originalItem.RemoteId);
                        }
                        if (originalItem.AdditionalData != null) {
                            auditPostBatchHandler.HandlePostBatchAuditData(new AuditPostBatchData(result,
                                originalItem.AdditionalData));
                        }


                    } catch (Exception e) {
                        if (options.GenerateProblems) {
                            var problemDataMap = originalItem.Id == null ? null : originalItem.DataMapJsonAsString;
                            var problem = _problemManager.Register(originalItem.Application, originalItem.ItemId, itemToSubmit.CrudData.UserId,
                                problemDataMap, user.DBId, e.StackTrace, e.Message, typeof(BatchItem).Name);
                            batch.Problems.Add(originalItem.RemoteId, problem);
                        } else {
                            throw;
                        }
                    }
                }
            }

            batch.Status = BatchStatus.COMPLETE;
            if (options.Synchronous) {
                //if asynchronous then the removal should be performed by the polling service
                _contextLookuper.RemoveFromMemoryContext(batch.RemoteId, true);
            } else {
                _dao.Save(batch);
            }
            return batch;
        }

        private BatchSubmissionData BuildSubmissionData(Batch batch) {

            var submissionData = new BatchSubmissionData();
            //var jArray = new JArray();
            var user = SecurityFacade.CurrentUser();
            foreach (var item in batch.Items) {
                //jArray.Add(item.DataMapJSonObject);

                var applicationMetadata = user.CachedSchema(item.Application, new ApplicationMetadataSchemaKey(item.Schema, SchemaMode.None, batch.Platform ?? ClientPlatform.Mobile));
                var entityMetadata = MetadataProvider.Entity(applicationMetadata.Entity);

                CrudOperationData crudOperationData;

                if (item.Fields != null) {
                    crudOperationData = new CrudOperationData(item.ItemId, item.Fields, new Dictionary<string, object>(), entityMetadata, applicationMetadata);
                } else {
                    crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata, applicationMetadata, item.DataMapJSonObject, item.ItemId);
                }


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
