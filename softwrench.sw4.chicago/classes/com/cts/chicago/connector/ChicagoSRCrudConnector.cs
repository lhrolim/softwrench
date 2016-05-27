using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using System;
using System.Net;
using cts.commons.simpleinjector;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.connector {

    public class ChicagoSRCrudConnector : BaseServiceRequestCrudConnector {

        private const string ISMTicketId = "ismticketid";
        private const string ISMTicketUid = "ismticketuid";

        private IProblemManager _problemManager;

        private IProblemManager ProblemManager {
            get {
                if (_problemManager != null) {
                    return _problemManager;
                }
                _problemManager = SimpleInjectorGenericFactory.Instance.GetObject<IProblemManager>(typeof(IProblemManager));
                return _problemManager;
            }

        }


        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;

            if (w.GetRealValue(sr, "STATUS").Equals("INPROG")) {
                w.SetValueIfNull(sr, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            } else if (w.GetRealValue(sr, "STATUS").Equals("RESOLVED")) {
                w.SetValue(sr, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            } else if (w.GetRealValue(sr, "STATUS").Equals("CLOSED")) {
                w.SetValue(sr, "ITDCLOSEDATE", DateTime.Now.FromServerToRightKind());
            }

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterUpdate(maximoTemplateData);

            var crudOperationData = (CrudOperationData)maximoTemplateData.OperationData;

            if (crudOperationData.ContainsAttribute("underwaycall", true) || string.IsNullOrEmpty(ApplicationConfiguration.RestCredentialsUser)) {
                //avoid infinite loop
                return;
            }

            if (!crudOperationData.ContainsAttribute(ISMTicketUid, true)) {
                //instance already existed on service layer but not on ISM
                AfterCreation(maximoTemplateData);
            } else {
                try {
                    //updating ISM Entry which already exists
                    crudOperationData.SetAttribute("ticketuid", crudOperationData.GetAttribute(ISMTicketUid));
                    crudOperationData.SetAttribute("ticketid", crudOperationData.GetAttribute(ISMTicketId));

                    crudOperationData.SetAttribute("underwaycall", true);

                    var mifOperationWrapper = new OperationWrapper(crudOperationData,
                        OperationConstants.CRUD_UPDATE) {
                        Wsprovider = WsProvider.REST
                    };

                    ConnectorEngine.Execute(mifOperationWrapper);
                } catch (WebException e) {
                    var maximoException = MaximoException.ParseWebExceptionResponse((WebException)e);
                    ProblemManager.Register("servicerequest", maximoTemplateData.OperationData.UserId, null,
                        SecurityFacade.CurrentUser().DBId, maximoException.StackTrace, maximoException.Message);
                } catch (Exception e) {
                    ProblemManager.Register("servicerequest", maximoTemplateData.OperationData.UserId, null,
                        SecurityFacade.CurrentUser().DBId, e.StackTrace, e.Message);
                }
            }
        }

        /// <summary>
        /// The entry first is created locally. 
        /// Then, we need to create that same entry using Rest API. 
        /// Following we pick the generated ID at the ISM site and update it locally
        /// </summary>
        /// <param name="maximoTemplateData"></param>
        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterCreation(maximoTemplateData);
            var crudOperationData = (CrudOperationData)maximoTemplateData.OperationData;

            if (crudOperationData.ContainsAttribute("underwaycall", true) || string.IsNullOrEmpty(ApplicationConfiguration.RestCredentialsUser)) {
                //avoid infinite loop
                return;
            }

            //entry was created locally, now we need to create it on ISM, using a rest call
            var result = maximoTemplateData.ResultObject;

            var originalTicketid = crudOperationData.GetAttribute("ticketid");
            var originalTicketuid = crudOperationData.GetAttribute("ticketuid");

            crudOperationData.SetAttribute("ticketid", null);
            crudOperationData.SetAttribute("ticketuid", null);

            crudOperationData.SetAttribute("underwaycall", true);

            var originalId = crudOperationData.Id;

            crudOperationData.Id = null;

            //updating ISM instance using REST --> no ticketid, ticketuid, they would be generated out there
            var operationWrapper = new OperationWrapper(crudOperationData,
                OperationConstants.CRUD_CREATE) {
                Wsprovider = WsProvider.REST
            };

            var restResult = ConnectorEngine.Execute(operationWrapper);

            //updating local entry, reset the ticketids

            crudOperationData.SetAttribute("ticketid", originalTicketid);
            crudOperationData.SetAttribute("ticketuid", originalTicketuid);

            crudOperationData.Id = originalId;

            crudOperationData.SetAttribute(ISMTicketUid, restResult.Id);
            crudOperationData.SetAttribute(ISMTicketId, restResult.UserId);

            var mifOperationWrapper = new OperationWrapper(crudOperationData,
                OperationConstants.CRUD_UPDATE) {
            };

            ConnectorEngine.Execute(mifOperationWrapper);





        }
    }
}
