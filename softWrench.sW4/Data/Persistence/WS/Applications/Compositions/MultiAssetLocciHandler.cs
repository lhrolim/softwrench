using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.mif_sr;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class MultiAssetLocciHandler {

        public static void HandleMultiAssetLoccis(CrudOperationData entity, object rootObject) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            var dirtyEntries = ((IEnumerable<CrudOperationData>)entity.GetRelationship("multiassetlocci_")).Where(w => "true".EqualsIc(w.GetStringAttribute("#isDirty"))).ToArray();

            WsUtil.CloneArray(dirtyEntries, rootObject, "MULTIASSETLOCCI", delegate(object integrationObject, CrudOperationData crudData) {

                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "siteid", user.SiteId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "orgid", user.OrgId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind(), "CHANGEDATE");
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                
            });
        }

    }
}
