using softWrench.sW4.Util;
using System;

namespace softwrench.sw4.Hapag.Data.Configuration {
    internal class HapagQueryConstants {


        internal const string DefaultQualifiedQuery = @"
        NOT
        (  
            ({0}.status IN ('CANCELLED') AND {0}.creationdate < @past(12 months))
                                    OR 
            ({0}.status IN ('CLOSED') AND ({0}.assetnum='' OR {0}.assetnum IS NULL) AND {0}.statusdate < @past(24 months))
        )";

        internal const string DefaultSRGridQuery = @"
        NOT
        (
            (SR.status IN ('CANCELLED') AND (SR.assetnum='' OR SR.assetnum IS NULL) AND SR.creationdate < @past(12 months))
                        OR 
            (SR.status IN ('CLOSED') AND (SR.assetnum='' OR SR.assetnum IS NULL) AND SR.ITDCLOSEDATE  < @past(24 months))
        )";

        internal const string DefaultIncidentGridQuery = @"
        NOT
        (
            (INCIDENT.status IN ('CANCELLED') AND (INCIDENT.assetnum='' OR INCIDENT.assetnum IS NULL) AND INCIDENT.creationdate < @past(12 months))
                        OR 
            (INCIDENT.status IN ('CLOSED') AND (INCIDENT.assetnum='' OR INCIDENT.assetnum IS NULL) AND INCIDENT.ITDCLOSEDATE  < @past(24 months))
        )";

        internal const string DefaultProblemGridQuery = @"
        PROBLEM.status NOT IN ('CANCELLED') AND PROBLEM.creationdate >= @past(12 months)";
        // from ServiceIT v2
        //        internal const string DefaultProblemGridQuery = @"
        //        NOT
        //        (
        //            (PROBLEM.status IN ('CANCELLED') AND (PROBLEM.assetnum='' OR PROBLEM.assetnum IS NULL) AND PROBLEM.creationdate < @past(12 months))
        //        )";

        internal static string ITCOpenImacs(string commaSeparatedPersongroups, bool isViewAllOperation) {
            if (!isViewAllOperation) {
                //here we did a left join on woactivity_
                return "woactivity_.status = 'INPRG' AND woactivity_.ownergroup IN ({0})".Fmt(commaSeparatedPersongroups);
            }

            return @"
            exists (select 1 from woactivity w where w.origrecordid = ticketid and w.origrecordclass = 'SR' 
                and w.status = 'INPRG' AND w.ownergroup IN ({0}))".Fmt(commaSeparatedPersongroups);


        }


        public static string SrITCDashboard() {
            return ITCActionRequired("sr");
        }

        public static string IncidentITCDashboard() {
            return ITCActionRequired("incident");
        }


        internal static string ITCActionRequired(string qualifier) {
            return String.Format("{0}.status in ('SLAHOLD','RESOLVED')", qualifier);
        }


        internal const string PurchaseSR = DefaultSRGridQuery + @" AND SR.Commoditygroup ='HLC-HWS'";
        
        internal const string DefaultAsset = @" asset.pluspcustomer != 'HLC-00'";

        internal const string PurchaseIncident = "Incident.Commoditygroup ='HLC-HWS'";


        internal const string EndUserSR = DefaultSRGridQuery + @"
              AND (UPPER(affectedperson) = UPPER(@personid) OR UPPER(reportedby) = UPPER(@personid)) 
        ";

        internal const string EndUserOpenRequests = @"
              SR.pluspcustomer like 'HLC-%' AND SR.status in ('QUEUED','PENDING','INPROG','REJECTED')
              AND (UPPER(affectedperson) = UPPER(@personid) OR UPPER(reportedby) = UPPER(@personid)) 
        ";

        internal const string EndUserActionRequired = @"
              SR.pluspcustomer like 'HLC-%' AND SR.status in ('SLAHOLD','RESOLVED') 
              AND (UPPER(affectedperson) = UPPER(@personid) OR UPPER(reportedby) = UPPER(@personid)) 
        ";
        //TODO: and SR.reportdate > @past(3 months) ??

        internal const string DefaultTapeBackUpReportQuery = @"
              (lastworklog_.worklogid = (select max(worklogid) from worklog where recordkey = incident.ticketid and siteid = incident.siteid) OR lastworklog_.worklogid is null)
              AND (incident.STATUS NOT IN ('CLOSED-FC', 'CLOSED-MNOW', 'CANCEL', 'CLOSED-UNSOLVED', 'RESOLVED', 'REMOTE-RESOLVED') OR
                   (incident.STATUS = 'CLOSED' AND incident.statusdate > @past(1 days) )) AND commodities_.description like 'HLC-SW-SERVICE-INFO-BACKUP%'
              
        ";

        internal const string DefaultHardwareRepairReportQuery = @"
            incident.COMMODITYGROUP = 'HLC-HWS'
            ";

        internal const string DefaultGroupReportQuery = @"
            persongroup like 'C-HLC-WW%' and 
            persongroup not like 'C-HLC-WW-LC%' and 
            persongroup not like 'C-HLC-WW-AR%' and 
            persongroup not like 'C-HLC-WW-RG%'
            ";


        internal const string DefaultEscalationIncidentQuery = @"
            (lastworklog_.worklogid = (select max(worklogid) from worklog where recordkey = incident.ticketid and siteid = incident.siteid) OR lastworklog_.worklogid is null) and            
            status not like 'CLOSED' and
            status not like 'CANCELLED'
            ";

        internal const string DefaultITCReportQuery = @"
            persongroupview.persongroup like 'C-HLC-WW-LC%' 
            ";

        internal const string DefaultITCReportRegionAndAreaQuery = @"
            locationpersongroup_.persongroup like 'C-HLC-WW-LC%' 
            ";    

        internal const string MovedAssets = @"
            asset.assetnum in (select assetnum from SR s where s.CLASSIFICATIONID = '81515100' and s.ITDCLOSEDATE > @past(3 months))";

        internal const string DashBoardADOpenIncidents = @"incident.ownergroup IN ({0}) AND incident.status in ('SLAHOLD','RESOLVED')";
        
        internal const string ADOpenIncidents = @"incident.ownergroup IN ({0})";
    }
}
