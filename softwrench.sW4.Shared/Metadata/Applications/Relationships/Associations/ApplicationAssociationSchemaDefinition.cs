using System;
using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Applications.UI;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations {

    public class ApplicationAssociationSchemaDefinition {

        public AssociationDataProvider DataProvider { get; set; }
        public FieldRenderer Renderer { get; set; }
        protected ISet<string> _dependantFields = new HashSet<string>();

        public ApplicationAssociationSchemaDefinition(AssociationDataProvider dataProvider, FieldRenderer renderer) {
            DataProvider = dataProvider;
            Renderer = renderer ?? new AssociationFieldRenderer(AssociationFieldRenderer.AssociationRendererType.COMBO.ToString(), null, null);
            //            if (_dataProvider != null) {
            //                _dependantFields = DependencyBuilder.TryParsingDependentFields(_dataProvider.WhereClause);
            //            }
            //            if (dependantFields != null) {
            //                var fields = dependantFields.Split(',');
            //                foreach (var field in fields) {
            //                    _dependantFields.Add(field);
            //                }
            //            }
        }

        public Boolean IsLazyLoaded {
            get { return ((AssociationFieldRenderer)Renderer).IsLazyLoaded; }
        }

        public ISet<string> DependantFields {
            get { return _dependantFields; }
        }

        public override string ToString() {
            return string.Format("DataProvider: {0}, Renderer: {1}, IsLazyLoaded: {2}", DataProvider, Renderer, IsLazyLoaded);
        }
    }
}
