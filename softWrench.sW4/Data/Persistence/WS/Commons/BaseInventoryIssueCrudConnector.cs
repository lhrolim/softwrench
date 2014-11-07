using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class BaseInventoryIssueCrudConnector : CrudConnectorDecorator {


        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var invIssue = maximoTemplateData.IntegrationObject;

            /*FIND CORRECT VALUES FOR THESE FIELDS*/
            w.SetValueIfNull(invIssue, "ACTUALDATE", DateTime.Now.FromServerToRightKind());
            w.SetValueIfNull(invIssue, "SITEID", user.SiteId);
            /*FIND CORRECT VALUES FOR THESE FIELDS*/

            w.SetValueIfNull(invIssue, "TRANSDATE", DateTime.Now.FromServerToRightKind());
            w.SetValueIfNull(invIssue, "SENDERSYSID", "SW");
            
            base.BeforeCreation(maximoTemplateData);
        }



        //private void HandlePRLINES(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData crudDataEntity, object sr) {
        //    var prlines = (IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("prline");

        //    var recordKey = crudDataEntity.Id;
        //    var user = SecurityFacade.CurrentUser();
        //    w.CloneArray((IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("prline"), sr, "PRLINE",
        //        delegate(object integrationObject, CrudOperationData crudData) {
        //            if (ReflectionUtil.IsNull(integrationObject, "PRLINENUM")) {
        //                //Need to generate a unique PR Line number for each PR
        //                //Web sevice doesn't do it and hence do a native query to get the 
        //                // maximum value and increment it by 1. 
        //                BaseHibernateDAO _dao = new MaximoHibernateDAO();
        //                String queryst = "Select MAX(prline.prlinenum) from prline where prnum like ";
        //                var prnum = w.GetRealValue(sr, "PRNUM");
        //                queryst = queryst + Convert.ToString(prnum);
        //                var id = _dao.FindSingleByNativeQuery<object>(queryst, null);
        //                int prlinenum = Convert.ToInt32(id) + 1;
        //                w.SetValue(integrationObject, "PRLINENUM", prlinenum);
        //            }
        //            var enterdate = sr;
        //            w.SetValueIfNull(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind());
        //            w.SetValueIfNull(integrationObject, "TAX1", 0);
        //            w.SetValueIfNull(integrationObject, "TAX2", 0);
        //            w.SetValueIfNull(integrationObject, "TAX3", 0);
        //            w.SetValueIfNull(integrationObject, "TAX4", 0);
        //            w.SetValueIfNull(integrationObject, "TAX5", 0);
        //            w.SetValueIfNull(integrationObject, "ISSUE", false);
        //            w.SetValueIfNull(integrationObject, "CHARGESTORE", false);
        //            w.SetValueIfNull(integrationObject, "RECEIPTREQD", false);
        //            w.SetValueIfNull(integrationObject, "LOADEDCOST", 0);
        //            w.SetValueIfNull(integrationObject, "PRORATESERVICE", false);
        //            w.SetValueIfNull(integrationObject, "CONVERTTORFQ", false);
        //            w.SetValueIfNull(integrationObject, "INSPECTIONREQUIRED", false);
        //            w.SetValueIfNull(integrationObject, "ISDISTRIBUTED", false);
        //            w.SetValueIfNull(integrationObject, "LINECOST", 0);
        //            w.SetValueIfNull(integrationObject, "LINETYPE", "ITEM");
        //            w.SetValueIfNull(integrationObject, "ENTERBY", user.Login);
        //            w.SetValueIfNull(integrationObject, "SITEID", user.SiteId);
        //            w.SetValueIfNull(integrationObject, "ORGID", user.OrgId);
        //            w.SetValueIfNull(integrationObject, "ENTEREDASTASK", false);
        //            w.SetValueIfNull(integrationObject, "CONVERTTOCONTRACT", false);
        //            w.SetValueIfNull(integrationObject, "LANGCODE", "EN");
        //            w.SetValueIfNull(integrationObject, "CONVERSION", 1);
        //            w.SetValueIfNull(integrationObject, "HASID", false);
        //            w.SetValueIfNull(integrationObject, "PRLINENUM", 0);
        //            w.SetValueIfNull(integrationObject, "MKTPLCITEM", false);
        //            w.SetValueIfNull(integrationObject, "TAXEXEMPT", false);
        //            w.SetValueIfNull(integrationObject, "CONSIGNMENT", false);
        //            w.SetValueIfNull(integrationObject, "ITEMNUM", "test");

        //            ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
        //        });

        //}

    }
}
