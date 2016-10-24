using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.pae.classes.com.cts.pae.connector {
    public class PaePrCrudConnector : BasePurchaseRequestCrudConnector {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var pr = maximoTemplateData.IntegrationObject;
            var user = SecurityFacade.CurrentUser();
            // This workaround required trigger in the Maximo DB and custom attribute "SWCHANGEBY" in pr
            w.SetValue(pr, "SWCHANGEBY", user.Login);
            base.BeforeUpdate(maximoTemplateData);
        }
    }
}
