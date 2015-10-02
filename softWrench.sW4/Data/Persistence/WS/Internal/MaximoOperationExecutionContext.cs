using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using log4net;
using softWrench.sW4.Data.Persistence.WS.API;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Mea;
using softWrench.sW4.Data.Persistence.WS.Mif;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    /// <summary>
    /// This class is a Holder that contains all the relevant data for a single maximo operation execution. It should not be reused between multiple invocations
    /// </summary>
    public abstract class MaximoOperationExecutionContext {

        protected const string WsInputLog = "WS_CALL_LOGS";

        protected static readonly ILog Log = LogManager.GetLogger(WsInputLog);

        protected MaximoOperationExecutionContext(IOperationData operationData) {
            _operationData = operationData;
            _applicationMetadata = operationData.ApplicationMetadata;
            _metadata = operationData.EntityMetadata;
        }

        private readonly IOperationData _operationData;
        private readonly ApplicationMetadata _applicationMetadata;
        private readonly EntityMetadata _metadata;

        public IOperationData OperationData { get { return _operationData; } }
        public ApplicationMetadata ApplicationMetadata { get { return _applicationMetadata; } }
        public EntityMetadata Metadata { get { return _metadata; } }

        public object IntegrationObject { get; set; }

        public TargetResult ResultObject { get; set; }

        public object RootInterfaceObject { get; set; }


        public DynamicObject Proxy { get; set; }
        private IDictionary<String, object> _properties = new Dictionary<string, object>();


        public IDictionary<string, object> Properties {
            get { return _properties; }
            set { _properties = value; }
        }

        public virtual object InvokeProxy() {
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

        internal abstract object FindById(Object id);

        /// <summary>
        /// Executes the invocation on the Proxy to MAXIMO's Web Service
        /// </summary>
        /// <returns></returns>
        protected virtual object DoProxyInvocation() {
            if (Log.IsDebugEnabled) {
                Log.Debug(SerializeIntegrationObject());
            } else if (ApplicationConfiguration.IsLocal() && Log.IsInfoEnabled) {
                Log.Info(SerializeIntegrationObject());
            }
            var result = Proxy.CallMethod(MethodName(),
                                      new[] { RootInterfaceObject.GetType() },
                                      new[] { RootInterfaceObject });
            return result;
        }

        /// <summary>
        /// Treats any exception thrown by executing <see cref="DoProxyInvocation"/>.
        /// Default: determines the root exception of e and returns a <see cref="MaximoException"/> 
        /// with e as its ImmediateCause and root as it's RootCause.
        /// Override for custom behavior.
        /// </summary>
        /// <param name="e">exception thrown</param>
        /// <returns></returns>
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
        protected virtual void OnProxyInvocationComplete(Stopwatch beforeInvocation) {}

    }
}
