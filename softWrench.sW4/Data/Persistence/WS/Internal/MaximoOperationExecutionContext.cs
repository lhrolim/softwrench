using System;
using System.Collections.Generic;
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

        public MaximoResult ResultObject { get; set; }

        public object RootInterfaceObject { get; set; }


        public DynamicObject Proxy { get; set; }
        private IDictionary<String, object> _properties = new Dictionary<string, object>();


        public IDictionary<string, object> Properties {
            get { return _properties; }
            set { _properties = value; }
        }



        public virtual object InvokeProxy() {
            try {
                var result = Proxy.CallMethod(MethodName(),
                                          new[] { RootInterfaceObject.GetType() },
                                          new[] { RootInterfaceObject });
                return result;
            }
            catch (Exception e) {
                var rootException = ExceptionUtil.DigRootException(e);
                throw rootException;
            }
        }

        public static MaximoOperationExecutionContext GetInstance(IOperationData operationData, DynamicObject proxy = null) {
            return ApplicationConfiguration.IsMif() ? (MaximoOperationExecutionContext)new MifExecutionContext(operationData, proxy) :
                new MeaExecutionContext(operationData, proxy);
        }

        protected abstract string MethodName();

        internal abstract object FindById(Object id);
    }
}
