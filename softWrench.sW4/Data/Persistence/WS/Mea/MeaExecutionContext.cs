using System;
using System.Linq;
using System.Reflection;
using cts.commons.simpleinjector;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using r = softWrench.sW4.Util.ReflectionUtil;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Mea {
    class MeaExecutionContext : MaximoOperationExecutionContext {

        public const string _MethodName = "processDocument";

        private DynamicObject _queryProxy = null;

        public DynamicProxyUtil ProxyUtil => SimpleInjectorGenericFactory.Instance.GetObject<DynamicProxyUtil>();

        public MeaExecutionContext(IOperationData operationData, DynamicObject proxy = null)
            : base(operationData) {
            if (proxy == null) {
                proxy = ProxyUtil.LookupProxy(operationData.EntityMetadata);
            }
            //            _queryProxy = DynamicProxyUtil.LookupProxy(operationData.EntityMetadata, true);
            var curUser = SecurityFacade.CurrentUser();
            BuildNotify(operationData, proxy, curUser);
        }

        private void BuildNotify(IOperationData operationData, DynamicObject proxy, InMemoryUser curUser) {
            Type type = proxy.ProxyType;
            MethodInfo mi = type.GetMethod(_MethodName);
            ParameterInfo pi = mi.GetParameters().First();
            object notifyInterface = r.InstanceFromType(pi.ParameterType);
            var header = r.InstantiateProperty(notifyInterface, "Header");
            r.InstantiateProperty(header, "SenderID", new {
                Value = SwConstants.ExternalSystemName
            });
            r.SetProperty(notifyInterface, "Header", header);
            var rootIntegrationObject = r.InstantiateArrayReturningSingleElement(notifyInterface, "Content");
            object integrationObject = r.InstantiateProperty(rootIntegrationObject, 0);
            r.SetProperty(integrationObject, "actionSpecified", true);
            r.InstantiateProperty(integrationObject, "CHANGEDATE", new {
                Value = DateTime.Now.FromServerToRightKind()
            });
            //TODO: get current user, in the mobile case below code may be wrong
            WsUtil.SetValue(integrationObject, "ORGID", curUser.OrgId);
            WsUtil.SetValue(integrationObject, "SITEID", curUser.SiteId);
            r.InstantiateProperty(integrationObject, "CHANGEBY", new {
                Value = curUser.Login
            });
            OperationType operationType = operationData.OperationType;
            EntityMetadata metadata = operationData.EntityMetadata;
            r.SetProperty(integrationObject, "action", operationType.ToString());
            Proxy = proxy;
            IntegrationObject = integrationObject;
            RootInterfaceObject = notifyInterface;
        }

        protected override string MethodName() {
            return _MethodName;
        }

        public override object FindById(Object id) {
            InMemoryUser curUser = SecurityFacade.CurrentUser();
            Type type = _queryProxy.ProxyType;
            MethodInfo mi = type.GetMethod(_MethodName);
            ParameterInfo pi = mi.GetParameters().First();
            object queryInterface = r.InstanceFromType(pi.ParameterType);
            var header = r.InstantiateProperty(queryInterface, "Header");
            w.SetValue(header, "operation", "Query");
            w.SetValue(header, "maxItems", "10");
            w.SetValue(header, "rsStart", "0");
            w.SetValue(header, "CreationDateTime", DateTime.Now.FromServerToRightKind());
            w.SetValue(header, "SenderID", SwConstants.ExternalSystemName);
            w.SetValue(header, "uniqueResult", true, true);
            r.SetProperty(queryInterface, "Header", header);
            var rootQueryObject = r.InstantiateArrayReturningSingleElement(queryInterface, "Content");
            object queryObject = r.InstantiateProperty(rootQueryObject, 0);
            w.SetQueryValue(queryObject, Metadata.Schema.IdAttribute.Name, id);
            try {
                var rawReponse = _queryProxy.CallMethod(MethodName(),
                                          new Type[] { queryInterface.GetType() },
                                          new object[] { queryInterface });
                var respArr = (Array)r.GetProperty(rawReponse, "Content");
                if (respArr.Length == 0) {
                    return null;
                }
                var rootResponse = respArr.GetValue(0);
                return r.GetProperty(rootResponse, 0);
            } catch (Exception e) {
                var rootException = ExceptionUtil.DigRootException(e);
                throw rootException;
            }
        }
    }
}
