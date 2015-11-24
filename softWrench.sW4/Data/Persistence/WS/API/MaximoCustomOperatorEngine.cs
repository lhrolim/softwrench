using System;
using System.Linq;
using System.Reflection;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.API {
    class MaximoCustomOperatorEngine {
        private const string ErrorParameter = "Custom operation{0} of entity {1} should have exactly 1 parameters: " +
                                              "and it should Extend softWrench.sW4.Data.Persistence.Operation.OperationData ";

        private readonly IMaximoConnector _connectorTemplate;

        public MaximoCustomOperatorEngine(IMaximoConnector connectorTemplate) {
            _connectorTemplate = connectorTemplate;
        }

        public TargetResult InvokeCustomOperation(OperationWrapper operationWrapper) {
            var operationName = operationWrapper.OperationName;
            var entityMetadata = operationWrapper.EntityMetadata;
            try {
                var mi = ReflectionUtil.GetMethodNamed(_connectorTemplate, operationName);
                if (mi == null) {
                    throw new InvalidOperationException(String.Format("operation {0} not found on entity {1}",
                                                                      operationName,
                                                                      entityMetadata.Name));
                }
                if (mi.GetParameters().Count() != 1) {
                    throw new InvalidOperationException(
                        String.Format(ErrorParameter, operationName, entityMetadata.Name));
                }
                var fp = mi.GetParameters().First();
                if (!typeof(OperationData).IsAssignableFrom(fp.ParameterType)) {
                    throw new InvalidOperationException(
                        String.Format(ErrorParameter, operationName, entityMetadata.Name));
                }
                var param = operationWrapper.OperationData(fp.ParameterType);
                operationWrapper.UserId = param.UserId;
                if (mi.ReturnType == typeof(void)) {
                    mi.Invoke(_connectorTemplate, new object[] { param });
                    return null;
                }
                var ob = mi.Invoke(_connectorTemplate, new object[] { param });
                if (ob is TargetResult) {
                    return (TargetResult)ob;
                }
                return new TargetResult(param.Id, param.UserId, ob);
            } catch (AmbiguousMatchException) {
                throw new InvalidOperationException(
                    String.Format("multiples methods found for operation {0} on entity {1}. Unable to decide", operationName,
                                  entityMetadata.Name));
            }


        }

    }
}
