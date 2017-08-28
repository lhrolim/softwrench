using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.WS.API;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Util.DeployValidation;
using r = softWrench.sW4.Util.ReflectionUtil;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Mif {
    class MifExecutionContext : MaximoOperationExecutionContext {
        private readonly string _methodName;

        public DynamicProxyUtil ProxyUtil => SimpleInjectorGenericFactory.Instance.GetObject<DynamicProxyUtil>();
        public IConfigurationFacade ConfigFacade => SimpleInjectorGenericFactory.Instance.GetObject<IConfigurationFacade>();

        public MifExecutionContext(IOperationData operationData, DynamicObject proxy = null)
            : base(operationData) {
            Proxy = proxy ?? ProxyUtil.LookupProxy(operationData.EntityMetadata);
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
            r.InstantiateProperty(integrationObject, "CHANGEDATE", new {
                Value = DateTime.Now.FromServerToRightKind()
            });
            //TODO: get current user, in the mobile case below code may be wrong
            r.InstantiateProperty(integrationObject, "CHANGEBY", new {
                Value = curUser.Login
            });
            //TODO: get from user
            r.InstantiateProperty(integrationObject, "ORGID", new {
                Value = curUser.OrgId
            });
            r.InstantiateProperty(integrationObject, "SITEID", new {
                Value = curUser.SiteId
            });
            r.SetProperty(integrationObject, "action", operationType.ToString());

            IntegrationObject = integrationObject;
            RootInterfaceObject = notifyInterface;
        }

        protected override string MethodName() {
            return _methodName;
        }

        public override object FindById(object id) {
            var type = Proxy.ProxyType;
            var mi = type.GetMethod(MethodName());
            var pi = mi.GetParameters().First();
            var parameterType = pi.ParameterType;
            var qType = r.InstanceFromType(parameterType);
            w.SetValue(qType, "WHERE", $"{Metadata.Schema.IdAttribute.Name}='{id}'");
            var parameterList = MifUtils.GetParameterListForQuery(qType);
            var typesFromParameters = DynamicProxyUtil.TypesFromParameters(parameterList);
            var result = Proxy.CallMethod(MethodName(), typesFromParameters, parameterList.ToArray());
            var enumerable = result as IEnumerable;
            if (enumerable == null) {
                return null;
            }

            var enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }
            return null;
        }

        protected override TargetResult DoProxyInvocation() {
            //            if (ApplicationConfiguration.IgnoreWsCertErrors) {
            //                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            //                ServicePointManager.Expect100Continue = true;
            //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            //                    | SecurityProtocolType.Tls11
            //                    | SecurityProtocolType.Tls12
            //                    | SecurityProtocolType.Ssl3;
            //            }

            string xml = null;

            if (Log.IsDebugEnabled) {
                xml = SerializeIntegrationObject();
                Log.Debug("sending content to mif :\n " + xml);
            } else if (ApplicationConfiguration.IsLocal() || Log.IsInfoEnabled) {
                Log.Info("sending content to mif :\n " + xml);
            }



            var parameterList = MifUtils.GetParameterList(RootInterfaceObject);
            var types = new Type[parameterList.Length];
            for (var i = 0; i < parameterList.Length; i++) {
                types.SetValue(parameterList.GetValue(i).GetType(), i);

            }

            object result = null;
            if (!DeployValidationService.MockProxyInvocation()) {
                result = Proxy.CallMethod(MethodName(), types, parameterList);
            }
            var targetResult = CreateResultData(result);

            if (ShouldAudit()) {
                xml = SerializeIntegrationObject();
                AuditXml(xml, targetResult);

            }

            return targetResult;
        }

        internal void CheckCredentials(DynamicObject proxy) {


            var credentialsUser = ConfigFacade.Lookup<string>(ConfigurationConstants.Maximo.MifUser, "mifcredentials.user");
            var credentialsPassword = ConfigFacade.Lookup<string>(ConfigurationConstants.Maximo.MifPassword, "mifcredentials.password");
            if (string.IsNullOrEmpty(credentialsUser) || string.IsNullOrEmpty(credentialsPassword))
                return;
            var url = (string)ReflectionUtil.GetProperty(proxy.ObjectInstance, "Url");
            var credCache = new CredentialCache();
            var netCred = new NetworkCredential(credentialsUser, credentialsPassword);
            credCache.Add(new Uri(url), "Basic", netCred);
            ReflectionUtil.SetProperty(proxy.ObjectInstance, "Credentials", credCache);
        }
    }
}
