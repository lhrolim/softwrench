using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarCustomGlobalFedService : ISingletonComponent {

        public const string GFedQuery = @"
            select * from 
                (select (select o.description from onmparms o where w.location like o.value + '%' and o.parameter='PlantID') as PlantName from workorder w where w.workorderid = ?) x 
                    left join 
                (Select assettitle as facilityTitle, 
	                (select top 1 p.displayname from email e left join person p on e.personid = p.personid where e.emailaddress = onm_regional_manager and onm_regional_manager is not null) as regmanager, 
	                (select top 1 p.displayname from email e left join person p on e.personid = p.personid where e.emailaddress = onm_site_manager and onm_site_manager is not null) as supervisor, 
	                (select top 1 p.displayname from email e left join person p on e.personid = p.personid where e.emailaddress = onm_maintenance_supervisor and onm_maintenance_supervisor is not null) as technician, 
	                (select top 1 e.personid from email e where e.emailaddress = onm_maintenance_supervisor and onm_maintenance_supervisor is not null) as technicianid, 
	                (select top 1 p.displayname from email e left join person p on e.personid = p.personid where e.emailaddress = onM_Planner_Scheduler and onM_Planner_Scheduler is not null) as planner, 
	                [street address line 1] as facilityAddress, 
	                city as facilityCity, 
	                [state code] as facilityState, 
	                [postal code] as facilityPostalCode
                from GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites) G on x.PlantName=G.facilityTitle";

        public const string TechColumn = "technician";
        public const string TechIdColumn = "technicianid";
        public const string SupervisorColumn = "supervisor";
        public const string RegionalManagerColumn = "regmanager";
        public const string PlannerColumn = "planner";

        public const string FacilityTitleColumn = "facilityTitle";
        public const string FacilityAdressColumn = "facilityAddress";
        public const string FacilityCityColumn = "facilityCity";
        public const string FacilityStateColumn = "facilityState";
        public const string FacilityPostalCodeColumn = "facilityPostalCode";

        public const string GFedEmailQuery = @"
            select * from 
                (select (select o.description from onmparms o where w.location like o.value + '%' and o.parameter='PlantID') as PlantName from workorder w where w.workorderid = ?) x 
                    left join 
                (Select assettitle as facilityTitle, 
	                onm_regional_manager  as  regmanageremail, 
	                onm_site_manager as supervisoremail,
	                onM_Planner_Scheduler  as planneremail, 
	                onM_Account_Manager as accountmanageremail,
	                [performance Engineer Email(s)] as perfengineeremail
                from GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites) G on x.PlantName=G.facilityTitle";

        public const string RegionalManagerEmailColumn = "regmanageremail";
        public const string SupervisorEmailColumn = "supervisoremail";
        public const string PlannerEmailColumn = "planneremail";
        public const string AccountManagerEmailColumn = "accountmanageremail";
        public const string PerformanceEngineerEmailColumn = "perfengineeremail";

        public const string GFedLostEnergy = @"
            SELECT
                R.OutageReportedAsType, 
                sum(R.TotalLostkWh / 1000) AS lostmwhh, 
                R.EventStart, 
                R.EventEnd as lostdate 
            FROM GLOBALFEDPRODUCTION.GlobalFed.Alarms.vwOperatorLogGADSRecords AS R 
            INNER JOIN GLOBALFEDPRODUCTION.GlobalFed.Business.vwSites as S 
                On R.SiteAssetID = S.AssetID 
            WHERE R.OutageReportedAsType IN ('FO','MO','PO','OMC','OMCCurtail','OMCCurtailBuyer') 
                AND S.assettitle in (select (select o.description from onmparms o where w.location like o.value + '%' and o.parameter='PlantID') from workorder w where w.workorderid = ?)
                AND R.EventEnd >= ?  
                AND R.EventEnd < ? 
            GROUP BY R.TotalLostkWh, S.AssetTitle, R.OutageReportedAsType, R.Description, R.EventStart, R.EventEnd 
            ORDER BY R.EventEnd DESC";

        public const string GFedTotalLostEnergy = @"
            SELECT SUM(R.TotalLostkWh / 1000) AS totallostmwh 
            FROM GLOBALFEDPRODUCTION.GlobalFed.Alarms.vwOperatorLogGADSRecords AS R 
            INNER JOIN GLOBALFEDPRODUCTION.GlobalFed.Business.vwSites as S 
                On R.SiteAssetID = S.AssetID 
            WHERE R.OutageReportedAsType IN ('FO','MO','PO','OMC','OMCCurtail','OMCCurtailBuyer') 
                AND S.assettitle in (select (select o.description from onmparms o where w.location like o.value + '%' and o.parameter='PlantID') from workorder w where w.workorderid = ?)
                AND R.EventEnd >= ?  
                AND R.EventEnd < ?";

        public const string LostEnergyColumn = "lostmwhh";
        public const string TotalLostEnergyColumn = "totallostmwh";
        public const string LostEnergyDateColumn = "lostdate";

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
            return " (select top 1 (select top 1 p.displayname from email e left join person p on e.personid = p.personid where e.emailaddress = G.onM_Planner_Scheduler and G.onM_Planner_Scheduler is not null) from onmparms o left join GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites G on (o.description = G.assettitle or o.value = G.maximo_LocationID) where o.parameter='PlantID' and {0}.location like o.value + '%') ".Fmt(context);
        }

        public async Task<Dictionary<string, decimal>> LoadGfedLostEnergyData(long? workOrderId, DateTime start, DateTime end) {
            var entries = new List<LostEnergyEntry>();
            if (!ApplicationConfiguration.IsProd()) {
                for (var i = 4; i >= 0; i--) {
                    var date = DateTime.Today.AddDays(-1 * i);
                    entries.Add(new LostEnergyEntry() {
                        Date = date,
                        Value = (i + 1) * 0.1m
                    });
                    entries.Add(new LostEnergyEntry() {
                        Date = date,
                        Value = (i + 1) * 1m
                    });
                }
                return JoinSameDateLostEnergy(entries);
            }


            var qryResult = await _maxDao.FindByNativeQueryAsync(GFedLostEnergy, workOrderId, start, end);
            if (qryResult == null || !qryResult.Any()) {
                return new Dictionary<string, decimal>();
            }

            qryResult.ForEach(result => {
                entries.Add(new LostEnergyEntry() {
                    Date = Convert.ToDateTime(result[LostEnergyDateColumn], new CultureInfo("en-US")),
                    Value = Convert.ToDecimal(result[LostEnergyColumn])
                });
            });

            return JoinSameDateLostEnergy(entries);
        }

        public async Task<decimal> LoadGfedTotalLostEnergy(long? workOrderId, DateTime start) {
            if (!ApplicationConfiguration.IsProd()) {
                return 16.5m;
            }

            var qryResult = await _maxDao.FindByNativeQueryAsync(GFedTotalLostEnergy, workOrderId, start, DateTime.Today.AddDays(1));
            if (qryResult == null || !qryResult.Any()) {
                return 0m;
            }

            var row = qryResult.First();
            return !row.ContainsKey(TotalLostEnergyColumn) ? 0m : Convert.ToDecimal(row[TotalLostEnergyColumn]);
        }

        public async Task LoadGfedData(ApplicationDetailResult result) {
            var row = await LoadGfedData(result.ResultObject.GetLongAttribute("workorderid"));
            if (row == null) {
                return;
            }

            AddColumn(row, result.ResultObject, TechColumn);
            AddColumn(row, result.ResultObject, TechIdColumn);
            AddColumn(row, result.ResultObject, SupervisorColumn);
            AddColumn(row, result.ResultObject, RegionalManagerColumn);
            AddColumn(row, result.ResultObject, PlannerColumn);

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

        public async Task LoadGfedData(WorkPackage package, List<DailyOutageMeeting> doms, string to = null) {
            if (doms == null || !doms.Any()) {
                return;
            }
            var row = await LoadGfedData(package.WorkorderId);
            if (row == null) {
                doms.ForEach(dom => {
                    dom.Email = BuildToFromGfed((Dictionary<string, string>)null);
                });
                return;
            }
            if (row.ContainsKey(FacilityTitleColumn)) {
                package.FacilityName = row[FacilityTitleColumn];
            }
            if (to == null) {
                to = BuildToFromGfed(row);
            }

            doms.ForEach(dom => {
                dom.Email = to;
            });
        }

        public async Task<string> BuildToFromGfed(WorkPackage package) {
            if (!ApplicationConfiguration.IsProd()) {
                return null;
            }

            var qryResult = await _maxDao.FindByNativeQueryAsync(GFedEmailQuery, package.WorkorderId);
            var dict = qryResult.FirstOrDefault();
            var to = BuildToFromGfed(dict);
            if (dict != null && dict.ContainsKey(FacilityTitleColumn)) {
                package.FacilityName = dict[FacilityTitleColumn];
            }
            return to;
        }

        private static string BuildToFromGfed(Dictionary<string, string> row) {
            var toList = new List<string>();
            if (row != null) {
                AddEmail(row, toList, PlannerEmailColumn);
                AddEmail(row, toList, RegionalManagerEmailColumn);
                AddEmail(row, toList, SupervisorEmailColumn);
                AddEmail(row, toList, AccountManagerEmailColumn);
                AddEmail(row, toList, PerformanceEngineerEmailColumn);
            }
            toList.Add("fsocoperators@firstsolar.com");
            toList.Add("fsocleadership@firstsolar.com");
            toList.Add("omengineering@firstsolar.com");
            toList.Add("brent.galyon@firstsolar.com");
            //            toList.Add("support@controltechnologysolutions.com");
            return string.Join("; ", toList);
        }

        private async Task<Dictionary<string, string>> LoadGfedData(long? workOrderId) {
            if (!ApplicationConfiguration.IsProd()) {
                return new Dictionary<string, string>() {
                    {TechColumn, "Test Technician"},
                    {TechIdColumn, "swadmin"},
                    {SupervisorColumn, "Test Supervisor"},
                    {RegionalManagerColumn, "Test Manager"},
                    {PlannerColumn, "Test Planner"},
                    {FacilityTitleColumn, "Test Facility"},
                    {FacilityAdressColumn, "1234 Test Street"},
                    {FacilityCityColumn, "Scottsdale"},
                    {FacilityStateColumn, "AZ"},
                    {FacilityPostalCodeColumn, "12345"}
                };
            }


            var qryResult = await _maxDao.FindByNativeQueryAsync(GFedQuery, workOrderId);
            if (qryResult == null || !qryResult.Any()) {
                return null;
            }
            return qryResult.First();
        }

        private static void AddFacilityData(Dictionary<string, string> source, Dictionary<string, object> target) {
            AddColumn(source, target, FacilityTitleColumn);
            AddColumn(source, target, FacilityAdressColumn);
            AddColumn(source, target, FacilityCityColumn);
            AddColumn(source, target, FacilityStateColumn);
            AddColumn(source, target, FacilityPostalCodeColumn);
        }

        private static void AddColumn(Dictionary<string, string> source, Dictionary<string, object> target, string column) {
            if (source.ContainsKey(column)) {
                target.Add("#" + column.ToLower(), source[column]);
            }
        }

        private static void AddEmail(Dictionary<string, string> source, List<string> target, string column) {
            if (source.ContainsKey(column) && !string.IsNullOrEmpty(source[column])) {
                target.Add(source[column]);
            }
        }

        private static void AddFacilityData(Dictionary<string, string> source, CallOut callout) {
            if (source.ContainsKey(FacilityTitleColumn)) {
                callout.FacilityName = source[FacilityTitleColumn];
            }
            if (source.ContainsKey(FacilityAdressColumn)) {
                callout.FacilityAddress = source[FacilityAdressColumn];
            }
            if (source.ContainsKey(FacilityCityColumn)) {
                callout.FacilityCity = source[FacilityCityColumn];
            }
            if (source.ContainsKey(FacilityStateColumn)) {
                callout.FacilityState = source[FacilityStateColumn];
            }
            if (source.ContainsKey(FacilityPostalCodeColumn)) {
                callout.FacilityPostalCode = source[FacilityPostalCodeColumn];
            }
        }

        private static Dictionary<string, decimal> JoinSameDateLostEnergy(List<LostEnergyEntry> entries) {
            var output = new Dictionary<string, decimal>();
            entries.ForEach(entry => {
                var datestring = entry.Date.ToString("yyyyMMdd");
                if (output.ContainsKey(datestring)) {
                    output[datestring] = output[datestring] + entry.Value;
                } else {
                    output[datestring] = entry.Value;
                }
            });


            return output;
        }

        public class LostEnergyEntry {
            public DateTime Date { get; set; }
            public decimal Value { get; set; }
        }
    }
}
