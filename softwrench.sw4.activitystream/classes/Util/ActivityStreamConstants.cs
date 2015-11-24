using System.Collections.Generic;

namespace softwrench.sw4.activitystream.classes.Util {
    internal class ActivityStreamConstants {
        public const int HoursToPurge = 24;

        public static string CreatedByCol = "createby";
        public static string CreatedDateCol = "createdate";
        public static string RowstampCol = "rowstamp";

        public static string DefaultStreamName = "defaultstream";



        public static string BaseIdCacheQuery =
            @"select max(ticketuid) as max, 'servicerequest' as application from ticket where class = 'SR' union  
                    select max(ticketuid) as max, 'incident' as application from ticket where class ='INCIDENT' union  
                    select max(workorderid) as max, 'workorder' as application from workorder union 
                    select max(commloguid) as max, 'commlog' as application from commlog union 
                    select max(worklogid) as max, 'worklog' as application from worklog";


        public static Dictionary<string, string> BaseQueries =
            new Dictionary<string, string>
            {
                {
                    "servicerequest",
                    "SELECT 'servicerequest' AS application, " +
                    "'editdetail' AS targetschema, 'service request' AS label, " +
                    "'fa-ticket' AS icon,ticketid AS id, ticketuid AS uid, " +
                    "null AS parentid, null AS parentuid, " +
                    "null AS parentapplication, description AS summary," +
                    "CASE WHEN {1} > ticketuid THEN changeby ELSE reportedby " +
                    "END changeby, changedate, " +
                    "CONVERT(bigint, rowstamp) AS rowstamp FROM ticket sr " +
                    "WHERE changedate > DATEADD(HOUR,-{0},GETDATE()) AND " +
                    "changedate < GETDATE() AND class='SR' "
                },
                {
                    "servicerequestcommlogs",
                    "SELECT 'commlog' AS application, null AS targetschema, " +
                    "'communication' AS label, 'fa-envelope-o' AS icon, " +
                    "CONVERT(varchar(10), commlogid) AS id, c.commloguid AS uid, " +
                    "sr.ticketid AS parentid, c.ownerid AS parentuid, " +
                    "'servicerequest' AS parentapplication, " +
                    "c.subject AS summary, c.createby AS changeby, " +
                    "c.createdate AS changedate, " +
                    "CONVERT(bigint, c.rowstamp) AS rowstamp FROM commlog c " +
                    "LEFT JOIN ticket sr ON sr.ticketuid = c.ownerid " +
                    "WHERE c.ownertable = 'SR' AND createdate >  DATEADD(HOUR,-{0},GETDATE()) AND " +
                    "createdate < GETDATE()"
                },
                {
                    "servicerequestworklogs",
                    "SELECT 'worklog' AS application, null AS targetschema, " +
                    "'work log' AS label, 'fa fa-gavel' AS icon, " +
                    "CONVERT(varchar(10), l.worklogid) AS id, " +
                    "CONVERT(varchar(10), l.worklogid) AS uid, " +
                    "l.recordkey AS parentid, sr.ticketuid AS parentuid, " +
                    "'servicerequest' AS parentapplication, " +
                    "l.description AS summary, l.createby AS changeby, " +
                    "l.modifydate AS changedate, " +
                    "CONVERT(bigint, l.rowstamp) AS rowstamp FROM worklog l " +
                    "LEFT JOIN ticket sr ON sr.ticketid = l.recordkey " +
                    "where l.class in ('SR') AND logtype = 'clientnote' " +
                    "AND modifydate >  DATEADD(HOUR,-{0},GETDATE()) AND modifydate < GETDATE()"
                },

                {
                    "incident",
                    "SELECT 'incident' AS application, " +
                    "'editdetail' AS targetschema, 'incident' AS label, " +
                    "'fa-warning' AS icon, ticketid AS id, ticketuid AS uid, " +
                    "null AS parentid, null AS parentuid, " +
                    "null AS parentapplication, description AS summary, " +
                    "changeby, changedate, CONVERT(bigint, rowstamp) AS rowstamp " +
                    "FROM ticket incident WHERE changedate > DATEADD(HOUR,-{0},GETDATE()) " +
                    "AND changedate < GETDATE() AND class='INCIDENT' "
                },
                {
                    "incidentcommlogs",
                    "SELECT 'commlog' AS application, null AS targetschema, " +
                    "'communication' AS label, 'fa-envelope-o' AS icon, " +
                    "CONVERT(varchar(10), commlogid) AS id, c.commloguid AS uid, " +
                    "incident.ticketid AS parentid, c.ownerid AS parentuid, " +
                    "'servicerequest' AS parentapplication, " +
                    "c.subject AS summary, c.createby AS changeby, " +
                    "c.createdate AS changedate, " +
                    "CONVERT(bigint, c.rowstamp) AS rowstamp FROM commlog c " +
                    "LEFT JOIN ticket incident ON incident.ticketuid = c.ownerid " +
                    "WHERE c.ownertable = 'INCIDENT' AND createdate >  DATEADD(HOUR,-{0},GETDATE()) AND " +
                    "createdate < GETDATE()"
                },
                {
                    "incidentworklogs",
                    "SELECT 'worklog' AS application, null AS targetschema, " +
                    "'work log' AS label, 'fa fa-gavel' AS icon, " +
                    "CONVERT(varchar(10), l.worklogid) AS id, " +
                    "CONVERT(varchar(10), l.worklogid) AS uid, " +
                    "l.recordkey AS parentid, incident.ticketuid AS parentuid, " +
                    "l.class AS parentapplication, " +
                    "l.description AS summary, l.createby AS changeby, " +
                    "l.modifydate AS changedate, " +
                    "CONVERT(bigint, l.rowstamp) AS rowstamp FROM worklog l " +
                    "LEFT JOIN ticket incident ON incident.ticketid = l.recordkey " +
                    "where l.class = 'INCIDENT' AND logtype = 'clientnote' " +
                    "AND modifydate >  DATEADD(HOUR,-{0},GETDATE()) AND modifydate < GETDATE()"
                },
                {
                    "workorder",
                    "SELECT 'workorder' AS application, " +
                    "'editdetail' AS targetschema, 'work order' AS label, " +
                    "'fa-wrench' AS icon, wonum AS id, workorderid AS uid, " +
                    "null AS parentid, null AS parentuid, " +
                    "null AS parentapplication, description AS summary, " +
                    "changeby, changedate, CONVERT(bigint, rowstamp) AS rowstamp " +
                    "FROM workorder WHERE changedate > DATEADD(HOUR,-{0},GETDATE()) " +
                    "AND changedate < GETDATE() "
                },
                {
                    "workorderworklogs",
                    "SELECT 'worklog' AS application, null AS targetschema, " +
                    "'work log' AS label, 'fa fa-gavel' AS icon, " +
                    "CONVERT(varchar(10), l.worklogid) AS id, " +
                    "CONVERT(varchar(10), l.worklogid) AS uid, " +
                    "l.recordkey AS parentid, workorder.workorderid AS parentuid, " +
                    "'WORKORDER' AS parentapplication, " +
                    "l.description AS summary, l.createby AS changeby, " +
                    "l.modifydate AS changedate, " +
                    "CONVERT(bigint, l.rowstamp) AS rowstamp FROM worklog l " +
                    "LEFT JOIN workorder ON workorder.wonum = l.recordkey " +
                    "WHERE class = 'WORKORDER' AND logtype = 'clientnote' " +
                    "AND modifydate >  DATEADD(HOUR,-{0},GETDATE()) AND " +
                    "modifydate < GETDATE()"
                }
            };
    }
}
