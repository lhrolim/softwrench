﻿using System;
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
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class MultiItemBatchSubmissionService : IBatchSubmissionService {

        private const string MissingConverter = "missing batch submission converter for application {0}";
        private const string MultipleBatchConverters = "Multiple batch converters where found for application {0} schema {1}";

        private readonly ISWDBHibernateDAO _dao;
        private readonly MaximoConnectorEngine _maximoEngine;
        private IEnumerable<IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper>> _converters;
        private readonly IContextLookuper _contextLookuper;
        private readonly BatchReportEmailService _batchReportEmailService;
        private readonly BatchSubmissionProvider _submissionProvider;
        private readonly BatchConfigurerProvider _configurerProvider;


        public MultiItemBatchSubmissionService(ISWDBHibernateDAO dao, MaximoConnectorEngine maximoEngine, IContextLookuper contextLookuper, BatchReportEmailService batchReportEmailService,
            BatchSubmissionProvider submissionProvider, BatchConfigurerProvider configurerProvider) {
            _dao = dao;
            _maximoEngine = maximoEngine;
            _contextLookuper = contextLookuper;
            _batchReportEmailService = batchReportEmailService;
            _submissionProvider = submissionProvider;
            _configurerProvider = configurerProvider;
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

        public TargetResult Submit(MultiItemBatch multiItemBatch, JObject jsonOb = null, BatchOptions options = null) {
            var converter = _submissionProvider.LookupItem(multiItemBatch.Application, multiItemBatch.Schema,
                ApplicationConfiguration.ClientName);
            if (converter == null) {
                throw BatchConfigurationException.BatchNotFound(multiItemBatch.Application, multiItemBatch.Schema, ApplicationConfiguration.ClientName);
            }

            if (options == null) {
                var configurer = _configurerProvider.LookupItem(multiItemBatch.Application, multiItemBatch.Schema, ApplicationConfiguration.ClientName);
                options = configurer.GenerateOptions(multiItemBatch);
            }

            var applicationMetadata = MetadataProvider
            .Application(multiItemBatch.Application).ApplyPolicies(new ApplicationMetadataSchemaKey(multiItemBatch.Schema), SecurityFacade.CurrentUser(), ClientPlatform.Web);

            BatchReport report = null;
            var submissionData = BuildSubmissionData(multiItemBatch, jsonOb, converter, applicationMetadata);
            if (options.GenerateReport) {
                report = UpdateDBEntries(submissionData, multiItemBatch);
                if (report == null) {
                    //no report means that we don´t need to submit anything and the batch was deleted
                    return null;
                }
            } else {
                report = new BatchReport {
                    CreationDate = DateTime.Now,
                    OriginalMultiItemBatch = multiItemBatch,
                };
            }
            if (!options.Synchronous) {
                //new thread to give a fast response to the user
                Task.Factory.NewThread(array => DoExecuteBatch(submissionData, report, options), submissionData);
                return null;
            }
            return DoExecuteBatch(submissionData, report, options);

        }

        public TargetResult CreateAndSubmit(string application, string schema, JObject datamap, string itemids = "", string alias = null) {
            var userId = SecurityFacade.CurrentUser().DBId;
            var configurer = _configurerProvider.LookupItem(application, schema, ApplicationConfiguration.ClientName);

            var batch = new MultiItemBatch {
                Alias = alias,
                Application = application,
                Schema = schema,
                Status = BatchStatus.INPROG,
                CreationDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UserId = userId,
                ItemIds = itemids,
                DataMapJsonAsString = datamap.ToString()
            };
            var options = configurer.GenerateOptions(batch);
            if (options.GenerateReport) {
                //no need to store it if there´ll be no report stored
                batch = _dao.Save(batch);
            }
            return Submit(batch, datamap);
        }

        private BatchReport UpdateDBEntries(BatchSubmissionData submissionData, MultiItemBatch multiItemBatch) {
            if (!submissionData.ShouldSubmit()) {
                //this means that we have nothing to process for this batch, since the customer didnt close anything lets just delete it
                _dao.Delete(multiItemBatch);
                return null;
            }

            //some of the originally selected entries might be removed, so let´s update the batch entries (besides of the status and date)
            multiItemBatch.ItemIds = string.Join(",", submissionData.RemainingIds);
            multiItemBatch.DataMapJsonAsString = submissionData.RemainingArray.ToString();
            multiItemBatch.Status = BatchStatus.SUBMITTING;
            multiItemBatch.UpdateDate = DateTime.Now;
            _dao.Save(multiItemBatch);

            var report = new BatchReport {
                CreationDate = DateTime.Now,
                OriginalMultiItemBatch = multiItemBatch,
            };

            report = _dao.Save(report);
            return report;
        }


        private TargetResult DoExecuteBatch(BatchSubmissionData submissionData, BatchReport report, BatchOptions options) {
            var reportKey = report.GetReportKey();
            _contextLookuper.SetMemoryContext(reportKey, report);
            foreach (var itemToSubmit in submissionData.ItemsToSubmit) {
                try {
                    _maximoEngine.Execute(itemToSubmit.CrudData);
                    report.AppendSentItem(itemToSubmit.CrudData.Id);
                } catch (Exception e) {
                    if (options.GenerateProblems) {
                        var problem = new BatchItemProblem {
                            DataMapJsonAsString = itemToSubmit.OriginalLine.ToString(),
                            ErrorMessage = e.Message,
                            ItemId = itemToSubmit.CrudData.Id,
                            Report = report
                        };
                        problem = _dao.Save(problem);
                        if (report.ProblemItens == null) {
                            report.ProblemItens = new HashedSet<BatchItemProblem>();
                        }
                        report.ProblemItens.Add(problem);
                        _dao.Save(report);
                    } else {
                        throw e;
                    }
                }
            }
            if (options.GenerateReport) {
                _contextLookuper.RemoveFromMemoryContext(reportKey);
                _dao.Save(report);
                report.OriginalMultiItemBatch.Status = BatchStatus.COMPLETE;
                _dao.Save(report.OriginalMultiItemBatch);
                if (options.SendEmail) {
                    _batchReportEmailService.SendEmail(report);
                }
            }
            //TODO implement for synchronous result
            return new BatchResult(null);
        }

        private BatchSubmissionData BuildSubmissionData(MultiItemBatch batch, JObject jsonOb, IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper> converter, ApplicationMetadata applicationMetadata) {
            if (jsonOb == null) {
                jsonOb = JObject.Parse(batch.DataMapJsonAsString);
            }
            var submissionData = new BatchSubmissionData();
            var jArray = converter.BreakIntoRows(jsonOb);

            foreach (var row in jArray) {
                var r = (JObject)row;
                var fields = r.Property("fields");
                var originalLine = r;
                if (fields != null) {
                    originalLine = ((JObject)fields.Value);
                }
                var shouldSubmit = converter.ShouldSubmit(originalLine);
                if (!shouldSubmit) {
                    continue;
                }
                var crudData = converter.Convert(originalLine, applicationMetadata);
                submissionData.AddItem(new BatchSubmissionItem {
                    CrudData = crudData,
                    OriginalLine = originalLine
                });
                submissionData.RemainingArray.Add(row);
                submissionData.RemainingIds.Add(crudData.Id);
            }
            return submissionData;
        }

    }
}