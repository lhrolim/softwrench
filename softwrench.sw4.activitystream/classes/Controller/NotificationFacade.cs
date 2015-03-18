using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.Shared2.Metadata.Applications.Notification;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sw4.activitystream.classes.Controller {
    public class NotificationFacade : ISingletonComponent {

        private const int HoursToPurge = 24;

        public static readonly IDictionary<string, InMemoryNotificationStream> NotificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();
        public static IDictionary<string, long> Counter = new ConcurrentDictionary<string, long>();

        private readonly MaximoHibernateDAO MaxDAO;

        public NotificationFacade(MaximoHibernateDAO maxDAO) {
            MaxDAO = maxDAO;
        }

        //Sets up the default notification stream.
        public void InitNotificationStreams() {
            var allRoleNotificationBuffer = new InMemoryNotificationStream();
            var query =
                string.Format(
                    "select max(ticketuid) as max, 'servicerequest' as application from ticket where class ='SR' union " +
                    "select max(ticketuid) as max, 'incident' as application from ticket where class ='INCIDENT' union " +
                    "select max(workorderid) as max, 'workoder' as application from workorder union " +
                    "select max(commloguid) as max, 'commlog' as application from commlog union " +
                    "select max(worklogid) as max, 'worklog' as application from worklog");
            var result = MaxDAO.FindByNativeQuery(query, null);
            foreach (var record in result) {
                Counter.Add(record["application"], Int32.Parse((record["max"] ?? "0")));
            }
            NotificationStreams["allRole"] = allRoleNotificationBuffer;
        }

        public static InMemoryNotificationStream CurrentNotificationStream() {
            return NotificationStreams["allRole"];
        }

        //Currently only inserts notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be inserted based on which roles have a
        //notificationstream attribute set to true
        public void InsertNotificationsIntoStreams(Iesi.Collections.Generic.ISet<Notification> notifications) {
            var streamToUpdate = NotificationStreams["allRole"];
            foreach (var notification in notifications) {
                streamToUpdate.InsertNotificationIntoStream(notification);
            }
        }

        //Currently only updates notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be updated based on which roles have a
        //notificationstream attribute set to true
        public void UpdateNotificationReadFlag(string role, string application, string id, long rowstamp, bool isRead) {
            var streamToUpdate = NotificationStreams[role];
            streamToUpdate.UpdateNotificationReadFlag(application, id, rowstamp, isRead);
        }

        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(string role, JArray notifications, bool isRead) {
            var streamToUpdate = NotificationStreams[role];

            foreach (var notification in notifications) {
                streamToUpdate.UpdateNotificationReadFlag(notification["application"].ToString(),
                    notification["id"].ToString(), Convert.ToInt64(notification["rowstamp"]), true);
            }
        }

        public void UpdateNotificationStreams()
        {
	        var slicedMetadataEntities = MetadataProvider.GetSlicedMetadataNotificationEntities();
            var currentTime = DateTime.Now.FromServerToRightKind();

            var streamToUpdate = NotificationStreams["allRole"];
            //foreach (var slicedEntity in slicedMetadataEntities) {

            //    var searchRequestDTO = new SearchRequestDto();
            //    foreach (var field in slicedEntity.AppSchema.Fields) {
            //        searchRequestDTO.AppendProjectionField(new ProjectionField(field.Label, field.Attribute));
            //    }
            //    searchRequestDTO.AppendProjectionField(new ProjectionField(slicedEntity.IdFieldName, slicedEntity.IdFieldName));
                
            //    searchRequestDTO.AppendProjectionField(new ProjectionField("rowstamp", "rowstamp"));
            //    var createddateFieldAlias = (from p in searchRequestDTO.ProjectionFields
            //                                 where p.Alias == "createddate"
            //                                 select p.Name).Single();
            //    var createddateField = slicedEntity.Name + "." + createddateFieldAlias;

            //    var createddateWhereClauseStr = String.Format("{0} > DATEADD(HOUR,-{1}, GETDATE()) and {0} <= '{2}'", createddateField,
            //        HoursToPurge, currentTime);
            //    searchRequestDTO.AppendWhereClause(createddateWhereClauseStr);
            //    EntityRepository entityRepo = new EntityRepository(null, MaxDAO);
            //    var resultList = entityRepo.Get(slicedEntity, searchRequestDTO);

            //    foreach (var result in resultList) {
            //        var notificationSchema = (ApplicationNotificationDefinition)slicedEntity.AppSchema;
            //        var application = notificationSchema.TargetApplication;
            //        var targetschema = notificationSchema.TargetSchema;
            //        var id = result.Attributes[slicedEntity.IdFieldName].ToString();
            //        var label = notificationSchema.Label;
            //        var icon = notificationSchema.Icon;

            //        long uId;
            //        Int64.TryParse(result.Attributes["uid"].ToString(), out uId);

            //        var flag = "changed";
            //        if (Counter[application] < uId) {
            //            flag = "created";
            //            Counter[application] = uId;
            //        }

            //        string parentid = null;
            //        if (result.Attributes.ContainsKey("parentid")) {
            //            parentid = result.Attributes["parentid"].ToString();    
            //        }
                    
            //        long parentuid = -1;
            //        if (result.Attributes.ContainsKey("parentuid")) {
            //            Int64.TryParse(result.Attributes["parentuid"].ToString(), out parentuid);    
            //        }


            //        string parentapplication = null;
            //        string parentlabel = null;
            //        if (result.Attributes.ContainsKey("parentid") || result.Attributes.ContainsKey("parentuid")) {
            //            parentapplication = result.Attributes["targetapplication"].ToString();


            //            if (parentapplication == "servicerequest") {
            //                parentlabel = "service request";
            //            } else if (parentapplication == "WORKORDER") {
            //                parentlabel = "work order";
            //            } else if (parentapplication == "INCIDENT") {
            //                parentlabel = "incident";
            //            }
            //        }
            //        var summary = result.Attributes["summary"].ToString();
            //        var changeby = result.Attributes["changeby"].ToString();
            //        var changedate = DateTime.Parse(result.Attributes["createddate"].ToString());
            //        var rowstamp = Convert.ToInt64(result.Attributes["rowstamp"]);
            //        var notification = new Notification(application, targetschema, label, icon, id, uId, parentid, parentuid, parentapplication, parentlabel, summary, changeby, changedate, rowstamp, flag);
            //        streamToUpdate.InsertNotificationIntoStream(notification);
            //    }
            //}

            
            var hardcodedQuery = string.Format(
                                      "select 'commlog' as application, null as targetschema,'communication' as label, 'fa-envelope-o' as icon ,CONVERT(varchar(10), commlogid) as id, c.commloguid as uid, " +
                                      "t.ticketid as parentid, c.ownerid as parentuid, CASE c.ownertable WHEN 'SR' THEN 'servicerequest' ELSE c.ownertable END as parentapplication, c.subject as summary, " +
                                      "c.createby as changeby, c.createdate as changedate, CONVERT(bigint, c.rowstamp) as rowstamp from commlog c " +
                                      "left join ticket t on t.ticketuid = c.ownerid " +
                                      "where createdate >  DATEADD(HOUR,-{0},GETDATE()) and createdate < '{1}' union " + 
                                      "select 'worklog' as application, null as targetschema, 'work log' as label, 'fa fa-wrench' as icon, CONVERT(varchar(10), l.worklogid) as id, CONVERT(varchar(10), l.worklogid) as uid, l.recordkey as parentid, t.ticketuid as parentuid, " +
                                      "CASE l.class WHEN 'SR' THEN 'servicerequest' ELSE l.class END AS parentapplication, l.description as summary, " +
                                      "l.createby as changeby, l.modifydate as changedate, CONVERT(bigint, l.rowstamp) as rowstamp from worklog l " +
                                      "left join ticket t on t.ticketid = l.recordkey " + 
                                      " where l.class in ('SR','INCIDENT') and logtype = 'clientnote' and " +
                                      "modifydate >  DATEADD(HOUR,-{0},GETDATE()) and modifydate < '{1}' union " +
                                      "select 'worklog' as application, null as targetschema, 'work log' as label, 'fa fa-wrench' as icon, CONVERT(varchar(10), l.worklogid) as id, CONVERT(varchar(10), l.worklogid) as uid, l.recordkey as parentid, w.workorderid as parentuid, " +
                                      "CASE l.class WHEN 'WORKORDER' THEN 'WORKORDER' ELSE l.class END AS parentapplication, l.description as summary, " +
                                      "l.createby as changeby, l.modifydate as changedate, CONVERT(bigint, l.rowstamp) as rowstamp from worklog l " +
                                      "left join workorder w on w.wonum = l.recordkey " +
                                      "where class in ('WORKORDER') and logtype = 'clientnote' and modifydate >  DATEADD(HOUR,-{0},GETDATE()) and modifydate < '{1}' union " +
                                      "select 'servicerequest' as application, 'editdetail' as targetschema, 'service request' as label, 'fa-ticket' as icon,ticketid as id, ticketuid as uid, null as parentid, null as parentuid, null as parentapplication, description as summary," +
                                      "changeby, changedate, CONVERT(bigint, rowstamp) as rowstamp from ticket " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < '{1}' and class='SR' union " +
                                      "select 'incident' as application, 'editdetail' as targetschema, 'incident' as label, 'fa-warning' as icon,ticketid as id, ticketuid as uid, null as parentid, null as parentuid, null as parentapplication, description as summary," +
                                      "changeby, changedate, CONVERT(bigint, rowstamp) as rowstamp from ticket " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < '{1}' and class='INCIDENT' union " +
                                      "select 'workorder' as application, 'editdetail' as targetschema, 'work order' as label, 'fa-wrench' as icon,wonum as id, workorderid as uid, null as parentid, null as parentuid, null as parentapplication, description as summary, " +
                                      "changeby, changedate, CONVERT(bigint, rowstamp) as rowstamp from workorder " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < '{1}' " +
                                      "order by rowstamp desc", HoursToPurge, currentTime);

            var hardcodedQueryResult = MaxDAO.FindByNativeQuery(hardcodedQuery, null);

            foreach (var record in hardcodedQueryResult)
            {
                var application = record["application"];
                var targetschema = record["targetschema"];
                var id = record["id"];
                var label = record["label"];
                var icon = record["icon"];
                var uid = Int64.Parse(record["uid"]);
                var flag = "changed";
                if (Counter[application] < uid) {
                    flag = "created";
                    Counter[application] = uid;
                }
                var parentid = record["parentid"];
                long parentuid = -1;
                if (record["parentuid"] != null) { 
                    Int64.TryParse(record["parentuid"], out parentuid);
                }
                var parentapplication = record["parentapplication"];
                string parentlabel = null;
                if (parentapplication == "servicerequest") {
                    parentlabel = "service request";
                } else if (parentapplication == "WORKORDER") {
                    parentlabel = "work order";
                } else if (parentapplication == "INCIDENT") {
                    parentlabel = "incident";
                }
                var summary = record["summary"];
                var changeby = record["changeby"];
                var changedate = DateTime.Parse(record["changedate"]);
                var rowstamp = Convert.ToInt64(record["rowstamp"]);
                var notification = new Notification(application, targetschema, label, icon, id, uid, parentid, parentuid, parentapplication, parentlabel, summary, changeby, changedate, rowstamp, flag);
                streamToUpdate.InsertNotificationIntoStream(notification);
            }
        }


        //By default this is only purging the allRole notification stream
        //by any record changed more than 24 hours ago
        public void PurgeNotificationsFromStream() {
            var streamToUpdate = NotificationStreams["allRole"];
            streamToUpdate.PurgeNotificationsFromStream(24);
        }
        
        public NotificationResponse GetNotificationStream(string role) {
            return NotificationStreams[role].GetNotifications();
        }
    }
}
