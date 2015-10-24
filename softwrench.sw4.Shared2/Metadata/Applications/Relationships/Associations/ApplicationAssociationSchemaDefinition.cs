using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public class ApplicationAssociationSchemaDefinition {

        public AssociationDataProvider DataProvider { get; set; }
        public FieldRenderer Renderer { get; set; }
        public IDictionary<string, object> RendererParameters {
            get { return Renderer == null ? new Dictionary<string, object>() : Renderer.ParametersAsDictionary(); }
        }

        public FieldFilter Filter { get; set; }
        public IDictionary<string, object> FilterParameters {
            get { return Filter == null ? new Dictionary<string, object>() : Filter.ParametersAsDictionary(); }
        }

        protected HashSet<string> _dependantFields = new HashSet<string>();

        public ApplicationAssociationSchemaDefinition(AssociationDataProvider dataProvider, FieldRenderer renderer, FieldFilter filter) {
            DataProvider = dataProvider;
            Renderer = renderer ?? new AssociationFieldRenderer(AssociationFieldRenderer.AssociationRendererType.COMBO.ToString(), null, null, ComponentStereotype.None.ToString());
            Filter = filter;
        }

        public Boolean IsLazyLoaded {
            get { return ((AssociationFieldRenderer)Renderer).IsLazyLoaded; }
        }

        public HashSet<string> DependantFields {
            get { return _dependantFields; }
            set { _dependantFields = value; }
        }

        public bool IsPaginated { get { return ((AssociationFieldRenderer)Renderer).IsPaginated; } }

        public override string ToString() {
            return string.Format("DataProvider: {0}, Renderer: {1}, IsLazyLoaded: {2}", DataProvider, Renderer, IsLazyLoaded);
        }
    }
}
