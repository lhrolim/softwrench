using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
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

            var dirtyEntries = ((IEnumerable<CrudOperationData>)entity.GetRelationship("multiassetlocci_")).Where(w => "true".EqualsIc(w.GetUnMappedAttribute("#isDirty"))).ToArray();

            WsUtil.CloneArray(dirtyEntries, rootObject, "MULTIASSETLOCCI", delegate (object integrationObject, CrudOperationData crudData) {
                var multiid = EntityRepository.GetNextEntityId(entity.EntityMetadata);
                WsUtil.SetValueIfNull(integrationObject, "multiid", multiid,false,true);
                WsUtil.SetValueIfNull(integrationObject, "ISPRIMARY", false);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "siteid", user.SiteId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "orgid", user.OrgId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind(), "CHANGEDATE");
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
            });
        }

    }
}
