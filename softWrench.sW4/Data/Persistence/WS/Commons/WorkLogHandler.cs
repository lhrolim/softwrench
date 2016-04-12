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

            // SWWEB-2365: send only edited or new worklogs
            var worklogs = ((IEnumerable<CrudOperationData>)entity.GetRelationship("worklog"))
                            .Where(w => w.UnmappedAttributes.ContainsKey("#isDirty"))
                            .ToArray();
            
            WsUtil.CloneArray(worklogs, rootObject, "WORKLOG", delegate (object integrationObject, CrudOperationData crudData) {
                WsUtil.SetValueIfNull(integrationObject, "worklogid", -1);
                WsUtil.SetValue(integrationObject, "recordkey", recordKey);
                WsUtil.SetValueIfNull(integrationObject, "class", entity.TableName);
                WsUtil.SetValueIfNull(integrationObject, "createby", user.Login);
                WsUtil.SetValueIfNull(integrationObject, "logtype", "CLIENTNOTE");

                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "siteid", user.SiteId);
                WsUtil.CopyFromRootEntity(rootObject, integrationObject, "orgid", user.OrgId);
                    
                WsUtil.SetValue(integrationObject, "modifydate", DateTime.Now.FromServerToRightKind(), true);
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                LongDescriptionHandler.HandleLongDescription(integrationObject, crudData);

                // create
                if (crudData.Id == null) {
                    WsUtil.CopyFromRootEntity(rootObject, integrationObject, "createdate", DateTime.Now.FromServerToRightKind(), "CHANGEDATE");
                    
                    // handle Attachments: only for new worklogs
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

