using System;
using System.Collections.Generic;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public abstract class ApplicationRelationshipDefinition : BaseDefinition, IApplicationDisplayable {

        public string From { get; set; }
        public string Label { get; set; }
        public virtual string RendererType { get; set; }
        public abstract IDictionary<string, string> RendererParameters { get; }
        public string Type { get { return GetType().Name; } }
        public abstract string Role { get; }
        public string ShowExpression { get; set; }
        public string EnableExpression { get; set; }
        public string ToolTip { get; set; }
        public bool? ReadOnly { get; set; }


        protected Lazy<EntityAssociation> LazyEntityAssociation;

        private EntityAssociation _entityAssociation;

        protected ApplicationRelationshipDefinition() {

        }

        protected ApplicationRelationshipDefinition(string @from, string label, string showExpression, string toolTip) {
            if (@from == null) throw new ArgumentNullException("from");
            From = @from;
            Label = label;
            ShowExpression = showExpression;
            ToolTip = toolTip;
            LazyEntityAssociation = new Lazy<EntityAssociation>(LookupEntityAssociation);
        }

        private EntityAssociation LookupEntityAssociation() {
            return null;
        }

        public bool Collection { get { return EntityAssociation != null && EntityAssociation.Collection; } }
        public bool Resolved { get { return !Collection; } }

        public EntityAssociation EntityAssociation {
            get { return _entityAssociation ?? (LazyEntityAssociation == null ? null : LazyEntityAssociation.Value); }
            set { _entityAssociation = value; }
        }

        public void SetLazyResolver(Lazy<EntityAssociation> resolver) {
            LazyEntityAssociation = resolver;
        }


    }
}
