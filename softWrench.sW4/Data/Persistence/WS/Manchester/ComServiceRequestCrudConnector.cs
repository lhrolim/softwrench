using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Services;
using softWrench.sW4.wsAsset;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Manchester
{
    class ComServiceRequestCrudConnector : BaseServiceRequestCrudConnector
    {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData)
        {
            base.BeforeUpdate(maximoTemplateData);
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            HandleTKServiceAddress(crudData, sr);
        }
        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData)
        {
            base.BeforeCreation(maximoTemplateData);
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
        }
        public static T HandleTKServiceAddress<T>(CrudOperationData entity, T rootObject)
        {
            var user = SecurityFacade.CurrentUser();
            var saddresscode = entity.GetUnMappedAttribute("saddresscode");
            var description = entity.GetUnMappedAttribute("#tkdesc");
            var formattedaddr = entity.GetUnMappedAttribute("#tkformattedaddress");
            var streetnumber = entity.GetUnMappedAttribute("#tkstaddrnumber");
            var streetaddr = entity.GetUnMappedAttribute("#tkstaddrstreet");
            var streettype = entity.GetUnMappedAttribute("#tkstaddrsttype");
            var tkserviceaddress = ReflectionUtil.InstantiateSingleElementFromArray(rootObject, "TKSERVICEADDRESS");
            w.SetValueIfNull(tkserviceaddress, "TKSERVICEADDRESSID", -1);
            w.SetValue(tkserviceaddress, "ORGID", user.OrgId);
            w.SetValue(tkserviceaddress, "SADDRESSCODE", saddresscode);
            w.SetValue(tkserviceaddress, "DESCRIPTION", description);
            w.SetValue(tkserviceaddress, "FORMATTEDADDRESS", formattedaddr);
            w.SetValue(tkserviceaddress, "STADDRNUMBER", streetnumber);
            w.SetValue(tkserviceaddress, "STADDRSTREET", streetaddr);
            w.SetValue(tkserviceaddress, "STADDRSTTYPE", streettype);
            return rootObject;
        }
    }
}
