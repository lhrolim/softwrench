﻿using System.Text;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagBaseApplicationDataSet : BaseApplicationDataSet {

        private IHlagLocationManager _locationManager;

        protected readonly EntityRepository EntityRepository = new EntityRepository();

        private MaximoHibernateDAO maxDAO;

        private readonly ISet<string> _applications = new HashSet<string>() { "ACTIVITY", "CHANGE", "INCIDENT", "PROBLEM", "SR", "WORKORDER" };


        protected IHlagLocationManager LocationManager {
            get {
                if (_locationManager != null) {
                    return _locationManager;
                }
                _locationManager =
                    SimpleInjectorGenericFactory.Instance.GetObject<IHlagLocationManager>(typeof(IHlagLocationManager));
                return _locationManager;
            }
        }



        protected MaximoHibernateDAO MaxDAO {
            get {
                if (maxDAO != null) {
                    return maxDAO;
                }
                maxDAO =
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoHibernateDAO>(typeof(MaximoHibernateDAO));
                return maxDAO;
            }
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var ob = result.ResultObject;
            if (ob != null) {
                var attachments = ob.GetAttribute("attachment_");
                if (attachments != null) {
                    foreach (var att in (IEnumerable<Dictionary<string, object>>)attachments) {
                        var urlDescription = AttachmentHandler.BuildFileName(att["docinfo_.urlname"] as string);
                        if (urlDescription == null) {
                            //keep description
                            att["urldescription"] = att["description"];
                        } else {
                            att["urldescription"] = urlDescription;
                        }

                    }
                }
            }
            return result;
        }

        public virtual IEnumerable<IAssociationOption> GetHlagAllLocations(OptionFieldProviderParameters parameters) {
            return LocationManager.FindAllLocations();
        }

        public IEnumerable<IAssociationOption> GetHlagAllCostCenters(OptionFieldProviderParameters parameters) {
            ISet<IAssociationOption> costcenters = new SortedSet<IAssociationOption>();
            var currentLocations = LocationManager.FindAllLocations();
            var hlagLocations = currentLocations as HlagGroupedLocation[] ?? currentLocations.ToArray();
            if (CollectionExtensions.IsNullOrEmpty(hlagLocations)) {
                Log.Warn(HapagErrorCatalog.Err001);
                return costcenters;
            }

            foreach (var hlagLocation in hlagLocations) {
                foreach (var costcenter in hlagLocation.CostCenters) {
                    var result = new AssociationOption(costcenter, costcenter);
                    costcenters.Add(result);
                }
            }
            return costcenters;
        }

        public virtual IEnumerable<IAssociationOption> GetHlagUserLocations(OptionFieldProviderParameters parameters) {
            var currentLocations = LocationManager.FindAllLocationsOfCurrentUser(parameters.ApplicationMetadata);
            var hlagUserlocations = currentLocations as HlagGroupedLocation[] ?? currentLocations.ToArray();
            if (CollectionExtensions.IsNullOrEmpty(hlagUserlocations)) {
                Log.Warn(HapagErrorCatalog.Err001);
                return hlagUserlocations;
            }
            if (parameters.OptionField.Options != null) {
                return hlagUserlocations.Union(parameters.OptionField.Options);
            }
            return hlagUserlocations;
        }

        public IEnumerable<IAssociationOption> GetHlagCustomerCostCenter(OptionFieldProviderParameters parameters) {
            ISet<IAssociationOption> costcenters = new SortedSet<IAssociationOption>();
            var currentLocations = LocationManager.FindAllLocationsOfCurrentUser(parameters.ApplicationMetadata);
            var hlagUserlocations = currentLocations as HlagGroupedLocation[] ?? currentLocations.ToArray();
            if (CollectionExtensions.IsNullOrEmpty(hlagUserlocations)) {
                Log.Warn(HapagErrorCatalog.Err001);
                return costcenters;
            }

            foreach (var hlaguserlocation in hlagUserlocations) {
                foreach (var costcenter in hlaguserlocation.CostCenters) {
                    var result = new AssociationOption(costcenter, costcenter);
                    costcenters.Add(result);
                }
            }
            return costcenters;
        }

        public IEnumerable<IAssociationOption> GetHlagUserLocationAndCostCenter(OptionFieldProviderParameters parameters) {
            ISet<IAssociationOption> costcenters = new SortedSet<IAssociationOption>();
            var currentLocations = LocationManager.FindAllLocationsOfCurrentUser(parameters.ApplicationMetadata);
            var hlagUserlocations = currentLocations as HlagGroupedLocation[] ?? currentLocations.ToArray();
            if (CollectionExtensions.IsNullOrEmpty(hlagUserlocations)) {
                Log.Warn(HapagErrorCatalog.Err001);
                return costcenters;
            }

            foreach (var hlaguserlocation in hlagUserlocations) {
                foreach (var costcenter in hlaguserlocation.CostCenters) {
                    var result = new AssociationOption(hlaguserlocation.SubCustomerSuffix + "-" + costcenter, hlaguserlocation.Label + " - " + costcenter);
                    costcenters.Add(result);
                }
            }
            return costcenters;
        }

        protected Boolean IsPrinterAsset(String classtructureId) {
            var results = MaxDAO.FindByNativeQuery(AssetConstants.PrinterClassificationPath);
            var list = results.Cast<IEnumerable<KeyValuePair<string, object>>>()
                .Select(r => r.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return
                classtructureId.EqualsAny(list.Select(r => r[AssetConstants.ClassStructureIdColumn]).Cast<string>());

        }

        protected Boolean IsStdAsset(String classtructureId) {
            var results = MaxDAO.FindByNativeQuery(AssetConstants.StdClassificationPathParent);
            var list = results.Cast<IEnumerable<KeyValuePair<string, object>>>()
                .Select(r => r.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return
                classtructureId.EqualsAny(list.Select(r => r[AssetConstants.ClassStructureIdColumn]).Cast<string>());

        }


        protected SearchRequestDto AssetByLocationCondition(SearchRequestDto searchDTO, string fromLocation, ApplicationMetadata metadata) {
            if (String.IsNullOrWhiteSpace(fromLocation)) {
                throw ExceptionUtil.InvalidOperation("from location parameter should not be null");
            }
            searchDTO.IgnoreWhereClause = true;
            var locations = LocationManager.FindAllLocationsOfCurrentUser(metadata);
            var location = locations.FirstOrDefault(l => l.SubCustomer.Contains(fromLocation));
            if (location == null) {
                throw ExceptionUtil.InvalidOperation("current user can not access location {0}", fromLocation);
            }

            searchDTO.AppendSearchEntry(ISMConstants.PluspCustomerColumn, "%" + fromLocation);
            searchDTO.AppendWhereClause(location.CostCentersForQuery("asset.glaccount"));
            return searchDTO;
        }

        public SearchRequestDto AppendRelatedRecordWCToWorklog(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            var ticketId = dto.ValuesDictionary["recordkey"].Value;
            var originalClass = dto.ValuesDictionary["class"].Value as string;
            dto.SearchValues = null;
            dto.SearchParams = null;
            //            var assetNum = preFilter.OriginalEntity.GetAttribute("assetnum");
            //            //as of HAP-882
            //            dto.AppendWhereClauseFormat(@"ticketid in (select recordkey from MULTIASSETLOCCI multi where multi.assetnum = '{0}' and RECORDCLASS in ({1}) ) " +
            //                                        "or (imac.classificationid = '81515700' and imac.description = 'Decommission of {0}')", assetNum, "'CHANGE','INCIDENT','PROBLEM','SR'");

            //            var applicationsToIterate = _applications.Where(a => !a.EqualsIc(originalClass));
            var sb = new StringBuilder();
            if (ticketId is ICollection) {
                ticketId = String.Join("','", ticketId.As<List<String>>());
                sb.AppendFormat("((worklog.recordkey in ('{0}') ) AND ( worklog.class = '{1}' )) ", ticketId,
                    originalClass);
                foreach (var application in _applications) {
                    sb.AppendFormat(@"
                or (worklog.recordkey in (select relatedrecord.relatedreckey as relatedreckey 
                from RELATEDRECORD as relatedrecord  where relatedrecord.recordkey in ('{0}')  AND  relatedrecord.class = '{1}' AND RELATEDRECCLASS = '{2}' ) and worklog.class = '{2}')",
                        ticketId, originalClass, application);
                }
                dto.ExtraLeftJoinSection =
                    "left join relatedrecord rr on (rr.RECORDKEY = worklog.RECORDKEY and worklog.CLASS = rr.CLASS and rr.RELATEDRECKEY in ('{0}'))".Fmt(ticketId);
                dto.AppendProjectionField(new ProjectionField("relatedrecordkey", "rr.RELATEDRECKEY"));
            } else {
                sb.AppendFormat("((worklog.recordkey = '{0}' ) AND ( worklog.class = '{1}' )) ", ticketId,
                    originalClass);
                foreach (var application in _applications) {
                    sb.AppendFormat(@"
                or (worklog.recordkey in (select relatedrecord.relatedreckey as relatedreckey 
                from RELATEDRECORD as relatedrecord  where relatedrecord.recordkey = '{0}'  AND  relatedrecord.class = '{1}' AND RELATEDRECCLASS = '{2}' ) and worklog.class = '{2}')",
                        ticketId, originalClass, application);
                }
            }
            dto.WhereClause = sb.ToString();

            return dto;
        }

        public override string ClientFilter() {
            return "hapag";
        }
    }
}
