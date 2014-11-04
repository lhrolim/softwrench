using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public class BatchSubmissionService : ISingletonComponent {

        private const string MissingConverter = "missing batch submission converter for application {0}";

        private readonly SWDBHibernateDAO _dao;
        private readonly MaximoConnectorEngine _maximoEngine;
        private IEnumerable<ISubmissionConverter> _converters;


        public BatchSubmissionService(SWDBHibernateDAO dao, MaximoConnectorEngine maximoEngine) {
            _dao = dao;
            _maximoEngine = maximoEngine;
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

        public void Submit(Batch batch, JArray jsonOb) {
            var converter = Converters.FirstWithException(f => f.ApplicationName().EqualsIc(batch.Application), MissingConverter, batch.Application);
            var submissionData = BuildSubmissionData(jsonOb, converter);
            var report = UpdateDBEntries(submissionData, batch);
            if (report == null) {
                //no report means that we don´t need to submit anything and the batch was deleted
                return;
            }
            //new thread to give a fast response to the user
            SubmitItensOnNewThread(submissionData, report);
        }

        private BatchReport UpdateDBEntries(BatchSubmissionData submissionData, Batch batch) {
            if (!submissionData.ShouldSubmit()) {
                //this means that we have nothing to process for this batch, since the customer didnt close anything lets just delete it
                _dao.Delete(batch);
                return null;
            }

            //some of the originally selected entries might be removed, so let´s update the batch entries (besides of the status and date)
            batch.ItemIds = string.Join(",", submissionData.RemainingIds);
            batch.DataMapJsonAsString = submissionData.RemainingArray.ToString();
            batch.Status = BatchStatus.SUBMITTING;
            batch.UpdateDate = DateTime.Now;
            _dao.Save(batch);

            var report = new BatchReport {
                CreationDate = DateTime.Now,
                OriginalBatch = batch,
            };

            report = _dao.Save(report);
            return report;
        }

        private void SubmitItensOnNewThread(BatchSubmissionData submissionData, BatchReport report) {
            Task.Factory.NewThread(array => {
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
                _dao.Save(report);
                report.OriginalBatch.Status = BatchStatus.COMPLETE;
                _dao.Save(report.OriginalBatch);
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
