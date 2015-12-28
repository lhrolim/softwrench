using System;
using System.ComponentModel;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public abstract class ApplicationRelationshipDefinition : BaseDefinition, IApplicationIndentifiedDisplayable {

        public string From { get; set; }
        [DefaultValue("")] public string Label { get; set; }
        public virtual string RendererType { get; set; }
        public string Type { get { return GetType().Name; } }
        public abstract string Role { get; }
        [DefaultValue("true")] public string ShowExpression { get; set; }
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

        public virtual bool Collection { get { return EntityAssociation != null && EntityAssociation.Collection; } }
        public bool Resolved { get { return !Collection; } }

        public EntityAssociation EntityAssociation {
            get { return _entityAssociation ?? (LazyEntityAssociation == null ? null : LazyEntityAssociation.Value); }
            set { _entityAssociation = value; }
        }

        public void SetLazyResolver(Lazy<EntityAssociation> resolver) {
            LazyEntityAssociation = resolver;
        }


        protected bool Equals(ApplicationRelationshipDefinition other) {
            return string.Equals(Role, other.Role);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationRelationshipDefinition)obj);
        }

        public override int GetHashCode() {
            return (Role != null ? Role.GetHashCode() : 0);
        }


        public abstract string Attribute { get; set; }
    }
}
