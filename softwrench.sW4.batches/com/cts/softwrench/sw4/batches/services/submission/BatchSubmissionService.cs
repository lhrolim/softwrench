using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.Util;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.report;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchSubmissionService : ISingletonComponent {

        private const string MissingConverter = "missing batch submission converter for application {0}";

        private readonly SWDBHibernateDAO _dao;
        private readonly MaximoConnectorEngine _maximoEngine;
        private IEnumerable<ISubmissionConverter> _converters;
        private readonly IContextLookuper _contextLookuper;
        private readonly BatchReportEmailService _batchReportEmailService;


        public BatchSubmissionService(SWDBHibernateDAO dao, MaximoConnectorEngine maximoEngine, IContextLookuper contextLookuper, BatchReportEmailService batchReportEmailService) {
            _dao = dao;
            _maximoEngine = maximoEngine;
            _contextLookuper = contextLookuper;
            _batchReportEmailService = batchReportEmailService;
        }

        public IEnumerable<ISubmissionConverter> Converters {
            get {
                if (_converters != null) {
                    return _converters;
                }
                _converters = SimpleInjectorGenericFactory.Instance.GetObjectsOfType<ISubmissionConverter>(typeof(ISubmissionConverter));
                return _converters;
            }
        }

        public void Submit(MultiItemBatch _multiItemBatch, JArray jsonOb) {
            var converter = Converters.FirstWithException(f => f.ApplicationName().EqualsIc(_multiItemBatch.Application), MissingConverter, _multiItemBatch.Application);
            var submissionData = BuildSubmissionData(jsonOb, converter);
            var report = UpdateDBEntries(submissionData, _multiItemBatch);
            if (report == null) {
                //no report means that we don´t need to submit anything and the batch was deleted
                return;
            }
            //new thread to give a fast response to the user
            SubmitItensOnNewThread(submissionData, report);
        }

        private BatchReport UpdateDBEntries(BatchSubmissionData submissionData, MultiItemBatch _multiItemBatch) {
            if (!submissionData.ShouldSubmit()) {
                //this means that we have nothing to process for this batch, since the customer didnt close anything lets just delete it
                _dao.Delete(_multiItemBatch);
                return null;
            }

            //some of the originally selected entries might be removed, so let´s update the batch entries (besides of the status and date)
            _multiItemBatch.ItemIds = string.Join(",", submissionData.RemainingIds);
            _multiItemBatch.DataMapJsonAsString = submissionData.RemainingArray.ToString();
            _multiItemBatch.Status = BatchStatus.SUBMITTING;
            _multiItemBatch.UpdateDate = DateTime.Now;
            _dao.Save(_multiItemBatch);

            var report = new BatchReport {
                CreationDate = DateTime.Now,
                OriginalMultiItemBatch = _multiItemBatch,
            };

            report = _dao.Save(report);
            return report;
        }

        private void SubmitItensOnNewThread(BatchSubmissionData submissionData, BatchReport report) {
            Task.Factory.NewThread(array => {
                var reportKey = "sw_batchreport{0}".Fmt(report.OriginalMultiItemBatch.Id);
                _contextLookuper.SetMemoryContext(reportKey, report);
                foreach (var itemToSubmit in submissionData.ItemsToSubmit) {
                    try {
                        _maximoEngine.Update(itemToSubmit.CrudData);
                        report.AppendSentItem(itemToSubmit.CrudData.Id);
                    } catch (Exception e) {
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
                    }
                }
                _contextLookuper.RemoveFromMemoryContext(reportKey);
                _dao.Save(report);
                report.OriginalMultiItemBatch.Status = BatchStatus.COMPLETE;
                _dao.Save(report.OriginalMultiItemBatch);
                _batchReportEmailService.SendEmail(report);
            }, submissionData);
        }

        private BatchSubmissionData BuildSubmissionData(IEnumerable<JToken> jsonOb, ISubmissionConverter converter) {
            var submissionData = new BatchSubmissionData();
            foreach (var row in jsonOb) {
                var r = (JObject)row;
                var fields = r.Property("fields");
                if (fields == null) {
                    continue;
                }
                var originalLine = ((JObject)fields.Value);
                var shouldSubmit = converter.ShouldSubmit(originalLine);
                if (!shouldSubmit) {
                    continue;
                }
                var crudData = converter.Convert(originalLine);
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
