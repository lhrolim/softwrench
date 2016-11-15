using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sw4.batch.api;
using softwrench.sw4.batch.api.services;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector {
    public class KongsbergSolutionCrudConnector : BaseSolutionCrudConnector {
        private const string FindKeywords = "SELECT keyword, keyworddesc FROM keyword WHERE keyworddesc in (:p0)";
        private const string FindSolnKeywords = "SELECT solnkeywordid, keyword FROM solnkeyword WHERE solution = ?";

        // TODO findout the problem of deleting solnkeyword by webservice
        // WE NORMALLY !!!DO NOT!!! CHANGE DATA ON MAXIMO DB BY QUERY THIS IS A HUGE WORKAROUND CASE
        private const string DeleteSolnKeywords = "DELETE FROM solnkeyword WHERE {0}";

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            ManageKeywords(maximoTemplateData);
            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            ManageKeywords(maximoTemplateData);
            base.BeforeCreation(maximoTemplateData);
        }

        private static void ManageKeywords(MaximoOperationExecutionContext maximoTemplateData) {
            var solution = maximoTemplateData.IntegrationObject;
            var operationData = maximoTemplateData.OperationData;

            var attributes = operationData.Holder;
            object keywordsObj;
            attributes.TryGetValue("allkeywords", out keywordsObj);
            object solutionObj;
            attributes.TryGetValue("solution", out solutionObj);


            var keywordDescriptions = ParseKeywords(keywordsObj);
            var keywords = new List<string>();
            var solnKeywordsToDelete = new List<string>();

            // get the existing keywords to avoid create the same keyword again
            // keywords are reused by different solutions
            GetExistingKeywords(keywordDescriptions, keywords);

            // get the existing solnkeywords (table between solution and keywords)
            // to know the solnkeyword that are needed to create and delete
            GetExistingSolnKeywords(keywords, solutionObj, solnKeywordsToDelete);

            // create the needed keywords
            CreateKeywords(keywordDescriptions, keywords);

            // create the needed solnkeywords
            CreateSolnKeywords(solution, keywords);

            // delete the uneeded solnkeywords
            DeleteUnusedSolnKeywords(solnKeywordsToDelete);
        }

        private static List<string> ParseKeywords(object keywords) {
            var keywordList = new List<string>();
            if (keywords == null) {
                return keywordList;
            }

            var keywordsStr = keywords as string;
            if (string.IsNullOrEmpty(keywordsStr)) {
                return keywordList;
            }

            keywordsStr.Split(',').ForEach(keyword => {
                var afterTrim = keyword.Trim();
                if (!string.IsNullOrEmpty(afterTrim)) {
                    keywordList.Add(afterTrim);
                }
            });

            return keywordList;
        }

        private static IMaximoHibernateDAO GetDao() {
            return SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>();
        }

        private static void GetExistingKeywords(ICollection<string> keywordsDescription, ICollection<string> keywords) {
            if (keywordsDescription == null || keywordsDescription.Count == 0) {
                return;
            }

            var keywordsFound = GetDao().FindByNativeQuery(FindKeywords, keywordsDescription);

            if (keywordsFound == null || keywordsFound.Count == 0) {
                return;
            }

            keywordsFound.ForEach(columns => {
                keywords.Add(columns["keyword"]);
                keywordsDescription.Remove(columns["keyworddesc"]);
            });
        }

        private static void GetExistingSolnKeywords(ICollection<string> keywords, object solutionObj, List<string> solnKeywordsToDelete) {
            var solution = solutionObj as string;
            if (string.IsNullOrEmpty(solution)) {
                return;
            }

            var keywordsFound = GetDao().FindByNativeQuery(FindSolnKeywords, solution);

            if (keywordsFound == null || keywordsFound.Count == 0) {
                return;
            }

            keywordsFound.ForEach(columns => {
                var keyword = columns["keyword"];
                var solnKeywordId = columns["solnkeywordid"];

                if (keywords.Contains(keyword)) {
                    keywords.Remove(keyword);
                } else {
                    solnKeywordsToDelete.Add(solnKeywordId);
                }
            });
        }

        private static void CreateKeywords(List<string> descriptions, ICollection<string> keywords) {
            if (descriptions == null || descriptions.Count == 0) {
                return;
            }

            var datamap = CreateKeywordsJson(descriptions, keywords);
            var batchSubmissionService = SimpleInjectorGenericFactory.Instance.GetObject<IBatchSubmissionService>();
            batchSubmissionService.CreateAndSubmit("keyword", "detail", datamap, "", null, GetBatchOptions());
        }

        private static void CreateSolnKeywords(object solution, List<string> keywords) {
            if (keywords == null || keywords.Count == 0) {
                return;
            }

            var solnkeywords = new List<CrudOperationData>();
            var solnkeywordEntity = MetadataProvider.Entity("solnkeyword");
            var solnkeywordid = EntityRepository.GetNextEntityId(solnkeywordEntity);
            keywords.ForEach(keyword => {
                var attributes = new Dictionary<string, object> {
                    {"solnkeywordid", solnkeywordid},
                    {"keyword", keyword}
                };
                solnkeywords.Add(new CrudOperationData(solnkeywordid.ToString(), attributes, new Dictionary<string, object>(), solnkeywordEntity, null));
                solnkeywordid += EntityRepository.GetRandomIncrement();
            });

            w.CloneArray(solnkeywords, solution, "SOLNKEYWORD", delegate (object integrationObject, CrudOperationData crudData) {
                w.SetValueIfNull(integrationObject, "solnkeywordid", crudData.GetAttribute("solnkeywordid"));
                w.CopyFromRootEntity(solution, integrationObject, "solution", null);
                w.SetValueIfNull(integrationObject, "keyword", crudData.GetAttribute("keyword"));
            });
        }

        private static void DeleteUnusedSolnKeywords(IReadOnlyCollection<string> solnKeywordsToDelete) {
            if (solnKeywordsToDelete == null || solnKeywordsToDelete.Count == 0) {
                return;
            }

            var conditionList = solnKeywordsToDelete.Select(id => string.Format("solnkeywordid = {0}", id));
            var conditionToken = string.Join(" OR ", conditionList);
            var query = string.Format(DeleteSolnKeywords, conditionToken);
            GetDao().ExecuteSql(query);
        }

        private static JObject CreateKeywordsJson(List<string> descriptions, ICollection<string> keywords) {
            var array = new JArray();

            var keywordEntity = MetadataProvider.Entity("keyword");
            var baseKeywordid = EntityRepository.GetNextEntityId(keywordEntity);
            var baeKeyword = EntityRepository.GetNextEntityId(keywordEntity, "keyword");
            var i = (int)0;

            descriptions.ForEach(description => {
                var keywordId = baseKeywordid + i;
                var keyword = (baeKeyword + i).ToString();
                keywords.Add(keyword);
                i += EntityRepository.GetRandomIncrement();
                var keywordJson = new JObject { { "keyword", keyword }, { "keyworddesc", description }, { "keywordid", keywordId } };
                array.Add(keywordJson);
            });

            return new JObject { { "#keywordlist_", array } };
        }

        private static BatchOptions GetBatchOptions() {
            return new BatchOptions() {
                GenerateProblems = false,
                SendEmail = false,
                Synchronous = true
            };
        }
    }
}