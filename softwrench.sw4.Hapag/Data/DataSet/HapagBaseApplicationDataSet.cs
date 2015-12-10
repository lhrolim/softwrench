using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Security;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagBaseApplicationDataSet : MaximoApplicationDataSet {

        private readonly IHlagLocationManager _locationManager;

        private readonly EntityRepository _entityRepository;

        //        protected readonly EntityRepository EntityRepository = new EntityRepository();

        private readonly MaximoHibernateDAO _maxDao;

        public HapagBaseApplicationDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, MaximoHibernateDAO maxDao) {
            _locationManager = locationManager;
            _entityRepository = entityRepository;
            _maxDao = maxDao;
        }


        public EntityRepository EntityRepository {
            get { return EntityRepository; }
        }

        protected IHlagLocationManager LocationManager {
            get { return _locationManager; }
        }



        

        public IEnumerable<IAssociationOption> GetHlagAllLocations(OptionFieldProviderParameters parameters) {
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

        public IEnumerable<IAssociationOption> GetHlagUserLocations(OptionFieldProviderParameters parameters) {
            var currentLocations = LocationManager.FindAllLocationsOfCurrentUser();
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
            var currentLocations = LocationManager.FindAllLocationsOfCurrentUser();
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
            var currentLocations = LocationManager.FindAllLocationsOfCurrentUser();
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
            var list = MaxDAO.FindByNativeQuery(AssetConstants.PrinterClassificationPath);
            return
                classtructureId.EqualsAny(list.Select(r => r[AssetConstants.ClassStructureIdColumn]).Cast<string>());

        }

        protected bool IsStdAsset(String classtructureId) {
            var list = MaxDAO.FindByNativeQuery(AssetConstants.StdClassificationPathParent);
            return
                classtructureId.EqualsAny(list.Select(r => r[AssetConstants.ClassStructureIdColumn]).Cast<string>());

        }


        protected SearchRequestDto AssetByLocationCondition(SearchRequestDto searchDTO, string fromLocation) {
            if (String.IsNullOrWhiteSpace(fromLocation)) {
                throw ExceptionUtil.InvalidOperation("from location parameter should not be null");
            }
            searchDTO.IgnoreWhereClause = true;
            var locations = LocationManager.FindAllLocationsOfCurrentUser();
            var location = locations.FirstOrDefault(l => l.SubCustomer.Contains(fromLocation));
            if (location == null) {
                throw ExceptionUtil.InvalidOperation("current user can not access location {0}", fromLocation);
            }

            searchDTO.AppendSearchEntry(ISMConstants.PluspCustomerColumn, "%" + fromLocation);
            searchDTO.AppendWhereClause(location.CostCentersForQuery("asset.glaccount"));
            return searchDTO;
        }

        public override string ClientFilter() {
            return "hapag";
        }
    }
}
