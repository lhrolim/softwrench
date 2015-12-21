using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class ApplicationSchemaCustomization : IApplicationDisplayableContainer, IApplicationDisplayable {

        public string Position { get; set; }

        [JsonIgnore]
        public ApplicationSchemaDefinition.LazyComponentDisplayableResolver ComponentDisplayableResolver;

        public ApplicationSchemaCustomization(string position, List<IApplicationDisplayable> displayables) {
            if (position == null) {
                throw new ArgumentNullException("position");
            }
            Position = position;
            Displayables = displayables;
        }

        public IList<T> GetDisplayable<T>(Type displayableType, SchemaFetchMode mode = SchemaFetchMode.All) {
            return DisplayableUtil.GetDisplayable<T>(displayableType, Displayables,mode);
        }

        public IList<ApplicationFieldDefinition> Fields {
            get { return GetDisplayable<ApplicationFieldDefinition>(typeof(ApplicationFieldDefinition)); }
        }

        [JsonIgnore]
        public virtual IList<ApplicationCompositionDefinition> Compositions {
            get {
                return GetDisplayable<ApplicationCompositionDefinition>(typeof(ApplicationCompositionDefinition));
            }
        }

        public List<IApplicationDisplayable> Displayables { get; set; }



        [JsonIgnore]
        public virtual IList<ApplicationAssociationDefinition> Associations {
            get { return GetDisplayable<ApplicationAssociationDefinition>(typeof(ApplicationAssociationDefinition)); }
        }

        [JsonIgnore]
        public virtual IList<ApplicationRelationshipDefinition> Relationships {
            get { return GetDisplayable<ApplicationRelationshipDefinition>(typeof(ApplicationRelationshipDefinition)); }
        }

        [JsonIgnore]
        public virtual IList<OptionField> OptionFields {
            get { return GetDisplayable<OptionField>(typeof(OptionField)); }
        }

        public override string ToString() {
            return string.Format("Position: {0}", Position);
        }

        public string RendererType { get; private set; }
        public string Type { get; private set; }
        public string Role { get; private set; }
        public string ShowExpression { get; set; }
        public string ToolTip { get; private set; }
        public string Label { get; private set; }
        public bool? ReadOnly { get; set; }
    }
}
