using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.WS.API;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Mea;
using softWrench.sW4.Data.Persistence.WS.Mif;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    /// <summary>
    /// This class is a Holder that contains all the relevant data for a single maximo operation execution. It should not be reused between multiple invocations
    /// </summary>
    public abstract class MaximoOperationExecutionContext {

        protected const string WsInputLog = "WS_CALL_LOGS";

        protected static readonly ILog Log = LogManager.GetLogger(WsInputLog);

        private readonly IAuditManager _auditManager = SimpleInjectorGenericFactory.Instance.GetObject<IAuditManager>();

        private readonly ConfigurationFacade _configurationFacade = SimpleInjectorGenericFactory.Instance.GetObject<ConfigurationFacade>();

        protected MaximoOperationExecutionContext(IOperationData operationData) {
            OperationData = operationData;
            ApplicationMetadata = operationData.ApplicationMetadata;
            Metadata = operationData.EntityMetadata;
        }

        public IOperationData OperationData { get; }

        public ApplicationMetadata ApplicationMetadata { get; }

        public EntityMetadata Metadata { get; }

        public object IntegrationObject {
            get; set;
        }

        public TargetResult ResultObject {
            get; set;
        }

        public object RootInterfaceObject {
            get; set;
        }


        public DynamicObject Proxy {
            get; set;
        }


        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public virtual TargetResult InvokeProxy() {
            var before = Stopwatch.StartNew();
            try {
                return DoProxyInvocation();
            } catch (Exception e) {
                throw HandleProxyInvocationError(e);
            } finally {
                OnProxyInvocationComplete(before);
            }
        }

        public static MaximoOperationExecutionContext GetInstance(IOperationData operationData, DynamicObject proxy = null) {
            return ApplicationConfiguration.IsMif() ? (MaximoOperationExecutionContext)new MifExecutionContext(operationData, proxy) :
                new MeaExecutionContext(operationData, proxy);
        }

        protected string SerializeIntegrationObject() {
            var rootElement = IntegrationObject;
            var serializer = new XmlSerializer(rootElement.GetType(), @"http://b2b.ibm.com/schema/B2B_CDM_Incident/R2_2");
            var sWriter = new StringWriter();
            serializer.Serialize(sWriter, rootElement);
            return sWriter.ToString().Substring(sWriter.ToString().IndexOf('\n') + 1, sWriter.ToString().Length - sWriter.ToString().IndexOf('\n') - 1);
        }

        protected abstract string MethodName();

        public abstract object FindById(Object id);

        /// <summary>
        /// Executes the invocation on the Proxy to MAXIMO's Web Service
        /// </summary>
        /// <returns></returns>
        protected virtual TargetResult DoProxyInvocation() {
            string xml = null;
            var result = InnerDoInvoke();

            var targetResult = CreateResultData(result);


            if (ShouldAudit()) {
                xml = SerializeIntegrationObject();
                AuditXml(xml, targetResult);

            }


            return targetResult;

        }

        public object InnerDoInvoke() {
            string xml;
            if (Log.IsDebugEnabled) {
                xml = SerializeIntegrationObject();
                Log.Debug(xml);
            } else if (ApplicationConfiguration.IsLocal() && Log.IsInfoEnabled) {
                xml = SerializeIntegrationObject();
                Log.Info(xml);
            }

            var result = Proxy.CallMethod(MethodName(),
                new[] { RootInterfaceObject.GetType() },
                new[] { RootInterfaceObject });
            return result;
        }

        protected void AuditXml(string xml, TargetResult targetResult) {
            _auditManager.AppendToCurrentTrail(OperationData.OperationType.ToString(), ApplicationMetadata.Name, targetResult.Id, targetResult.UserId, xml);
        }

        protected bool ShouldAudit() {
            return _configurationFacade.Lookup<bool>(AuditConstants.AuditEnabled);
        }

        /// <summary>
        /// Treats any exception thrown by executing <see cref="DoProxyInvocation"/>.
        /// Default: determines the root exception of e and returns a <see cref="MaximoException"/> 
        /// with e as its ImmediateCause and root as it's RootCause.
        /// Override for custom behavior.
        /// </summary>
        /// <param name="e">exception thrown</param>
        /// <returns></returns>
        /// 
        protected virtual Exception HandleProxyInvocationError(Exception e) {
            var rootException = ExceptionUtil.DigRootException(e);
            return new MaximoException(e, rootException);
        }

        /// <summary>
        /// Executed when a <see cref="DoProxyInvocation"/> completes, regardless of failure or success.
        /// Receives a stopwatch that is started before the remote invocation for tracking/logging purposes.
        /// Default: does nothing.
        /// Override for custom behavior.
        /// </summary>
        /// <param name="beforeInvocation">stopwatch started before the invocation</param>
        protected virtual void OnProxyInvocationComplete(Stopwatch beforeInvocation) {
        }


        protected TargetResult CreateResultData(object resultData) {
            if (OperationData.Id != null) {
                //update scenario
                return new TargetResult(OperationData.Id, OperationData.UserId, resultData);
            }

            var idProperty = Metadata.Schema.IdAttribute.Name;
            var siteIdAttribute = Metadata.Schema.SiteIdAttribute;
            var userIdProperty = Metadata.Schema.UserIdAttribute.Name;
            var resultOb = (Array)resultData;
            var firstOb = resultOb?.GetValue(0);
            var id = firstOb == null ? null : WsUtil.GetRealValue(firstOb, idProperty);
            var userId = firstOb == null ? null : WsUtil.GetRealValue(firstOb, userIdProperty);
            string siteId = null;
            if (siteIdAttribute != null && firstOb != null) {
                //not all entities will have a siteid...
                siteId = WsUtil.GetRealValue(firstOb, siteIdAttribute.Name) as string;
            }
            if (!idProperty.Equals(userIdProperty) && userId == null) {
                Log.WarnFormat("User Identifier {0} not received after creating object in Maximo.", idProperty);
                return new TargetResult(null, null, resultData, null, siteId);
            }
            if (id == null && userId == null) {
                Log.WarnFormat("Identifier {0} not received after creating object in Maximo.", idProperty);
                return new TargetResult(null, null, resultData, null, siteId);
            }
            if (id == null) {
                Log.WarnFormat("Identifier {0} not received after creating object in Maximo.", idProperty);
                return new TargetResult(null, userId.ToString(), resultData, null, siteId);
            }
            return new TargetResult(id.ToString(), userId.ToString(), resultData, null, siteId);
        }

    }
}
