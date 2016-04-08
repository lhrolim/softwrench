﻿using cts.commons.portable.Util;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Util;
using System;
using cts.commons.Util;
using softwrench.sw4.Shared2.Metadata.Entity;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntityAttribute : IQueryHolder {
        public const string AttributeQualifierSeparator = ".";

        private readonly string _name;
        private readonly string _type;
        private readonly bool _requiredExpression;
        private readonly ConnectorParameters _connectorParameters;
        private readonly bool _isAutoGenerated;

        public EntityAttribute([NotNull] string name, [NotNull] string type, bool requiredExpression, bool isAutoGenerated, [NotNull] ConnectorParameters connectorParameters, string query) {
            Validate.NotNull(name, "name");
            Validate.NotNull(type, "type");
            Validate.NotNull(connectorParameters, "connectorParameters");

            _name = name;
            _type = type;
            Query = query;
            _requiredExpression = requiredExpression;
            _connectorParameters = connectorParameters;
            _isAutoGenerated = isAutoGenerated;
        }

        [NotNull]
        public string Name {
            get {
                return _name;
            }
        }

        [NotNull]
        public string Type {
            get {
                return _type;
            }
        }


        public bool RequiredExpression {
            get {
                return _requiredExpression;
            }
        }

        public string Query {
            get; set;
        }

        [NotNull]
        public ConnectorParameters ConnectorParameters {
            get {
                return _connectorParameters;
            }
        }

        public bool IsAutoGenerated {
            get {
                return _isAutoGenerated;
            }
        }

        public bool IsAssociated {
            get {
                return _name.Contains(AttributeQualifierSeparator);
            }
        }

        public bool IsDate {
            get {
                return Type == "timestamp" || Type == "datetime";
            }
        }

        public bool IsNumber {
            get {
                return Type.EqualsAny("int", "bigint", "float", "integer", "decimal", "double");
            }
        }

        public override string ToString() {
            return string.Format("Name: {0}, Type: {1}", _name, _type);
        }

        protected bool Equals(EntityAttribute other) {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityAttribute)obj);
        }

        public override int GetHashCode() {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public virtual string GetQueryReplacingMarkers(String entityName) {
            if (Query.StartsWith("ref:")) {
                if (entityName.StartsWith("#")) {
                    Query = MetadataProvider.SwdbEntityQuery(Query);
                } else {
                    Query = MetadataProvider.EntityQuery(Query);
                }
            }
            return Query.Replace("!@", entityName + ".");
        }

        public EntityAttribute ClonePrependingContext(string context) {
            if (context == null) {
                return this;
            }

            return new ContextualEntityAttribute(context + "." + Name, Type, RequiredExpression,
                IsAutoGenerated, ConnectorParameters, Query, context);
        }
    }
}