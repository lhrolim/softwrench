namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FSWPackageConstants {
        public const string WorklogsRelationship = "wkpgworklogs_";
        public const string AttachsRelationship = "wkpgattachments_";
        public const string CallOutAttachsRelationship = "wkpgcoattachments_";
        public const string MaintenanceEngAttachsRelationship = "wkpgmeattachments_";
        public const string DailyOutageMeetingAttachsRelationship = "wkpgdomattachments_";
        public const string AllAttachmentsRelationship = "wkpgallattachments_";

        public const string GFedQueryQuery = @"
            select  g.onm_regional_manager as regmanager, g.onm_site_manager as supervisor, g.onm_maintenance_supervisor as tech, g.onM_Planner_Scheduler as planner 
                g.assettitle as facilityTitle, g.[street address line 1] as facilityAddress, g.city facilityCity, g.[state code] as facilityState, g.[postal code] as facilityPostalCode from 
                (select (select o.description from onmparms o where w.location like o.value + '%' and o.parameter='PlantID') as PlantName from workorder w where w.workorderid = ?) x 
                    left join 
                (Select assettitle, onm_regional_manager, onm_site_manager, onm_maintenance_supervisor, onM_Planner_Scheduler, [street address line 1], city, [state code], [postal code]
                    from GLOBALFEDPRODUCTION.GlobalFed.Business.vwsites) G on x.PlantName=G.assettitle";

        public const string TechColumn = "tech";
        public const string SupervisorColumn = "supervisor";
        public const string RegionalManagerColumn = "regmanager";
        public const string PlannerColumn = "planner";


        public const string FacilityTitleColumn = "facilityTitle";
        public const string FacilityAdressColumn = "facilityAddress";
        public const string FacilityCityColumn = "facilityCity";
        public const string FacilityStateColumn = "facilityState";
        public const string FacilityPostalCodeColumn = "facilityPostalCode";
    }
}
