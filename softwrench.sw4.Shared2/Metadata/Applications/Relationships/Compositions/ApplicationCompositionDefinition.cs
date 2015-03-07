using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softwrench.sW4.Shared2.Util;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {

    public class ApplicationCompositionDefinition : ApplicationRelationshipDefinition, IDependableField {


        private String _relationship;
        public bool isHidden { get; set; }
        private ApplicationCompositionSchema _schema;
        public ApplicationHeader Header { get; set; }

        public ApplicationCompositionDefinition() {

        }

        public ApplicationCompositionDefinition(string @from, string relationship, string label, ApplicationCompositionSchema schema, string showExpression, string toolTip, bool hidden, ApplicationHeader header)
            : base(from, label, showExpression, toolTip) {
            if (relationship == null) throw new ArgumentNullException("relationship");
            _relationship = relationship;
            _schema = schema;
            isHidden = hidden;
            Header = header;
            if (isHidden) {
                //if hidden then the detail schema can be marked as empty
                _schema.DetailSchema = "";
            }
        }


        public string Relationship {
            get { return EntityUtil.GetRelationshipName(_relationship); }
            set { _relationship = value; }
        }

        public override string RendererType {
            get {
                return _schema.Renderer.RendererType;
            }
        }

        public override string Role {
            get { return From + "." + Relationship; }
        }

        public override string Attribute { get { return _relationship; } set {  } }


        public ApplicationCompositionSchema Schema {
            get { return _schema; }
            set { _schema = value; }
        }

        public Boolean Inline {
            get { return _schema.INLINE; }
        }

        public override string ToString() {
            return string.Format("From: {0}, To: {1} , Collection: {2}", From, EntityAssociation, Collection);
        }

        public string TabId { get { return Relationship; } }

        public ISet<string> DependantFields {
            get { return _schema.DependantFields; }
        }

        public string AssociationKey { get { return Relationship; } }
    }
}
