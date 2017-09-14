using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarWorkPackageCompositionHandler : ISingletonComponent, ISWEventListener<ApplicationStartedEvent>, ISWEventListener<ContainerReloadedEvent> {

        public static readonly ApplicationMetadataSchemaKey CompositionSchemaKey = new ApplicationMetadataSchemaKey("workpackageschema", SchemaMode.None, ClientPlatform.Web);

        private const string UnknownSource = "Unknown";
        private const string RelayEventAttachmentSource = "Relay Event File";
        private const string WalkDownPhotoAttachmentSource = "Walk Down Photo";
        private const string CallOutEmailAttachmentSource = "Subcontractor Email Attachment";
        private const string MaintenanceEngEmailAttachmentSource = "Maintenance Engineering Email Attachment";
        private const string DomEmailAttachmentSource = "Daily Outage Meeting Email Attachment";
        private const string InterconnectDocsAttachmentSource = "Interconnect Document";
        private const string OperationProcAttachmentSource = "Operation Procedure File";

        private readonly Dictionary<string, string> _testsI18NDict = new Dictionary<string, string>();

        public void HandleWorkLogs(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var wkpkgWorkLogs = woResult.ResultObject.FirstOrDefault(pair => FSWPackageConstants.WorklogsRelationship.Equals(pair.Key)).Value;

            if (wkpkgWorkLogs == null) {
                //might be null due to security restrictions
                return;
            }


            var workLogMap = new Dictionary<string, IList<Dictionary<string, object>>>();
            wkpkgWorkLogs.ResultList.ForEach(worklog => {
                var description = worklog["description"].ToString();
                var realRelationship = "#" + description.Substring(7) + "s_";
                if (!workLogMap.ContainsKey(realRelationship)) {
                    workLogMap.Add(realRelationship, new List<Dictionary<string, object>>());
                }
                workLogMap[realRelationship].Add(worklog);
            });

            workLogMap.ForEach(pair => {
                var searchResult = new EntityRepository.SearchEntityResult {
                    ResultList = pair.Value,
                    IdFieldName = wkpkgWorkLogs.IdFieldName,
                    PaginationData = wkpkgWorkLogs.PaginationData
                };
                packageResult.ResultObject.Add(pair.Key, searchResult);
            });
        }

        public void HandleAttachments(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var wkpkgAttachs = woResult.ResultObject.FirstOrDefault(pair => FSWPackageConstants.AttachsRelationship.Equals(pair.Key)).Value;
            if (wkpkgAttachs == null) {
                //might be null due to security restrictions
                return;
            }

            var attachsMap = new Dictionary<string, IList<Dictionary<string, object>>>();
            wkpkgAttachs.ResultList.ForEach(attach => {
                var filter = attach["docinfo_.urlparam1"].ToString().ToLower();
                var realRelationship = "#" + filter.Substring(7) + "fileexplorer_";
                if (!attachsMap.ContainsKey(realRelationship)) {
                    attachsMap.Add(realRelationship, new List<Dictionary<string, object>>());
                }
                attachsMap[realRelationship].Add(attach);
            });

            attachsMap.ForEach(pair => {
                var searchResult = new EntityRepository.SearchEntityResult {
                    ResultList = pair.Value,
                    IdFieldName = wkpkgAttachs.IdFieldName,
                    PaginationData = wkpkgAttachs.PaginationData
                };
                packageResult.ResultObject.Add(pair.Key, searchResult);
            });
        }

        public void HandleRelatedWos(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var relatedWos = woResult.ResultObject.FirstOrDefault(pair => FSWPackageConstants.RelatedWorkOrdersRelationship.Equals(pair.Key)).Value;

            if (relatedWos == null) {
                //might be null due to security restrictions
                return;
            }


            var relatedWosResult = new List<Dictionary<string, object>>();
            relatedWos.ResultList.ForEach(relatedWo => {
                var relatedWoMap = new Dictionary<string, object>();
                relatedWo.ForEach(pair => {
                    relatedWoMap.Add("#" + pair.Key, pair.Value);
                });

                // workaround to show expand button
                relatedWoMap.Add("id", 1);

                relatedWosResult.Add(relatedWoMap);
            });

            var searchResult = new EntityRepository.SearchEntityResult {
                ResultList = relatedWosResult,
                IdFieldName = relatedWos.IdFieldName,
                PaginationData = relatedWos.PaginationData
            };
            packageResult.ResultObject.Add("#" + FSWPackageConstants.RelatedWorkOrdersRelationship.Substring(4), searchResult);
        }

        public void HandleAttachmentsTab(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var allAttachs = woResult.ResultObject.FirstOrDefault(pair => FSWPackageConstants.AllAttachmentsRelationship.Equals(pair.Key)).Value;
            if (allAttachs == null) {
                //could be missing due to security profile restrictions
                return;
            }


            var attachList = new List<Dictionary<string, object>>();
            allAttachs.ResultList.ForEach(attach => {
                attach["#attachdocument"] = attach["docinfo_.description"];
                attach["#attachcreateddate"] = attach["createdate"];
                attach["id"] = attach["doclinksid"]; // workaround to show expand button


                var baseFilter = attach["docinfo_.urlparam1"];
                if (baseFilter == null) {
                    attach["#attachsource"] = UnknownSource;
                    attachList.Add(attach);
                    return;
                }

                var filter = baseFilter.ToString().ToLower();
                if (filter.StartsWith("swwpkg:")) {
                    var test = filter.Substring(7);

                    if ("relayevent".Equals(test)) {
                        attach["#attachsource"] = RelayEventAttachmentSource;
                    } else if ("walkdown".Equals(test)) {
                        attach["#attachsource"] = WalkDownPhotoAttachmentSource;
                    } else if ("interconnect".Equals(test)) {
                        attach["#attachsource"] = InterconnectDocsAttachmentSource;
                    } else if ("operationproc".Equals(test)) {
                        attach["#attachsource"] = OperationProcAttachmentSource;
                    } else {
                        attach["#attachsource"] = _testsI18NDict.ContainsKey(test) ? _testsI18NDict[test] : UnknownSource;
                    }
                } else if (filter.StartsWith("swwpkgco:")) {
                    attach["#attachsource"] = CallOutEmailAttachmentSource;
                } else if (filter.StartsWith("swwpkgme:")) {
                    attach["#attachsource"] = MaintenanceEngEmailAttachmentSource;
                } else if (filter.StartsWith("swwpkgdo:")) {
                    attach["#attachsource"] = DomEmailAttachmentSource;
                } else {
                    attach["#attachsource"] = UnknownSource;
                }

                attachList.Add(attach);
            });

            var searchResult = new EntityRepository.SearchEntityResult {
                ResultList = attachList,
                IdFieldName = allAttachs.IdFieldName,
                PaginationData = allAttachs.PaginationData
            };
            packageResult.ResultObject.Add("#allattachments_", searchResult);
        }

        public CompositionFetchRequest WoCompositionRequest(string woId, List<string> compositions) {
            return new CompositionFetchRequest {
                CompositionList = compositions,
                Id = woId + "",
                Key = CompositionSchemaKey,
                PaginatedSearch = new PaginatedSearchRequestDto {
                    PageNumber = 1,
                    PageSize = 1000,
                    NumberOfPages = 1,
                    PaginationOptions = new List<int>() { 1000 }
                }
            };
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            BuildTestsDict();
        }

        public void HandleEvent(ContainerReloadedEvent eventToDispatch) {
            BuildTestsDict();
        }

        private void BuildTestsDict() {
            _testsI18NDict.Clear();
            var detailSchema = MetadataProvider.Schema("_WorkPackage", "adetail", ClientPlatform.Web);
            detailSchema?.GetDisplayable<OptionField>().Where(field => field.Attribute.EndsWith("tests")).ForEach(test => {
                test.Options.ForEach(option => {
                    _testsI18NDict.Add(option.Value, test.Label + " - " + option.Label);
                });
            });
        }
    }
}
