﻿using System;
using System.Collections.Generic;
using cts.commons.persistence;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchItemSubmissionService : ISingletonComponent {


        private readonly ISWDBHibernateDAO _dao;
        private readonly MaximoConnectorEngine _maximoEngine;
        private IEnumerable<IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>> _converters;
        private readonly IContextLookuper _contextLookuper;
        private readonly ProblemManager _problemManager;


        public BatchItemSubmissionService(ISWDBHibernateDAO dao, MaximoConnectorEngine maximoEngine, IContextLookuper contextLookuper, ProblemManager problemManager) {
            _dao = dao;
            _maximoEngine = maximoEngine;
            _contextLookuper = contextLookuper;
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
            var user = SecurityFacade.CurrentUser();
            _contextLookuper.SetMemoryContext(batch.RemoteId, batch);
            foreach (var itemToSubmit in submissionData.ItemsToSubmit) {
                var originalItem = itemToSubmit.OriginalItem;
                try {
                    _maximoEngine.Execute(itemToSubmit.CrudData);
                    batch.SuccessItems.Add(originalItem.RemoteId);
                } catch (Exception e) {
                    if (options.GenerateProblems) {
                        var problemDataMap = originalItem.Id == null ? null : originalItem.DataMapJsonAsString;
                        var problem = _problemManager.Register(typeof(BatchItem).Name, "" + originalItem.Id, problemDataMap, user.DBId, e.StackTrace, e.Message);
                        batch.Problems.Add(originalItem.RemoteId, problem);
                    } else {
                        throw;
                    }
                }
            }
            batch.Status = BatchStatus.COMPLETE;
            if (options.Synchronous) {
                //if asynchronous then the removal should be performed by the polling service
                _contextLookuper.RemoveFromMemoryContext(batch.RemoteId);
            } else {
                _dao.Save(batch);
            }
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
