using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class WorkLogHandler {

        private static AttachmentHandler _handler;

        private static AttachmentHandler AttachmentHandlerInstance() {
            return _handler ?? (_handler = new AttachmentHandler());
        }

        public static void HandleWorkLogs(CrudOperationData entity, object rootObject) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            var worklogs = ((IEnumerable<CrudOperationData>)entity.GetRelationship("worklog")).ToArray();
            
            WsUtil.CloneArray(worklogs, rootObject, "WORKLOG", delegate (object integrationObject, CrudOperationData crudData) {
                // Filter work order materials for any modified entries.  This is done by using the modifydate.  
                // Modifydate is null when detail schema is passed, which designate the record as updated or changed.  
                if (crudData.GetAttribute("modifydate") == null) {
                    WsUtil.SetValueIfNull(integrationObject, "worklogid", -1);
                    WsUtil.SetValue(integrationObject, "recordkey", recordKey);
                    WsUtil.SetValueIfNull(integrationObject, "class", entity.TableName);
                    WsUtil.SetValueIfNull(integrationObject, "createby", user.Login);
                    WsUtil.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");

                    WsUtil.CopyFromRootEntity(rootObject, integrationObject, "siteid", user.SiteId);
                    WsUtil.CopyFromRootEntity(rootObject, integrationObject, "orgid", user.OrgId);
                    WsUtil.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind(), "CHANGEDATE");

                    WsUtil.SetValue(integrationObject, "modifydate", DateTime.Now.FromServerToRightKind(), true);
                    ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                    LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);
                }

                // handle Attachments: only for new worklogs
                if (crudData.Id == null) {
                    var worklogContent = crudData.GetUnMappedAttribute("newattachment");
                    var worklogPath = crudData.GetUnMappedAttribute("newattachment_path");
                    if (string.IsNullOrWhiteSpace(worklogContent) || string.IsNullOrWhiteSpace(worklogPath)) return;
                    var attachmentParam = new AttachmentDTO() {
                        Data = worklogContent,
                        Path = worklogPath
                    };
                    try {
                        AttachmentHandlerInstance().AddAttachment(integrationObject, attachmentParam);
                    } catch (MaximoException e) {
                        throw new MaximoException("Could not attach image file. Please contact support about 'Installation Task [SWWEB-2156]'", e, ExceptionUtil.DigRootException(e));
                    }
                }
            });
        }
    }
}

