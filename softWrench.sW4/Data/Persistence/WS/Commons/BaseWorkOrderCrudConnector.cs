using Newtonsoft.Json;
﻿using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.wsWorkorder;
using System;
using System.Collections.Generic;
using System.Linq;
using r = softWrench.sW4.Util.ReflectionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BaseWorkOrderCrudConnector : CrudConnectorDecorator {
        //        private const string _dbwostatusKey = "dbwostatus";
        //        private const string _oldlongdescriptionKey = "oldlongdescriptionKey";
        //        private const string _newlongdescriptionKey = "newlongdescriptionKey";
        //        private const string _notFoundLog = "{0} {1} not found. Impossible to generate FollowUp Workorder";

        protected AttachmentHandler _attachmentHandler;
        protected MaximoHibernateDAO _maxHibernate; 

        public BaseWorkOrderCrudConnector() {
            _attachmentHandler = new AttachmentHandler();
            _maxHibernate = MaximoHibernateDAO.GetInstance(); 
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            if (((CrudOperationData)maximoTemplateData.OperationData).ContainsAttribute("#hasstatuschange")) {
                //first let´s 'simply change the status
                WsUtil.SetValue(maximoTemplateData.IntegrationObject, "STATUSIFACE", true);
                WsUtil.SetValue(maximoTemplateData.IntegrationObject, "CHANGEBY", user.Login.ToUpper());
                if (!WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "STATUS").Equals("CLOSE") || !WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "STATUS").Equals("CAN")) {
                    maximoTemplateData.InvokeProxy();
                }
                WsUtil.SetValue(maximoTemplateData.IntegrationObject, "STATUSIFACE", false);
            }

            // COMSW-52 Auto-populate the actual start and end time for a workorder depending on status change
            // TODO: Caching the status field to prevent multiple SQL update.  
            if (((CrudOperationData)maximoTemplateData.OperationData).ContainsAttribute("#hasstatuschange")) {
                var maxStatusValue = _maxHibernate.FindSingleByNativeQuery<string>(String.Format("SELECT MAXVALUE FROM SYNONYMDOMAIN WHERE DOMAINID = 'WOSTATUS' AND VALUE = '{0}'", WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "STATUS")), null);
                if (maxStatusValue.Equals("INPRG")) {
                    // We might need to update the client database and cycle the server: update MAXVARS set VARVALUE=1 where VARNAME='SUPPRESSACTCHECK';
                    // Actual date must be in the past - thus we made it a minute behind the current time.   
                    // More info: http://www-01.ibm.com/support/docview.wss?uid=swg1IZ90431
                    WsUtil.SetValueIfNull(maximoTemplateData.IntegrationObject, "ACTSTART", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                }
                else if (maxStatusValue.Equals("COMP")) {
                    // Actual date must be in the past - thus we made it a minute behind the current time.   
                    WsUtil.SetValueIfNull(maximoTemplateData.IntegrationObject, "ACTSTART", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                    WsUtil.SetValueIfNull(maximoTemplateData.IntegrationObject, "ACTFINISH", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                }
            }

            CommonTransaction(maximoTemplateData);

            // This will prevent multiple action on these items
            WorkLogHandler.HandleWorkLogs((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject);
            HandleMaterials((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject);
            HandleLabors((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject);
            HandleTools((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject);
            HandleAttachments((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject, maximoTemplateData.ApplicationMetadata);
            
            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
            base.BeforeCreation(maximoTemplateData);
        }

        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterUpdate(maximoTemplateData);

            ((CrudOperationData)maximoTemplateData.OperationData).Fields["wonum"] = maximoTemplateData.ResultObject.UserId;
            maximoTemplateData.OperationData.Id = maximoTemplateData.ResultObject.UserId;
            maximoTemplateData.OperationData.OperationType = Internal.OperationType.AddChange;

            // Resubmitting MIF for ServiceAddress Update
            ConnectorEngine.Update((CrudOperationData)maximoTemplateData.OperationData);
        }



        private void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            var wo = maximoTemplateData.IntegrationObject;
            WsUtil.SetValueIfNull(wo, "ESTDUR", 0);
            WsUtil.SetValueIfNull(wo, "ESTLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTMATCOST", 0);

            WsUtil.SetValueIfNull(wo, "ESTINTLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTINTLABCOST", 0);

            WsUtil.SetValueIfNull(wo, "ESTATAPPRLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRSERVCOST", 0);

            WsUtil.SetValueIfNull(wo, "ESTAPPRLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTAPPRMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTAPPRLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "WOCLASS", "WORKORDER");
            WsUtil.SetValueIfNull(wo, "ESTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTSERVCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ACTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTSERVCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTSERVCOST", 0);

            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData);
            HandleServiceAddress((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject);
        }



        protected virtual void HandleLabors(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order materials for any new entries where matusetransid is null
            var Labors = (IEnumerable<CrudOperationData>)entity.GetRelationship("labtrans");
            var newLabors = Labors.Where(r => r.GetAttribute("labtransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newLabors as CrudOperationData[] ?? newLabors.ToArray();

            WsUtil.CloneArray(crudOperationData, wo, "LABTRANS", delegate(object integrationObject, CrudOperationData crudData) {

                if (ReflectionUtil.IsNull(integrationObject, "LABTRANSID")) {
                    WsUtil.SetValue(integrationObject, "LABTRANSID", -1);
                }

                WsUtil.SetValue(integrationObject, "REFWO", recordKey);
                WsUtil.SetValue(integrationObject, "TRANSTYPE", "WORK");
                WsUtil.SetValueIfNull(integrationObject, "SITEID", user.SiteId);
                WsUtil.SetValueIfNull(integrationObject, "ORGID", user.OrgId);
                WsUtil.SetValueIfNull(integrationObject, "LABORCODE", user.Login.ToUpper());
                WsUtil.SetValueIfNull(integrationObject, "ENTERBY", user.Login.ToUpper());
                WsUtil.SetValueIfNull(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValueIfNull(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValueIfNull(integrationObject, "PAYRATE", 0.0); 

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }

        protected virtual void HandleMaterials(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order materials for any new entries where matusetransid is null
            var Materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
            var newMaterials = Materials.Where(r => r.GetAttribute("matusetransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newMaterials as CrudOperationData[] ?? newMaterials.ToArray();
            
            WsUtil.CloneArray(crudOperationData, wo, "MATUSETRANS", delegate(object integrationObject, CrudOperationData crudData) {
                
                WsUtil.SetValueIfNull(integrationObject, "QTYREQUESTED", 0);
                WsUtil.SetValueIfNull(integrationObject, "UNITCOST", 0);

                var itemtype = WsUtil.GetRealValue(integrationObject, "LINETYPE");
                var quantity = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");
                var unitcost = (double)WsUtil.GetRealValue(integrationObject, "UNITCOST");

                if (itemtype.Equals("ITEM")) {
                    WsUtil.SetValue(integrationObject, "DESCRIPTION", crudData.UnmappedAttributes["#description"]); 
                }

                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "ACTUALDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "QUANTITY", -1 * quantity);
                WsUtil.SetValueIfNull(integrationObject, "UNITCOST", 0);
                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);
                WsUtil.SetValueIfNull(integrationObject, "DESCRIPTION", "");
                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);

                WsUtil.SetValueIfNull(integrationObject, "ISSUETYPE", "ISSUE");
                WsUtil.SetValue(integrationObject, "MATUSETRANSID", -1);

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }

        protected virtual void HandleTools(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order materials for any new entries where matusetransid is null
            var Tools = (IEnumerable<CrudOperationData>)entity.GetRelationship("tooltrans");
            var newTools = Tools.Where(r => r.GetAttribute("tooltransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newTools as CrudOperationData[] ?? newTools.ToArray();

            WsUtil.CloneArray(crudOperationData, wo, "TOOLTRANS", delegate(object integrationObject, CrudOperationData crudData) {
                WsUtil.SetValueIfNull(integrationObject, "TOOLRATE", 0.00);
                WsUtil.SetValueIfNull(integrationObject, "TOOLQTY", 0);
                WsUtil.SetValueIfNull(integrationObject, "TOOLHRS", 0); 
                
                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);

                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);
                WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind(), true);

                WsUtil.SetValue(integrationObject, "TOOLTRANSID", -1);
                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }

        protected virtual void HandleServiceAddress(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Create a new WOSERVICEADDRESS instance created
            var woserviceaddress = ReflectionUtil.InstantiateSingleElementFromArray(wo, "WOSERVICEADDRESS");
            
            // Extract data from unmapped attribute
            var json = entity.GetUnMappedAttribute("#woaddress_");

            // If empty, we assume there's no selected data.  
            if (json != null) {
                dynamic woaddress = JsonConvert.DeserializeObject(json);

                String addresscode = woaddress.addresscode;
                String desc = woaddress.description;
                String straddrnumber = woaddress.staddrnumber;
                String straddrstreet = woaddress.staddrstreet;
                String straddrtype = woaddress.staddrtype;

                WsUtil.SetValue(woserviceaddress, "SADDRESSCODE", addresscode);
                WsUtil.SetValue(woserviceaddress, "DESCRIPTION", desc);
                WsUtil.SetValue(woserviceaddress, "STADDRNUMBER", straddrnumber);
                WsUtil.SetValue(woserviceaddress, "STADDRSTREET", straddrstreet);
                WsUtil.SetValue(woserviceaddress, "STADDRSTTYPE", straddrtype);
            }
            else {
                WsUtil.SetValueIfNull(woserviceaddress, "STADDRNUMBER", "");
                WsUtil.SetValueIfNull(woserviceaddress, "STADDRSTREET", "");
                WsUtil.SetValueIfNull(woserviceaddress, "STADDRSTTYPE", "");
            }

            var prevWOServiceAddress = entity.GetRelationship("woserviceaddress");

            if (prevWOServiceAddress != null) {
                WsUtil.SetValue(woserviceaddress, "FORMATTEDADDRESS", ((CrudOperationData)prevWOServiceAddress).GetAttribute("formattedaddress") ?? "");
            }

            //WsUtil.SetValueIfNull(woserviceaddress, "WOSERVICEADDRESSID", -1);          
            WsUtil.SetValue(woserviceaddress, "ORGID", user.OrgId);
            WsUtil.SetValue(woserviceaddress, "SITEID", user.SiteId);

            ReflectionUtil.SetProperty(woserviceaddress, "action", OperationType.AddChange.ToString());
        }

        protected virtual void HandleAttachments(CrudOperationData entity, object wo, ApplicationMetadata metadata) {
            var user = SecurityFacade.CurrentUser();

            var attachmentParam = new AttachmentParameters() { 
                Data = entity.GetUnMappedAttribute("newattachment"),
                Path = entity.GetUnMappedAttribute("newattachment_path")
            };
            if (!String.IsNullOrWhiteSpace(attachmentParam.Data) && !String.IsNullOrWhiteSpace(attachmentParam.Path)) {
                _attachmentHandler.HandleAttachments(wo, attachmentParam, metadata);
            }

            var screenshotParam = new AttachmentParameters() {
                Data = entity.GetUnMappedAttribute("newscreenshot"),
                Path = "screen" + DateTime.Now.ToUserTimezone(user).ToString("yyyyMMdd") + ".png"
            };
            if (!String.IsNullOrWhiteSpace(screenshotParam.Data) && !String.IsNullOrWhiteSpace(screenshotParam.Path)) {
                _attachmentHandler.HandleAttachments(wo, attachmentParam, metadata);
            }

            var attachments = entity.GetRelationship("attachment");
            if (attachments != null) {
                // this will only filter new attachments
                foreach (var attachment in ((IEnumerable<CrudOperationData>)attachments).Where(a => a.Id == null)) {
                    var docinfo = (CrudOperationData)attachment.GetRelationship("docinfo");
                    var title = attachment.GetAttribute("document").ToString();
                    var desc = docinfo != null && docinfo.Fields["description"] != null ? docinfo.Fields["description"].ToString() : ""; 
                    var content = new AttachmentParameters() {
                        Title = title,
                        Data = attachment.GetUnMappedAttribute("newattachment"),
                        Path = attachment.GetUnMappedAttribute("newattachment_path"),
                        Description = desc
                    };

                    if (content.Data != null) {
                        _attachmentHandler.HandleAttachments(wo, content, metadata);
                    }
                }
            }
        }
    }
}
