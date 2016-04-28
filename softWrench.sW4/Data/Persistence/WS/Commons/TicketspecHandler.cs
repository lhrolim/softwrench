using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Microsoft.Ajax.Utilities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public static class TicketspecHandler {
        // This is a mapping used to get/set the attributes on KOGT Project Request application.
        // The ticketspec assetattrid's are the keys and the application attributes are the values.
        public static Dictionary<string, string> ClassspecMap = new Dictionary<string, string>() {
            {"24 HOUR CONTACT PHONE", "contactphone"},
            {"ANTICIPATED START DATE", "anticipatestartdate"},
            {"CUSTOMER REP ALT PHONE", "custrepaltphone"},
            {"CUSTOMER REP EMAIL", "custrepemail"},
            {"CUSTOMER REP FULL NAME", "custrepfullname"},
            {"CUSTOMER REP PHONE", "custrepphone"},
            {"DIRECTIONAL DRILLING SERVICE PROVIDER", "ddservprov"},
            {"JOB DURATION", "jobduration"},
            {"LOCATION", "prlocation"},
            {"M/LWD SERVICE PROVIDER", "mlwdservprov"},
            {"PRODUCT SERVICE LINE", "prodservline"},
            {"PROJECT NUMBER", "projnum"},
            {"RIGSITE REP ALT PHONE", "rigsiterepaltphone"},
            {"RIGSITE REP EMAIL", "rigsiterepemail"},
            {"RIGSITE REP FULL NAME", "rigsiterepfullname"},
            {"RIGSITE REP PHONE", "rigsiterepphone"},
            {"SERVICE COMPANY", "servcomp"},
            {"SERVICE COMPANY REP", "servcomprep"},
            {"SERVICE COMPANY REP EMAIL", "servcomprepemail"},
            {"SERVICE COMPANY WITS VERSION", "servcompwitsversion"},
            {"SERVICE COMPANY WITSML VERSION", "servcompwitsmlversion"},
            {"SURFACE DATA LOGGING SERVICE PROVODER", "surfacedatalogservprov"},
            {"UNITSET", "unitset"},
            {"WELL SITE TIME ZONE", "wellsitetimezone"}
        };

        public static void HandleTicketspec(MaximoOperationExecutionContext maximoTemplateData) {
            
            var rootObject = maximoTemplateData.IntegrationObject;
            var entity = ((CrudOperationData)maximoTemplateData.OperationData);
            var ticketid = WsUtil.GetRealValue(rootObject, "TICKETID");
            if (ticketid == null) {
                return;
            }
            var unmappedAttributes = entity.UnmappedAttributes;
            var orgid = WsUtil.GetRealValue(rootObject, "orgid");
            var siteid = WsUtil.GetRealValue(rootObject, "siteid");
            UpdateTicketspecIds(ticketid.ToString(), orgid.ToString(), siteid.ToString(), unmappedAttributes);
            var classstructureid = WsUtil.GetRealValue(rootObject, "CLASSSTRUCTUREID").ToString();
            var ticketspecMetadata = MetadataProvider.Entity("ticketspec");
            

            var parsedOperationData = new List<CrudOperationData>();
            foreach (var classspec in ClassspecMap) {
                var attributes = new Dictionary<string, object>();

                attributes.Add("CLASSSTRUCTUREID", classstructureid);
                string classspecid;
                unmappedAttributes.TryGetValue(classspec.Value + "classspecid", out classspecid);
                attributes.Add("CLASSSPECID", classspecid);
                string ticketspecid;
                unmappedAttributes.TryGetValue(classspec.Value + "ticketspecid", out ticketspecid);
                attributes.Add("TICKETSPECID", ticketspecid);
                attributes.Add("SECTION", null);
                attributes.Add("ASSETATTRID", classspec.Key);
                var changedate = WsUtil.GetRealValue(rootObject, "changedate");
                attributes.Add("CHANGEDATE", changedate);
                var alnvalue = "";
                unmappedAttributes.TryGetValue(classspec.Value, out alnvalue);
                attributes.Add("ALNVALUE", alnvalue);

                var crudData = new CrudOperationData(ticketspecid, attributes, new Dictionary<string, object>(), ticketspecMetadata, entity.ApplicationMetadata);
                parsedOperationData.Add(crudData);
            }

            WsUtil.CloneArray(parsedOperationData, rootObject, "TICKETSPEC",
                delegate(object integrationObject, CrudOperationData crudData) {
                    ReflectionUtil.SetProperty(integrationObject, "action",
                        ticketid == null
                            ? ProcessingActionType.Add.ToString()
                            : ProcessingActionType.Change.ToString());
                    ReflectionUtil.SetProperty(integrationObject, "actionSpecified", true);
                    WsUtil.SetValue(integrationObject, "SECTION", null, true);
                });
        }

        public static void UpdateTicketspecIds(string ticketid, string orgid, string siteid, IDictionary<string, string> unmappedAttributes) {
            var ticketspecs = MaximoHibernateDAO.GetInstance().FindByNativeQuery("SELECT * FROM TICKETSPEC WHERE TICKETID = '{0}' AND ORGID = '{1}' AND SITEID = '{2}'".Fmt(ticketid, orgid, siteid));
            foreach (var ticketspec in ticketspecs) {
                string attributename;
                ClassspecMap.TryGetValue(ticketspec["assetattrid"], out attributename);
                string ticketspecid = ticketspec["ticketspecid"];
                unmappedAttributes[attributename + "ticketspecid"] = ticketspecid;
            }
        }
    }
}

