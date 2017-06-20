﻿using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarWorkPackageCompositionHandler : ISingletonComponent, ISWEventListener<ApplicationStartedEvent>, ISWEventListener<ContainerReloadedEvent> {

        private const string UnknownSource = "Unknown";
        private const string RelayEventAttachmentSource = "Relay Event File";
        private const string CallOutEmailAttachmentSource = "Subcontractor Email Attachment";
        private const string MaintenanceEngEmailAttachmentSource = "Maintenance Engineering Email Attachment";

        private readonly Dictionary<string, string> _testsI18NDict = new Dictionary<string, string>();

        public void HandleTestsWorkLogs(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var wkpkgWorkLogs = woResult.ResultObject.First(pair => FSWPackageConstants.WorklogsRelationship.Equals(pair.Key)).Value;

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

        public void HandleTestAttachments(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var wkpkgAttachs = woResult.ResultObject.First(pair => FSWPackageConstants.AttachsRelationship.Equals(pair.Key)).Value;

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

        public void HandleAttachmentsTab(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            var allAttachs = woResult.ResultObject.First(pair => FSWPackageConstants.AllAttachmentsRelationship.Equals(pair.Key)).Value;

            var attachList = new List<Dictionary<string, object>>();
            allAttachs.ResultList.ForEach(attach => {
                attach["#attachdocument"] = attach["docinfo_.description"];
                attach["#attachcreateddate"] = attach["createdate"];
                attach["id"] = 0; // workaround to show expand button


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
                    } else {
                        attach["#attachsource"] = _testsI18NDict.ContainsKey(test) ? _testsI18NDict[test] : UnknownSource;
                    }
                } else if (filter.StartsWith("swwpkgco:")) {
                    attach["#attachsource"] = CallOutEmailAttachmentSource;
                } else if (filter.StartsWith("swwpkgme:")) {
                    attach["#attachsource"] = MaintenanceEngEmailAttachmentSource;
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
