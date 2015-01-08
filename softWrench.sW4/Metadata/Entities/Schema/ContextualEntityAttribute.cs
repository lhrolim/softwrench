﻿using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities.Connectors;
using System;

namespace softWrench.sW4.Metadata.Entities.Schema {
    /// <summary>
    /// This class is a entityAttribute whose context has been fixed upon construction. 
    /// 
    /// Needed because on SlicedEntityMetadata attribute iteration the attributes are slided to the top entity, 
    /// which could lead the query replacement wrong (imac.assetnum instead of asset_.assetnum for instance)
    /// </summary>
    public class ContextualEntityAttribute : EntityAttribute {

        public string Context { get; set; }

        public ContextualEntityAttribute([NotNull] string name, [NotNull] string type, bool requiredExpression, bool isAutoGenerated,
            [NotNull] ConnectorParameters connectorParameters, string query, string context)
            : base(name, type, requiredExpression, isAutoGenerated, connectorParameters, query) {
            Context = context;
        }

        public override string GetQueryReplacingMarkers(String entityName) {


            if (Query.StartsWith("ref:")) {
                if (entityName.StartsWith("#")) {
                    Query = MetadataProvider.SwdbEntityQuery(Query);
                } else {
                    Query = MetadataProvider.EntityQuery(Query);
                }
            }
            if (Context != null) {
                return Query.Replace("!@", Context + ".");
            }
            return base.GetQueryReplacingMarkers(entityName);
        }


    }
}
