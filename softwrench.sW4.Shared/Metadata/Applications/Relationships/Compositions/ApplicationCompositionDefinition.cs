using System;
using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared.Metadata.Entity.Association;
using softwrench.sW4.Shared.Util;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Compositions {

    public class ApplicationCompositionDefinition : ApplicationRelationshipDefinition {


        private String _relationship;
        public bool Hidden { get; set; }
        private readonly ApplicationCompositionSchema _schema;

        public IDictionary<string, object> GenericParameters { get; set; }

        public ApplicationCompositionDefinition() {

        }

        public ApplicationCompositionDefinition(string @from, string relationship, string label, ApplicationCompositionSchema schema, string showExpression, string toolTip, bool hidden)
            : base(from, label, showExpression, toolTip) {
            if (relationship == null) throw new ArgumentNullException("relationship");
            _relationship = relationship;
            _schema = schema;
            Hidden = hidden;
        }

        protected override EntityAssociation LookupEntityAssociation() {
            //
            //            var metadata = MetadataProvider.Application(From);
            //            var suffixed = EntityUtil.GetRelationshipName(_relationship);
            //
            //            return MetadataProvider
            //                .Entity(MetadataProvider.Application(From).Entity)
            //                .Associations
            //                .FirstOrDefault(a => a.Qualifier == suffixed);
            return null;
        }

        public string Relationship {
            get { return EntityUtil.GetRelationshipName(_relationship); }
            set { _relationship = value; }
        }

        public override string RendererType {
            get {
                return Collection ? _schema.Renderer.RendererType : CompositionFieldRenderer.CompositionRendererType.INLINE.ToString();
            }
        }

        public override string Role {
            get { return From + "." + Relationship; }
        }



        public ApplicationCompositionSchema Schema {
            get { return _schema; }
        }

        public Boolean Inline {
            get { return _schema.INLINE; }
        }



    }
}
