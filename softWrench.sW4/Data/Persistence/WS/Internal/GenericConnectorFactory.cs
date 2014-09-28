using System;
using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Data.Persistence.WS.Ism;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Mea;
using softWrench.sW4.Data.Persistence.WS.Mif;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    internal class GenericConnectorFactory {
        private const string Customconnector = "_customconnector";
        private const string WrongCrudConnectorType = "Custom crud connector {0} should be of type {1}";
        private const string WrongConnectorType = "Custom connector {0} should be of type {1}";

        public static IMaximoConnector GetConnector(EntityMetadata metadata, String operation) {
            BaseMaximoCrudConnector baseCrudConnector = GetBaseConnector();
            IMaximoConnector decoratedCrudConnector = LookupCustomConnector(metadata, operation);
            if (decoratedCrudConnector == null) {
                return baseCrudConnector;
            }
            var decorator = decoratedCrudConnector as CrudConnectorDecorator;
            if (decorator != null) {
                decorator.RealCrudConnector = baseCrudConnector;
            }
            return decoratedCrudConnector;
        }

        private static IMaximoConnector LookupCustomConnector(EntityMetadata metadata, String operation) {
            var provider = WsUtil.WsProvider();
            var prefix = provider.ToString().ToLower();
            var connectorParams = metadata.ConnectorParameters.Parameters;
            String customConnectorTypeName = null;
            connectorParams.TryGetValue(operation.ToLower() + "_" + prefix + Customconnector, out customConnectorTypeName);
            if (customConnectorTypeName == null) {
                connectorParams.TryGetValue(operation.ToLower() + Customconnector, out customConnectorTypeName);
            }
            if (customConnectorTypeName == null) {
                connectorParams.TryGetValue(prefix + Customconnector, out customConnectorTypeName);
            }
            if (customConnectorTypeName == null) {
                connectorParams.TryGetValue("customconnector", out customConnectorTypeName);
            }
            return DoGetConnector(operation, customConnectorTypeName);
        }

        private static IMaximoConnector DoGetConnector(string operation, string customConnectorTypeName) {
            if (customConnectorTypeName == null) {
                return null;
            }
            //TODO: use spring
            try {
                var connector = (IMaximoConnector)ReflectionUtil.InstanceFromName(customConnectorTypeName);
                if (OperationConstants.IsCrud(operation) && !(connector is IMaximoCrudConnector)) {
                    throw ExceptionUtil.InvalidOperation(WrongCrudConnectorType, customConnectorTypeName,
                        typeof(IMaximoCrudConnector).Name);
                }
                return connector;
            } catch (InvalidCastException) {
                throw ExceptionUtil.InvalidOperation(WrongConnectorType, customConnectorTypeName,
                    typeof(IMaximoConnector).Name);
            } catch (Exception) {
                throw ExceptionUtil.InvalidOperation("Custom connector {0} not found", customConnectorTypeName);
            }
        }

        public static BaseMaximoCrudConnector GetBaseConnector() {
            var provider = WsUtil.WsProvider();
            if (WsProvider.MEA.Equals(provider)) {
                return new MeaCrudConnector();
            }
            if (WsProvider.MIF.Equals(provider)) {
                return new MifCrudConnector();
            }
            if (WsProvider.ISM.Equals(provider)) {
                return new IsmCrudConnector();
            }
            throw new InvalidOperationException("Please, configure WsProvider key to either mea or mif");
        }
    }
}
