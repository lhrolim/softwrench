using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class RelatedRecordHandler {


        public static void HandleRelatedRecords(MaximoOperationExecutionContext maximoOperation) {
            var srData = (CrudOperationData)maximoOperation.OperationData;
            var relatedRecords = ((IEnumerable<CrudOperationData>)srData.GetRelationship("relatedrecord_"))
                                    .Where(r => r.IsDirty)
                                    .ToList();

            if (!relatedRecords.Any()) return;

            var ticket = maximoOperation.IntegrationObject;
            var user = SecurityFacade.CurrentUser();

            w.CloneArray(relatedRecords, ticket, "RELATEDRECORD", (relatedRecord, relatedRecordData) => {
                w.SetValue(relatedRecord, "RELATEDRECORDID", -1);
                w.SetValue(relatedRecord, "RELATETYPE", "RELATED");
                // current SR data
                w.SetValue(relatedRecord, "RECORDKEY", srData.UserId);
                w.CopyFromRootEntity(ticket, relatedRecord, "SITEID", user.SiteId);
                w.CopyFromRootEntity(ticket, relatedRecord, "ORGID", user.OrgId);
                // related target data
                var relatedClass = relatedRecordData.GetStringAttribute("relatedrecclass").ToUpper();
                if (relatedClass.Equals("WORKORDER")) {
                    w.SetValueIfNull(relatedRecord, "RELATEDRECWOCLASS", relatedClass);
                    w.SetValueIfNull(relatedRecord, "RELATEDRECWONUM", relatedRecordData.GetAttribute("relatedreckey"));
                    //in case of workorder we cannot pass these values, but rather the WO specific ones
                    w.NullifyValue(relatedRecord, "RELATEDRECCLASS");
                    w.NullifyValue(relatedRecord, "RELATEDRECKEY");
                }
            });
        }

    }
}
