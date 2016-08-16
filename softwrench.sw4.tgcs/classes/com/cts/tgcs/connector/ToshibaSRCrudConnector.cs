﻿using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.connector {

    public class ToshibaSRCrudConnector : BaseServiceRequestCrudConnector {

        private const string ISMTicketId = "ismticketid";
        private const string ISMTicketUid = "ismticketuid";

        private IProblemManager _problemManager;
        private IMaximoHibernateDAO _maximoHibernateDAO;

        private IProblemManager ProblemManager {
            get {
                if (_problemManager != null) {
                    return _problemManager;
                }
                _problemManager = SimpleInjectorGenericFactory.Instance.GetObject<IProblemManager>(typeof(IProblemManager));
                return _problemManager;
            }


        }

        private IMaximoHibernateDAO MaximoDAO {
            get {
                if (_maximoHibernateDAO != null) {
                    return _maximoHibernateDAO;
                }
                _maximoHibernateDAO =
                    SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>(typeof(IMaximoHibernateDAO));
                return _maximoHibernateDAO;
            }
        }


        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;

            var statusValue = w.GetRealValue(sr, "STATUS");
            if (statusValue == null) {
                base.BeforeUpdate(maximoTemplateData);
                return;
            }


            if (statusValue.Equals("INPROG")) {
                w.SetValueIfNull(sr, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            } else if (statusValue.Equals("RESOLVED")) {
                w.SetValue(sr, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            } else if (statusValue.Equals("CLOSED")) {
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
                //updating ISM Entry which already exists
                var ismTicketUid = crudOperationData.GetAttribute(ISMTicketUid);
                crudOperationData.SetAttribute("ticketuid", ismTicketUid);
                crudOperationData.SetAttribute("ticketid", crudOperationData.GetAttribute(ISMTicketId));

                crudOperationData.SetAttribute("underwaycall", true);

                crudOperationData.Id = ismTicketUid as string;

                var mifOperationWrapper = new OperationWrapper(crudOperationData,
                    OperationConstants.CRUD_UPDATE) {
                    Wsprovider = WsProvider.REST
                };

                ConnectorEngine.Execute(mifOperationWrapper);
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

            var ticketOriginalData = GetTicketOriginalData(crudOperationData, maximoTemplateData.ResultObject);

            crudOperationData.SetAttribute("underwaycall", true);
            crudOperationData.SetAttribute("ticketid", null);
            crudOperationData.SetAttribute("ticketuid", null);


            crudOperationData.Id = null;
            crudOperationData.UserId = null;

            //updating ISM instance using REST --> no ticketid, ticketuid, they would be generated out there
            var operationWrapper = new OperationWrapper(crudOperationData,
                OperationConstants.CRUD_CREATE) {
                Wsprovider = WsProvider.REST
            };

            var restResult = ConnectorEngine.Execute(operationWrapper);

            //updating local entry, reset the ticketids

            crudOperationData.SetAttribute("ticketid", ticketOriginalData.Item1);
            crudOperationData.SetAttribute("ticketuid", ticketOriginalData.Item2);
            crudOperationData.SetAttribute("status", ticketOriginalData.Item3);


            
            crudOperationData.Id = ticketOriginalData.Item1;
            crudOperationData.UserId = ticketOriginalData.Item2;


            crudOperationData.SetAttribute(ISMTicketUid, restResult.Id);
            crudOperationData.SetAttribute(ISMTicketId, restResult.UserId);

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
        private Tuple<string, string, string> GetTicketOriginalData(CrudOperationData crudOperationData, TargetResult result) {
            var originalTicketid = crudOperationData.GetAttribute("ticketid") as string;
            var originalTicketuid = crudOperationData.GetAttribute("ticketuid") as string;
            var status = crudOperationData.GetAttribute("status") as string;
            if (originalTicketid != null && originalTicketuid != null) {
                return new Tuple<string, string, string>(originalTicketuid, originalTicketid, status);
            }

            var originalSiteid = crudOperationData.GetAttribute("siteid");
            var ticketUid = result.Id;
            var ticketId = result.UserId;
            if (ticketUid != null && ticketId != null) {
                return new Tuple<string, string, string>(ticketUid, ticketId, status);
            }


            //mif returns only the ticketid. ticketuid needs to be picked from database
            var obj = MaximoDAO.FindSingleByNativeQuery<object>("select ticketuid,status from sr where ticketid =? and siteid =? ", ticketId, originalSiteid) as dynamic;

            ticketUid = obj[0].ToString();
            result.Id = ticketUid;

            return new Tuple<string, string, string>(ticketUid, ticketId, obj[1].ToString());


        }

    }
}