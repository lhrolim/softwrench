using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared.Metadata.Applications.Command;
using softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared.Util;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {

    public class ApplicationSchemaDefinition : IDefinition {


        public IList<IApplicationDisplayable> Displayables = new List<IApplicationDisplayable>();

        public string SchemaId { get; set; }
        public virtual SchemaStereotype Stereotype { get; set; }
        public virtual SchemaMode? Mode { get; set; }
        public string Title { get; set; }

        public IDictionary<string, object> ExtensionParameters { get; set; }


        protected IList<ApplicationFieldDefinition> _fields = new List<ApplicationFieldDefinition>();
        public IEnumerable<ApplicationAssociationDefinition> Associations = new List<ApplicationAssociationDefinition>();
        public IEnumerable<ApplicationCompositionDefinition> Compositions = new List<ApplicationCompositionDefinition>();

        public String ApplicationName { get; set; }
        public ClientPlatform? Platform { get; set; }

        public bool Abstract { get; set; }
        public ApplicationSchemaDefinition ParentSchema { get; set; }

        public ApplicationCommandSchema CommandSchema { get; set; }

        public IDictionary<string, ISet<string>> _depandantFields = new Dictionary<string, ISet<string>>();

        public IDictionary<string, string> _properties;


        public ICollection<string> _fieldWhichHaveDeps = new HashSet<string>();

        public string Name { get { return ApplicationName; } }

        public string IdFieldName { get; set; }

        public ApplicationSchemaDefinition() {
        }

        public ApplicationSchemaDefinition(
            String applicationName, string title, string schemaId, SchemaStereotype stereotype,
            SchemaMode? mode, ClientPlatform? platform, bool @abstract,
            IList<IApplicationDisplayable> displayables,
            ApplicationSchemaDefinition parentSchema, ApplicationCommandSchema commandSchema) {
            if (displayables == null) throw new ArgumentNullException("displayables");

            ApplicationName = applicationName;
            Platform = platform;
            Displayables = displayables;
            ParentSchema = parentSchema;
            SchemaId = schemaId;
            Stereotype = stereotype;
            Abstract = @abstract;
            Mode = mode;
            CommandSchema = commandSchema;
            Title = title;
        }


        //        [JsonIgnore]
        public virtual IEnumerable<ApplicationFieldDefinition> RelationshipFields {
            get { return Fields.Where(f => f.Attribute.Contains(".")); }
        }

        public IEnumerable<ApplicationFieldDefinition> TransientFields {
            get { return Fields.Where(f => f.Attribute.Contains("#")); }
        }

        public virtual IList<ApplicationFieldDefinition> Fields {
            get { return _fields; }
        }


        public virtual IEnumerable<ApplicationFieldDefinition> NonRelationshipFields {
            get { return Fields.Where(f => !f.Attribute.Contains(".") && !f.Attribute.Contains("#")); }
        }

        protected IList<T> GetDisplayable<T>(Type displayableType) {
            return DisplayableUtil.GetDisplayable<T>(displayableType, Displayables);
        }


        public IEnumerable<ApplicationRelationshipDefinition> CollectionRelationships() {
            IEnumerable<ApplicationRelationshipDefinition> applicationAssociations = Associations.Where(a => a.Collection);
            IEnumerable<ApplicationRelationshipDefinition> applicationCompositions = Compositions.Where(a => a.Collection);
            return applicationAssociations.Union(applicationCompositions);
        }







        /// <summary>
        /// returns all the non inline composition this application holds, which will indicate to the screen 
        /// to place a navigator component enclosing the detail page.
        /// </summary>
        public IEnumerable<ApplicationCompositionDefinition> NonInlineComposition {
            get { return Compositions.Where(c => c.Schema.INLINE == false); }
        }

        /// <summary>
        /// returns all the non inline composition this application holds, which will indicate to the screen 
        /// to place a navigator component enclosing the detail page.
        /// </summary>
        public Boolean HasNonInlineComposition {
            get { return Compositions.Any(c => c.Schema.INLINE == false && !c.Hidden); }
        }


        public IDictionary<string, string> Properties {
            get { return _properties; }
        }



        public override string ToString() {
            return string.Format("Application:{0} ,SchemaId: {1}, Stereotype: {2}, Mode: {3}, ", ApplicationName, SchemaId, Stereotype, Mode);
        }


        protected bool Equals(ApplicationSchemaDefinition other) {
            return string.Equals(SchemaId, other.SchemaId) && Mode == other.Mode
                && string.Equals(ApplicationName, other.ApplicationName) && Platform == other.Platform;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationSchemaDefinition)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (SchemaId != null ? SchemaId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Mode.GetHashCode();
                hashCode = (hashCode * 397) ^ (ApplicationName != null ? ApplicationName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Platform;
                return hashCode;
            }
        }

        public ApplicationMetadataSchemaKey GetSchemaKey() {
            return new ApplicationMetadataSchemaKey(SchemaId, Mode, Platform);
        }

        public ICollection<string> FieldWhichHaveDeps {
            get { return _fieldWhichHaveDeps; }
        }



        public virtual IDictionary<string, ISet<string>> DepandantFields {
            get { return _depandantFields; }
        }

        public Boolean IsWebPlatform() {
            //TODO: multi level hierarchy
            return ClientPlatform.Web == Platform || (ParentSchema != null && ParentSchema.IsWebPlatform());
        }


    }
}