using System;
using cts.commons.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sw4.problem.classes;
using softwrench.sw4.problem.classes.api;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Persistence.Engine.Exception;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Util.DeployValidation;
using softWrench.sw4.problem;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Data.Persistence.Engine {
    public class MaximoConnectorEngine : AConnectorEngine {

        //        private readonly SyncItemHandler _syncHandler;


        private static IProblemManager _problemManager;
        private IContextLookuper _lookuper;


        public MaximoConnectorEngine(EntityRepository entityRepository, IProblemManager problemManager, IContextLookuper lookuper)
            : base(entityRepository) {
            _problemManager = problemManager;
            _lookuper = lookuper;
            //            _syncHandler = syncHandler;
        }

        public override TargetResult Execute(OperationWrapper operationWrapper) {
            var entityMetadata = operationWrapper.EntityMetadata;
            var connector = GenericConnectorFactory.GetConnector(entityMetadata, operationWrapper.OperationName, operationWrapper.Wsprovider);
            var operationName = operationWrapper.OperationName;

            var result = DoExecuteCrud(operationWrapper, connector);
            if (result != null) {
                return result;
            }

            //lets search for a custom operation with same name of the connector
            var mi = ReflectionUtil.GetMethodNamed(connector, operationName);
            if (mi == null) {
                //fallback to crud methods
                var isCreate = operationWrapper.Id == null;
                operationWrapper.OperationName = isCreate
                    ? OperationConstants.CRUD_CREATE
                    : OperationConstants.CRUD_UPDATE;
                return DoExecuteCrud(operationWrapper, connector);
            }

            var customOperatorEngine = new MaximoCustomOperatorEngine(connector);

            try {
                return customOperatorEngine.InvokeCustomOperation(operationWrapper);
            } catch (System.Exception e) {
                LoggingUtil.DefaultLog.Error(e);
                var crudConnector = connector as IMaximoCrudConnector;
                string operationXml = null;
                if (crudConnector != null) {
                    //custom operations do now allow us to get the XML
                    operationXml = GetOperationXml(operationWrapper, crudConnector, entityMetadata);
                }

                if (operationWrapper.OperationData().ProblemData != null) {
                    var operationData = operationWrapper.OperationData();
                    return HandleDefaultProblem(operationData, e, xmlCurrentData: operationXml, jsonOriginalData: operationWrapper.JSON);
                }
            }

            return customOperatorEngine.InvokeCustomOperation(operationWrapper);
        }

        private static string GetOperationXml(OperationWrapper operationWrapper, IMaximoCrudConnector crudConnector,
            EntityMetadata entityMetadata)
        {
            var proxy = crudConnector.CreateProxy(entityMetadata);
            var crudOperationData = (CrudOperationData) operationWrapper.OperationData(null);
            var maximoTemplateData = crudConnector.CreateExecutionContext(proxy, crudOperationData);
            var xml = crudConnector.GenerateXml(maximoTemplateData);
            return xml;
        }


        private TargetResult DoExecuteCrud(OperationWrapper operationWrapper, IMaximoConnector connector) {
            var operationName = operationWrapper.OperationName;
            operationName = operationName.ToLower();
            if (!OperationConstants.IsCrud(operationName)) {
                //custom operation
                return null;
            }

            var crudConnector = new MaximoCrudConnectorEngine((IMaximoCrudConnector)connector, this);
            var crudOperationData = (CrudOperationData)operationWrapper.OperationData(null);
            operationWrapper.UserId = crudOperationData.UserId;
            switch (operationName) {
                case OperationConstants.CRUD_CREATE:
                    return crudConnector.Create(crudOperationData, operationWrapper.JSON);
                case OperationConstants.CRUD_UPDATE:
                    return crudConnector.Update(crudOperationData, operationWrapper.JSON);
                case OperationConstants.CRUD_DELETE:
                    return crudConnector.Delete(crudOperationData);
                case OperationConstants.CRUD_FIND_BY_ID:
                    return crudConnector.FindById(crudOperationData);
            }

            return null;
        }

        private TargetResult HandleDefaultProblem(IOperationData operationData,
            System.Exception e, string xmlCurrentData, JObject jsonOriginalData) {
            var context = _lookuper.LookupContext();
            var problemData = operationData.ProblemData;
            //default problem handling
            var problem = Problem.BaseProblem(operationData.ApplicationMetadata.Name,
                operationData.ApplicationMetadata.Schema.SchemaId, operationData.Id, operationData.UserId, e.StackTrace,
                e.Message, problemData.ProblemKey, xmlCurrentData, new JSonXmlProblemData(xmlCurrentData, jsonOriginalData));
            problem.ClientPlatform = context.OfflineMode ? ClientPlatform.Mobile : ClientPlatform.Web;
            problem.ProblemHandler = problemData.ProblemHandler;


            problem = _problemManager.RegisterOrUpdateProblem(SecurityFacade.CurrentUser().UserId.Value, problem, null);
            if (operationData.ProblemData.PropagateException) {
                throw new ProblemExceptionWrapper(e, problem);
            }
            return null;
        }

        public object Create(CrudOperationData crudOperationData) {
            return Execute(new OperationWrapper(crudOperationData, OperationConstants.CRUD_CREATE));
        }

        public object Update(CrudOperationData crudOperationData) {
            return Execute(new OperationWrapper(crudOperationData, OperationConstants.CRUD_UPDATE));
        }

        public object Delete(CrudOperationData crudOperationData) {
            return Execute(new OperationWrapper(crudOperationData, OperationConstants.CRUD_DELETE));
        }

        //        public override SynchronizationApplicationData Sync(ApplicationMetadata appMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData, SyncItemHandler.SyncedItemHandlerDelegate syncItemHandlerDelegate = null) {
        //            return _syncHandler.Sync(appMetadata, applicationSyncData, syncItemHandlerDelegate);
        //        }

        sealed class MaximoCrudConnectorEngine {

            private readonly IMaximoCrudConnector _crudConnector;
            private MaximoConnectorEngine _outer;

            public MaximoCrudConnectorEngine(IMaximoCrudConnector crudConnector, MaximoConnectorEngine outer) {
                _crudConnector = crudConnector;
                _outer = outer;
            }



            public TargetResult Update(CrudOperationData operationData, JObject originalJSON) {
                operationData.OperationType = OperationType.AddChange;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                try {
                    _crudConnector.BeforeUpdate(maximoTemplateData);
                    _crudConnector.DoUpdate(maximoTemplateData);
                } catch (System.Exception e) {
                    if (maximoTemplateData.OperationData.ProblemData != null) {
                        var xml = _crudConnector.GenerateXml(maximoTemplateData);
                        return _outer.HandleDefaultProblem(operationData, e, xml, originalJSON);
                    }
                    throw;
                }

                //ToDo: Improve this - Remove this if condition and replace with some advanced Mocking
                if (!DeployValidationService.MockProxyInvocation()) {
                    _crudConnector.AfterUpdate(maximoTemplateData);
                }

                return maximoTemplateData.ResultObject;
            }



            public TargetResult Create(CrudOperationData operationData, JObject originalJSON) {
                operationData.OperationType = OperationType.Add;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                try {
                    _crudConnector.BeforeCreation(maximoTemplateData);
                    _crudConnector.DoCreate(maximoTemplateData);
                } catch (System.Exception e) {
                    if (maximoTemplateData.OperationData.ProblemData != null) {
                        var xml = _crudConnector.GenerateXml(maximoTemplateData);
                        return _outer.HandleDefaultProblem(operationData, e, xml, originalJSON);
                    }
                    throw;
                }

                //ToDo: Improve this - Remove this if condition and replace with some advanced Mocking
                if (!DeployValidationService.MockProxyInvocation()) {
                    try {
                        _crudConnector.AfterCreation(maximoTemplateData);
                    } catch (System.Exception e) {
                        throw new AfterCreationException(maximoTemplateData.ResultObject, e);
                    }
                }

                return maximoTemplateData.ResultObject;
            }

            public TargetResult Delete(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Delete;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeDeletion(maximoTemplateData);
                _crudConnector.DoDelete(maximoTemplateData);

                //ToDo: Improve this - Remove this if condition and replace with some advanced Mocking
                if (!DeployValidationService.MockProxyInvocation()) {
                    _crudConnector.AfterDeletion(maximoTemplateData);
                }

                return maximoTemplateData.ResultObject;
            }

            public TargetResult FindById(CrudOperationData operationData) {
                operationData.OperationType = OperationType.Item;
                var proxy = _crudConnector.CreateProxy(operationData.EntityMetadata);
                var maximoTemplateData = _crudConnector.CreateExecutionContext(proxy, operationData);
                _crudConnector.PopulateIntegrationObject(maximoTemplateData);
                _crudConnector.BeforeFindById(maximoTemplateData);
                _crudConnector.DoFindById(maximoTemplateData);

                //ToDo: Improve this - Remove this if condition and replace with some advanced Mocking
                if (!DeployValidationService.MockProxyInvocation()) {
                    _crudConnector.AfterFindById(maximoTemplateData);
                }

                return maximoTemplateData.ResultObject;
            }
        }
    }
}
