using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.connector {

    public class ChicagoSRCrudConnector : BaseServiceRequestCrudConnector {

        private const string ISMTicketId = "ismticketid";
        private const string ISMTicketUid = "ismticketuid";

        private static readonly ILog Log = LogManager.GetLogger(typeof(ChicagoSRCrudConnector));

        private IProblemManager ProblemManager {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<IProblemManager>(typeof(IProblemManager));
            }
        }

        private IMaximoHibernateDAO MaximoDAO {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>(typeof(IMaximoHibernateDAO));
            }
        }

        public ChicagoSRCrudConnector() {
            Log.Debug("init log...");
        }


        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;

            SetSwChangeBy(sr);

            var statusValue = HandleActualDates(maximoTemplateData);
            if (statusValue != null && statusValue.Equals("CLOSED") && w.GetRealValue(sr, "ITDCLOSEDATE") == null) {
                w.SetValue(sr, "ITDCLOSEDATE", DateTime.Now.FromServerToRightKind());
                SetReloadAfterSave(maximoTemplateData);
            }

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            SetSwChangeBy(sr);
            base.BeforeCreation(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterUpdate(maximoTemplateData);

            var crudOperationData = (CrudOperationData)maximoTemplateData.OperationData;

            if (crudOperationData.ContainsAttribute("underwaycall", true) || !MaximoRestUtils.IsRestSetup()) {
                Log.DebugFormat("returning from underway call");
                //avoid infinite loop
                return;
            }

            if (!crudOperationData.ContainsAttribute(ISMTicketUid, true)) {
                Log.InfoFormat("creating new ticket at ISM side");
                //instance already existed on service layer but not on ISM
                AfterCreation(maximoTemplateData);
            } else {
                var originalTicketUid = crudOperationData.Id;
                var originalTicketid = crudOperationData.UserId;
                

                //updating ISM Entry which already exists
                var ismTicketUid = crudOperationData.GetAttribute(ISMTicketUid);
                crudOperationData.SetAttribute("ticketuid", ismTicketUid);
                var ismTicketId = crudOperationData.GetAttribute(ISMTicketId);
                crudOperationData.SetAttribute("ticketid", ismTicketId);

                Log.InfoFormat("updating already existing ticket at ism for {0}, ISM : {1}", originalTicketid, ISMTicketId);

                //also required for all the rest operations
                crudOperationData.SetAttribute("pluspcustomer", "CPS-00");

                crudOperationData.SetAttribute("underwaycall", true);

                crudOperationData.Id = ismTicketUid as string;

                try {
                    var mifOperationWrapper = new OperationWrapper(crudOperationData,
                        OperationConstants.CRUD_UPDATE) {
                        Wsprovider = WsProvider.REST
                    };
                    ConnectorEngine.Execute(mifOperationWrapper);
                } catch (Exception e) {

                    var schemaId = maximoTemplateData.ApplicationMetadata.Name == "servicerequest"
                        ? "editdetail"
                        : "quickeditdetail";

                    ProblemManager.RegisterOrUpdateProblem(
                        // ReSharper disable once PossibleInvalidOperationException
                        SecurityFacade.CurrentUser().UserId.Value,
                        Problem.BaseProblem(maximoTemplateData.ApplicationMetadata.Name, schemaId, originalTicketUid, originalTicketid, e.StackTrace, e.Message, "ismrestsync"),
                        () => Problem.ByEntryAndType.Fmt(originalTicketUid, "servicerequest", "ismrestsync")
                        );
                    maximoTemplateData.ResultObject.WarningDto = new ErrorDto(e) {
                        WarnMessage = "Failed to Sync ticket to ISM. An Error has been created"
                    };
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

            if (crudOperationData.ContainsAttribute("underwaycall", true) || !MaximoRestUtils.IsRestSetup()) {
                //avoid infinite loop
                return;
            }

            //entry was created locally, now we need to create it on ISM, using a rest call
            var result = maximoTemplateData.ResultObject;

            var ticketOriginalData = GetTicketOriginalData(crudOperationData, maximoTemplateData.ResultObject);

            crudOperationData.SetAttribute("underwaycall", true);
            crudOperationData.SetAttribute("ticketid", null);
            crudOperationData.SetAttribute("ticketuid", null);


            crudOperationData.Id = null;
            crudOperationData.UserId = null;

            TargetResult restResult = null;

            try {
                //updating ISM instance using REST --> no ticketid, ticketuid, they would be generated out there
                //also required for all the rest operations
                crudOperationData.SetAttribute("pluspcustomer", "CPS-00");
                var operationWrapper = new OperationWrapper(crudOperationData,
                    OperationConstants.CRUD_CREATE) {
                    Wsprovider = WsProvider.REST
                };
                restResult = ConnectorEngine.Execute(operationWrapper);
            } catch (Exception e) {

                var schemaId = maximoTemplateData.ApplicationMetadata.Name == "servicerequest"
                    ? "editdetail"
                    : "quickeditdetail";

                ProblemManager.RegisterOrUpdateProblem(
                    // ReSharper disable once PossibleInvalidOperationException
                    SecurityFacade.CurrentUser().UserId.Value,
                    Problem.BaseProblem(maximoTemplateData.ApplicationMetadata.Name, schemaId, ticketOriginalData.TicketUId, ticketOriginalData.TicketId, e.StackTrace, e.Message, "ismrestsync"),
                    () => Problem.ByEntryAndType.Fmt(ticketOriginalData.TicketUId, maximoTemplateData.ApplicationMetadata.Name, "ismrestsync")
                    );
                maximoTemplateData.ResultObject.WarningDto = new ErrorDto(e) {
                    WarnMessage = "Failed to Sync ticket to ISM. An Error has been created"
                };
                return;
            }

            ProblemManager.DeleteProblems("servicerequest", ticketOriginalData.TicketUId, "ismrestsync");


            //updating local entry, reset the ticketids

            crudOperationData.SetAttribute("ticketid", ticketOriginalData.TicketId);
            crudOperationData.SetAttribute("ticketuid", ticketOriginalData.TicketUId);
            crudOperationData.SetAttribute("status", ticketOriginalData.Status);

            Log.DebugFormat("updating ism ticketids for ticket {0}. id: {1}, uid:{2} ", ticketOriginalData.TicketId, restResult.UserId, restResult.Id);

            crudOperationData.Id = ticketOriginalData.TicketUId;
            crudOperationData.UserId = ticketOriginalData.TicketId;


            crudOperationData.SetAttribute(ISMTicketUid, restResult.Id);
            crudOperationData.SetAttribute(ISMTicketId, restResult.UserId);

            crudOperationData.ClearRelationShips("ld_");

            var mifOperationWrapper = new OperationWrapper(crudOperationData,
                OperationConstants.CRUD_UPDATE) {
            };

            ConnectorEngine.Execute(mifOperationWrapper);

        }

        /// <summary>
        /// Returns ticketuid, ticketid, Status tuple
        /// </summary>
        /// <param name="crudOperationData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private TicketOriginalData GetTicketOriginalData(CrudOperationData crudOperationData, TargetResult result) {
            var originalTicketid = crudOperationData.GetAttribute("ticketid") as string;
            var originalTicketuid = crudOperationData.GetAttribute("ticketuid") as string;
            var status = crudOperationData.GetAttribute("status") as string;
            if (originalTicketid != null && originalTicketuid != null) {
                return new TicketOriginalData(originalTicketuid, originalTicketid, status);
            }

            var originalSiteid = crudOperationData.GetAttribute("siteid");
            var ticketUid = result.Id;
            var ticketId = result.UserId;
            if (ticketUid != null && ticketId != null) {
                return new TicketOriginalData(ticketUid, ticketId, status);
            }


            //mif returns only the ticketid. ticketuid needs to be picked from database
            var obj = MaximoDAO.FindSingleByNativeQuery<object>("select ticketuid,status from sr where ticketid =? and siteid =? ", ticketId, originalSiteid) as dynamic;

            ticketUid = obj[0].ToString();
            result.Id = ticketUid;

            return new TicketOriginalData(ticketUid, ticketId, obj[1].ToString());

        }

        internal class TicketOriginalData {
            internal string TicketUId {
                get; set;
            }
            internal string TicketId {
                get; set;
            }
            internal string Status {
                get; set;
            }

            public TicketOriginalData(string ticketUId, string ticketId, string status) {
                TicketUId = ticketUId;
                TicketId = ticketId;
                Status = status;
            }
        }

    }
}
