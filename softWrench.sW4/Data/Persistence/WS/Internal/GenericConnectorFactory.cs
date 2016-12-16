using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Data.Persistence.WS.Mea;
using softWrench.sW4.Data.Persistence.WS.Mif;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    internal class GenericConnectorFactory {
        private const string Customconnector = "_customconnector";
        private const string WrongCrudConnectorType = "Custom crud connector {0} should be of type {1}";
        private const string WrongConnectorType = "Custom connector {0} should be of type {1}";

        public static IMaximoConnector GetConnector(EntityMetadata metadata, string operation, WsProvider? provider = null) {
            var baseCrudConnector = GetBaseConnector(provider);
            var decoratedCrudConnector = LookupCustomConnector(metadata, operation, provider);
            if (decoratedCrudConnector == null) {
                return baseCrudConnector;
            }
            var decorator = decoratedCrudConnector as CrudConnectorDecorator;
            if (decorator != null) {
                decorator.RealCrudConnector = baseCrudConnector;
            }
            return decoratedCrudConnector;
        }

        private static IMaximoConnector LookupCustomConnector(EntityMetadata metadata, string operation, WsProvider? provider = null) {
            if (provider == null) {
                provider = WsUtil.WsProvider();
            }
            var prefix = provider.ToString().ToLower();
            var connectorParams = metadata.ConnectorParameters.Parameters;
            string customConnectorTypeName;
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
            var entityName = metadata.Name;
            if (entityName.Equals("sr")) {
                entityName = "servicerequest";
            }

            var decoratorProvider = ConnectorDecoratorProvider.GetInstance();

            var customConnector = decoratorProvider.LookupItem(entityName, operation, ApplicationConfiguration.ClientName);
            return customConnector;
        }

        //        private static IMaximoConnector DoGetConnector(string entityName, string operation, string customConnectorTypeName) {
        //          try {
        //                
        //                if (OperationConstants.IsCrud(operation) && !(connector is IMaximoCrudConnector)) {
        //                    throw ExceptionUtil.InvalidOperation(WrongCrudConnectorType, customConnectorTypeName,
        //                        typeof(IMaximoCrudConnector).Name);
        //                }
        //                return connector;
        //            } catch (InvalidCastException) {
        //                throw ExceptionUtil.InvalidOperation(WrongConnectorType, customConnectorTypeName,
        //                    typeof(IMaximoConnector).Name);
        //            } catch (Exception) {
        //                throw ExceptionUtil.InvalidOperation("Custom connector not found", customConnectorTypeName);
        //            }
        //        }

        public static BaseMaximoCrudConnector GetBaseConnector(WsProvider? provider = null) {
            if (provider == null) {
                provider = WsUtil.WsProvider();
            }

            if (WsProvider.MEA.Equals(provider)) {
                return new MeaCrudConnector();
            }
            if (WsProvider.MIF.Equals(provider)) {
                return new MifCrudConnector();
            }
            if (WsProvider.ISM.Equals(provider)) {
                //                return new IsmCrudConnector();
            }
            if (WsProvider.REST.Equals(provider)) {
                return new RestCrudConnector();
            }
            throw new InvalidOperationException("Please, configure WsProvider key to either mea or mif");
        }
    }
}
