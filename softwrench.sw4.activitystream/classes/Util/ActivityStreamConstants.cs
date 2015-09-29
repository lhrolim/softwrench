using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.activitystream.classes.Util {
    internal class ActivityStreamConstants
    {
        public const int HoursToPurge = 24;

        public static string CreatedByCol = "createby";
        public static string CreatedDateCol = "createdate";
        public static string RowstampCol = "rowstamp";

        public static Dictionary<string, string> baseQueries = 
            new Dictionary<string, string>
            {
                {
                    "sr",
                    "SELECT 'servicerequest' AS application, " +
                    "'editdetail' AS targetschema, 'service request' AS label, " +
                    "'fa-ticket' AS icon,ticketid AS id, ticketuid AS uid, " +
                    "null AS parentid, null AS parentuid, " +
                    "null AS parentapplication, description AS summary," +
                    "CASE WHEN {2} > ticketuid THEN changeby ELSE reportedby " +
                    "END changeby, changedate, " +
                    "CONVERT(bigint, rowstamp) AS rowstamp FROM ticket sr " +
                    "WHERE changedate > DATEADD(HOUR,-{0},GETDATE()) AND " +
                    "changedate < '{1}' AND class='SR' "
                },
                {
                    "srCommlogs",
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
                    "createdate < '{1}'"
                },
                {
                    "srWorklogs",
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
                    "AND modifydate >  DATEADD(HOUR,-{0},GETDATE()) AND modifydate < '{1}'"
                },
                {
                    "ssr",
                    "SELECT 'servicerequest' AS application, " +
                    "'editdetail' AS targetschema, 'service request' AS label, " +
                    "'fa-ticket' AS icon,ticketid AS id, ticketuid AS uid, " +
                    "null AS parentid, null AS parentuid, " +
                    "null AS parentapplication, description AS summary," +
                    "CASE WHEN {2} > ticketuid THEN changeby ELSE reportedby " +
                    "END changeby, changedate, " +
                    "CONVERT(bigint, rowstamp) AS rowstamp FROM ticket sr " +
                    "WHERE changedate > DATEADD(HOUR,-{0},GETDATE()) AND " +
                    "changedate < '{1}' AND class='SR' "
                },
                {
                    "ssrCommlogs",
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
                    "createdate < '{1}'"
                },
                {
                    "ssrWorklogs",
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
                    "AND modifydate >  DATEADD(HOUR,-{0},GETDATE()) AND modifydate < '{1}'"
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
                    "AND changedate < '{1}' AND class='INCIDENT' "
                },
                {
                    "incidentCommlogs",
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
                    "createdate < '{1}'"
                },
                {
                    "incidentWorklogs",
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
                    "AND modifydate >  DATEADD(HOUR,-{0},GETDATE()) AND modifydate < '{1}'"
                },
                {
                    "workorders",
                    "SELECT 'workorder' AS application, " +
                    "'editdetail' AS targetschema, 'work order' AS label, " +
                    "'fa-wrench' AS icon, wonum AS id, workorderid AS uid, " +
                    "null AS parentid, null AS parentuid, " +
                    "null AS parentapplication, description AS summary, " +
                    "changeby, changedate, CONVERT(bigint, rowstamp) AS rowstamp " +
                    "FROM workorder WHERE changedate > DATEADD(HOUR,-{0},GETDATE()) " +
                    "AND changedate < '{1}' "
                },
                {
                    "workordersWorklogs",
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
                    "modifydate < '{1}'"
                }
            };
    }
}
