using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarCustomGlobalFedService : ISingletonComponent {

        // DO NOT USE [Import] HERE THERE IS A BUG WITH REFLECTION

        private readonly IMaximoHibernateDAO _maxDao;

        public FirstSolarCustomGlobalFedService(IMaximoHibernateDAO maxDao) {
            _maxDao = maxDao;
        }

        public string FacilityQuery(string context) {
            if (!ApplicationConfiguration.IsProd()) {
                return " SUBSTRING({0}.location, 0, 5) ".Fmt(context);
            }
            return " (CASE WHEN exists (select * from onmparms o where {0}.location like o.value + '%') THEN (select top 1 G.scadA_GUID from onmparms o left join GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites G on  (o.description=G.assettitle or o.value=G.maximo_LocationID) where o.parameter='PlantID' and {0}.location like o.value + '%') WHEN 1=1 then SUBSTRING({0}.location, 0, 5) END) ".Fmt(context);
        }

        public string PlannerQuery(string context) {
            if (!ApplicationConfiguration.IsProd()) {
                return " ( SUBSTRING({0}.supervisor, 1, 0) + 'Test Planner') ".Fmt(context); // substring turns out to be a empty string, just to avoid a constant in dev
            }
            return " (select top 1 G.onM_Planner_Scheduler from onmparms o left join GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites G on (o.description = G.assettitle or o.value = G.maximo_LocationID) where o.parameter='PlantID' and {0}.location like o.value + '%') ".Fmt(context);
        }

        public async Task LoadGfedData(ApplicationDetailResult result) {
            var row = await LoadGfedData(result.ResultObject.GetLongAttribute("workorderid"));
            if (row == null) {
                return;
            }

            if (row.ContainsKey(FSWPackageConstants.TechColumn)) {
                result.ResultObject.SetAttribute("#technician", row[FSWPackageConstants.TechColumn]);
            }
            if (row.ContainsKey(FSWPackageConstants.SupervisorColumn)) {
                result.ResultObject.SetAttribute("#supervisor", row[FSWPackageConstants.SupervisorColumn]);
            }
            if (row.ContainsKey(FSWPackageConstants.RegionalManagerColumn)) {
                result.ResultObject.SetAttribute("#manager", row[FSWPackageConstants.RegionalManagerColumn]);
            }
            if (row.ContainsKey(FSWPackageConstants.PlannerColumn)) {
                result.ResultObject.SetAttribute("#planner", row[FSWPackageConstants.PlannerColumn]);
            }

            AddFacilityData(row, result.ResultObject);

            if (result.ResultObject.ContainsKey("callOuts_")) {
                var callouts = result.ResultObject["callOuts_"] as List<Dictionary<string, object>>;
                callouts?.ForEach(callout => {
                    AddFacilityData(row, callout);
                });
            }
        }

        public async Task LoadGfedData(WorkPackage package, List<CallOut> callouts) {
            if (callouts == null || !callouts.Any()) {
                return;
            }

            var row = await LoadGfedData(package.WorkorderId);
            if (row == null) {
                return;
            }

            callouts.ForEach(callout => AddFacilityData(row, callout));
        }

        public async Task LoadGfedData(WorkPackage package, CallOut callout) {
            if (callout == null) {
                return;
            }
            var row = await LoadGfedData(package.WorkorderId);
            if (row == null) {
                return;
            }
            AddFacilityData(row, callout);
        }

        private async Task<Dictionary<string, string>> LoadGfedData(long? workOrderId) {
            if (!ApplicationConfiguration.IsProd()) {
                return new Dictionary<string, string>() {
                    {FSWPackageConstants.TechColumn, "Test Technician"},
                    {FSWPackageConstants.SupervisorColumn, "Test Supervisor"},
                    {FSWPackageConstants.RegionalManagerColumn, "Test Manager"},
                    {FSWPackageConstants.PlannerColumn, "Test Planner"},
                    {FSWPackageConstants.FacilityTitleColumn, "Test Facility"},
                    {FSWPackageConstants.FacilityAdressColumn, "1234 Test Street"},
                    {FSWPackageConstants.FacilityCityColumn, "Scottsdale"},
                    {FSWPackageConstants.FacilityStateColumn, "AZ"},
                    {FSWPackageConstants.FacilityPostalCodeColumn, "12345"}
                };
            }


            var qryResult = await _maxDao.FindByNativeQueryAsync(FSWPackageConstants.GFedQueryQuery, workOrderId);
            if (qryResult == null || !qryResult.Any()) {
                return null;
            }
            return qryResult.First();
        }

        private void AddFacilityData(Dictionary<string, string> source, Dictionary<string, object> target) {
            if (source.ContainsKey(FSWPackageConstants.FacilityTitleColumn)) {
                target.Add("#facilitytitle", source[FSWPackageConstants.FacilityTitleColumn]);
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityAdressColumn)) {
                target.Add("#facilityaddress", source[FSWPackageConstants.FacilityAdressColumn]);
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityCityColumn)) {
                target.Add("#facilitycity", source[FSWPackageConstants.FacilityCityColumn]);
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityStateColumn)) {
                target.Add("#facilitystate", source[FSWPackageConstants.FacilityStateColumn]);
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityPostalCodeColumn)) {
                target.Add("#facilitypostalcode", source[FSWPackageConstants.FacilityPostalCodeColumn]);
            }
        }

        private void AddFacilityData(Dictionary<string, string> source, CallOut callout) {
            if (source.ContainsKey(FSWPackageConstants.FacilityTitleColumn)) {
                callout.FacilityName = source[FSWPackageConstants.FacilityTitleColumn];
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityAdressColumn)) {
                callout.FacilityAddress = source[FSWPackageConstants.FacilityAdressColumn];
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityCityColumn)) {
                callout.FacilityCity = source[FSWPackageConstants.FacilityCityColumn];
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityStateColumn)) {
                callout.FacilityState = source[FSWPackageConstants.FacilityStateColumn];
            }
            if (source.ContainsKey(FSWPackageConstants.FacilityPostalCodeColumn)) {
                callout.FacilityPostalCode = source[FSWPackageConstants.FacilityPostalCodeColumn];
            }
        }
    }
}
