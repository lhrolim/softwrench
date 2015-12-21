using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softWrench.sW4.Data.Persistence.WS.AMEX {
   public class AmexAssetConnectors : CrudConnectorDecorator {
       public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData)
       {
           base.BeforeUpdate(maximoTemplateData);
           var assetObject = maximoTemplateData.IntegrationObject;
           WsUtil.SetValue(assetObject, "status", "REC ASSET MGT");
       }
   }
}
