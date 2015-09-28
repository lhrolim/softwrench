using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Reflection;
using WcfSamples.DynamicProxy;
using r = softWrench.sW4.Util.ReflectionUtil;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Mif {
    class MifExecutionContext : MaximoOperationExecutionContext {
        private readonly string _methodName;


        public MifExecutionContext(IOperationData operationData, DynamicObject proxy = null)
            : base(operationData) {
            Proxy = proxy ?? DynamicProxyUtil.LookupProxy(operationData.EntityMetadata);
            CheckCredentials(Proxy);

            var curUser = SecurityFacade.CurrentUser();
            var operationType = operationData.OperationType;
            var isCreation = OperationType.Add == operationType;
            _methodName = MifMethodNameUtils.GetMethodName(Metadata, operationType);
            var type = Proxy.ProxyType;

            var mi = type.GetMethod(_methodName);
            var pi = mi.GetParameters().First();
            // element array , like MXSW3_WO_TYPE[]
            var parameterType = pi.ParameterType;

            object notifyInterface;
            object integrationObject;
            if (parameterType.IsArray) {
                notifyInterface = ReflectionUtil.InstantiateArrayWithBlankElements(parameterType.GetElementType(), 1);
                integrationObject = ((Array)notifyInterface).GetValue(0);
            } else {
                notifyInterface = ReflectionUtil.InstanceFromType(parameterType);
                integrationObject = notifyInterface;
            }

            r.SetProperty(integrationObject, "actionSpecified", true);
            r.InstantiateProperty(integrationObject, "CHANGEDATE", new { Value = DateTime.Now.FromServerToRightKind() });
            //TODO: get current user, in the mobile case below code may be wrong
            r.InstantiateProperty(integrationObject, "CHANGEBY", new { Value = curUser.Login });
            //TODO: get from user
            r.InstantiateProperty(integrationObject, "ORGID", new { Value = curUser.OrgId });
            r.InstantiateProperty(integrationObject, "SITEID", new { Value = curUser.SiteId });
            r.SetProperty(integrationObject, "action", operationType.ToString());

            IntegrationObject = integrationObject;
            RootInterfaceObject = notifyInterface;
        }

        protected override string MethodName() {
            return _methodName;
        }

        internal override object FindById(object id) {
            Type type = Proxy.ProxyType;
            MethodInfo mi = type.GetMethod(MethodName());
            ParameterInfo pi = mi.GetParameters().First();
            Type parameterType = pi.ParameterType;
            object qType = r.InstanceFromType(parameterType);
            w.SetValue(qType, "WHERE", string.Format("{0}='{1}'", Metadata.Schema.IdAttribute.Name, id));
            var parameterList = MifUtils.GetParameterListForQuery(qType);
            Type[] typesFromParameters = DynamicProxyUtil.TypesFromParameters(parameterList);
            object result = Proxy.CallMethod(MethodName(), typesFromParameters, parameterList.ToArray());
            var enumerable = result as IEnumerable;
            if (enumerable == null) {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }
            return null;
        }

        protected override object DoProxyInvocation() {
            if (ApplicationConfiguration.IgnoreWsCertErrors) {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
            if (Log.IsDebugEnabled) {
                Log.Debug("sending content to mif :\n " + SerializeIntegrationObject());
            } else if (ApplicationConfiguration.IsLocal() || Log.IsInfoEnabled) {
                Log.Info("sending content to mif :\n " + SerializeIntegrationObject());
            }

            var parameterList = MifUtils.GetParameterList(RootInterfaceObject);
            var types = new Type[parameterList.Length];
            for (var i = 0; i < parameterList.Length; i++) {
                types.SetValue(parameterList.GetValue(i).GetType(), i);

            }
            var result = Proxy.CallMethod(MethodName(), types, parameterList);
            return result;
        }

        internal void CheckCredentials(DynamicObject proxy) {
            var credentialsUser = ApplicationConfiguration.MifCredentialsUser;
            var credentialsPassword = ApplicationConfiguration.MifCredentialsPassword;
            if (string.IsNullOrEmpty(credentialsUser) || string.IsNullOrEmpty(credentialsPassword)) return;
            var url = (string)ReflectionUtil.GetProperty(proxy.ObjectInstance, "Url");
            var credCache = new CredentialCache();
            var netCred = new NetworkCredential(credentialsUser, credentialsPassword);
            credCache.Add(new Uri(url), "Basic", netCred);
            ReflectionUtil.SetProperty(proxy.ObjectInstance, "Credentials", credCache);
        }
    }
}
