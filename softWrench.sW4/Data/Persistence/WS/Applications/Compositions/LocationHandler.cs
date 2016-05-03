using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.wsWorkorder;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions
{

    class LocationHandler
    {

        public static void HandleLocation(CrudOperationData entity, object rootObject)
        {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Filter work order materials for any modified entries.  This is done by using the modifydate.  
            // Modifydate is null when detail schema is passed, which designate the record as updated or changed.  
            var Location = entity.GetRelationship("location");
            WsUtil.CloneSingle((CrudOperationData)Location,rootObject, "LOCATIONS", delegate(object integrationObject, CrudOperationData crudData) {
                
                WsUtil.SetValue(integrationObject, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "CHANGEBY", user.Login);

                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
            });
        }
    }
}

