using System;
using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared.Metadata.Entity.Association;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations {

    public abstract class ApplicationRelationshipDefinition : IApplicationDisplayable,IDefinition {

        public string From { get; set; }
        public string Label { get; set; }
        public abstract string RendererType { get; }
        public string Type { get { return GetType().Name; } }
        public abstract string Role { get; }
        public string ShowExpression { get; set; }
        public string ToolTip { get; set; }

        public IDictionary<string, object> ExtensionParameters{ get; set; }

        private readonly Lazy<EntityAssociation> _entityAssociation;

        protected ApplicationRelationshipDefinition()
        {
            
        }

        protected ApplicationRelationshipDefinition(string @from, string label, string showExpression, string toolTip) {
            if (@from == null) throw new ArgumentNullException("from");
            From = @from;
            Label = label;
            ShowExpression = showExpression;
            ToolTip = toolTip;
            _entityAssociation = new Lazy<EntityAssociation>(LookupEntityAssociation);
        }

        protected abstract EntityAssociation LookupEntityAssociation();

        public bool Collection { get { return EntityAssociation != null && EntityAssociation.Collection; } }
        public bool Resolved { get { return !Collection; } }

        public EntityAssociation EntityAssociation {
            get { return _entityAssociation.Value; }
        }
    }
}
