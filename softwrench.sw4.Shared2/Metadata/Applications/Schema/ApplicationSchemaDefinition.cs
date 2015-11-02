using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sw4.Shared2.Metadata.Exception;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {

    public class ApplicationSchemaDefinition : BaseDefinition, IApplicationIdentifier, IApplicationDisplayableContainer {


        public Dictionary<string, ApplicationCompositionSchema> CachedCompositions {
            get; set;
        }

        private List<IApplicationDisplayable> _displayables = new List<IApplicationDisplayable>();

        private IDictionary<string, ApplicationEvent> _events = new Dictionary<string, ApplicationEvent>();

        /// <summary>
        /// This fields can only be resolved once the entire metadata.xml are parsed, so that´s why we are using this Lazy strategy.
        /// 
        /// 
        /// 
        /// </summary>
        /// 
        /// 
        /// 
        ///  public delegate byte[] Base64Delegate(string attachmentData);
        public delegate IList<IApplicationDisplayable> LazyFkResolverDelegate(ApplicationSchemaDefinition definition);

        public delegate IEnumerable<IApplicationDisplayable> LazyComponentDisplayableResolver(ReferenceDisplayable reference, ApplicationSchemaDefinition schema, IEnumerable<DisplayableComponent> components);

        /// <summary>
        /// let´s wait to resolve the filters after all customizations, hirarchy merges have been applied
        /// </summary>
        /// <returns></returns>
        public delegate SchemaFilters LazySchemaFilterResolver(ApplicationSchemaDefinition definition);

        [JsonIgnore]
        public ApplicationSchemaDefinition.LazySchemaFilterResolver SchemaFilterResolver;

        [JsonIgnore]
        public ApplicationSchemaDefinition.LazyFkResolverDelegate FkLazyFieldsResolver;

        [JsonIgnore]
        public LazyComponentDisplayableResolver ComponentDisplayableResolver;


        private IList<int> _tabs;
        private IList<int> _nonInlineCompositions;
        private IList<int> _inlineCompositions;

        public string SchemaId {
            get; set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual SchemaStereotype Stereotype {
            get; set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual SchemaMode? Mode {
            get; set;
        }
        public string Title {
            get; set;
        }

        public string UnionSchema {
            get; set;
        }

        public string ApplicationName {
            get; set;
        }

        public string EntityName {
            get; set;
        }

        public ClientPlatform? Platform {
            get; set;
        }

        public bool Abstract {
            get; set;
        }
        [JsonIgnore]
        public ApplicationSchemaDefinition ParentSchema {
            get; set;
        }

        public ApplicationSchemaDefinition PrintSchema {
            get; set;
        }

        public ApplicationCommandSchema CommandSchema {
            get; set;
        }

        private IDictionary<string, ISet<string>> _depandantFields = new Dictionary<string, ISet<string>>();

        private readonly IDictionary<string, string> _properties = new Dictionary<string, string>();


        public ICollection<string> _fieldWhichHaveDeps = new HashSet<string>();

        public string Name {
            get {
                return ApplicationName;
            }
        }

        public string IdFieldName {
            get; set;
        }

        public string UserIdFieldName {
            get; set;
        }

        public string IdDisplayable {
            get; set;
        }

        private bool _lazyFksResolved;

        private bool _filtersResolved;

        private bool _referencesResolved;

        private readonly bool _redeclaringSchema;

        [JsonIgnore]
        public SchemaFilters DeclaredFilters {
            get; set;
        }

        private SchemaFilters _schemaFilters;


        public bool RedeclaringSchema {
            get {
                return _redeclaringSchema;
            }
        }

        public IDictionary<string, ApplicationCompositionSchema> CompositionSchemas {
            get; set;
        }

        public ApplicationSchemaDefinition() {
            CompositionSchemas = new Dictionary<string, ApplicationCompositionSchema>();
        }

        public ApplicationSchemaDefinition(string entityName,
            string applicationName, string title, string schemaId, bool redeclaringSchema, SchemaStereotype stereotype,
            SchemaMode? mode, ClientPlatform? platform, bool @abstract,
            List<IApplicationDisplayable> displayables, SchemaFilters declaredFilters, IDictionary<string, string> schemaProperties,
            ApplicationSchemaDefinition parentSchema, ApplicationSchemaDefinition printSchema, ApplicationCommandSchema commandSchema,
            string idFieldName, string userIdFieldName, string unionSchema, IEnumerable<ApplicationEvent> events = null) {
            CompositionSchemas = new Dictionary<string, ApplicationCompositionSchema>();
            if (displayables == null) throw new ArgumentNullException("displayables");
            EntityName = entityName;
            ApplicationName = applicationName;
            Platform = platform;
            _displayables = displayables;
            _redeclaringSchema = redeclaringSchema;
            ParentSchema = parentSchema;
            PrintSchema = printSchema;
            SchemaId = schemaId;
            Stereotype = stereotype;
            Abstract = @abstract;
            Mode = mode;
            CommandSchema = commandSchema;
            Title = title;
            if (schemaProperties != null) {
                _properties = schemaProperties;
            }

            IdFieldName = idFieldName;
            UserIdFieldName = userIdFieldName;
            UnionSchema = unionSchema;

            if (events != null) {
                _events = events.ToDictionary(f => f.Type, f => f);
            }

            //to avoid eventual null pointers
            DeclaredFilters = declaredFilters ?? new SchemaFilters(new LinkedList<BaseMetadataFilter>());
        }




        [JsonIgnore]
        public virtual IEnumerable<ApplicationFieldDefinition> RelationshipFields {
            get {
                return Fields.Where(f => f.Attribute.Contains("."));
            }
        }

        [JsonIgnore]
        public IEnumerable<ApplicationFieldDefinition> TransientFields {
            get {
                return Fields.Where(f => f.Attribute.Contains("#"));
            }
        }

        //TODO: test the JsonIgnore field on Ipad
        [JsonIgnore]
        public IList<ApplicationFieldDefinition> Fields {
            get {
                return GetDisplayable<ApplicationFieldDefinition>(typeof(ApplicationFieldDefinition));
            }
        }

        public IEnumerable<ApplicationFieldDefinition> NonHiddenFields {
            get {
                return Fields.Where(f => !f.IsHidden);
            }
        }

        [JsonIgnore]
        public virtual IList<ApplicationCompositionDefinition> Compositions {
            get {
                return GetDisplayable<ApplicationCompositionDefinition>(typeof(ApplicationCompositionDefinition));
            }
        }


        public SchemaFilters SchemaFilters {
            get {
                if (Stereotype != SchemaStereotype.List) {
                    //only resolve it for list schemas
                    _filtersResolved = true;
                    return _schemaFilters;
                }

                if (SchemaFilterResolver != null && !_filtersResolved) {
                    _schemaFilters = SchemaFilterResolver(this);
                    _filtersResolved = true;
                }
                return _schemaFilters;
            }
            set {
                _schemaFilters = value;
            }
        }


        public List<IApplicationDisplayable> Displayables {
            get {
                //run this piece of code, just once, in the web app version, before mobile serialization.
                if (FkLazyFieldsResolver != null && !_lazyFksResolved) {
                    var resultList = FkLazyFieldsResolver(this);
                    if (resultList != null) {
                        //this will happen only when this method is invoked after the full Metadata serialization
                        foreach (var displayable in resultList) {
                            if (!_displayables.Contains(displayable)) {
                                _displayables.Add(displayable);
                            }
                        }
                        _lazyFksResolved = true;
                    }


                }

                if (!_referencesResolved && ComponentDisplayableResolver != null) {
                    var performReferenceReplacement = DisplayableUtil.PerformReferenceReplacement(_displayables, this, ComponentDisplayableResolver);
                    if (performReferenceReplacement != null) {
                        _displayables = performReferenceReplacement;
                        _referencesResolved = true;
                    }
                }
                return _displayables;
            }
            set {
                _displayables = value;
            }
        }


        [JsonIgnore]
        public virtual IList<ApplicationAssociationDefinition> Associations {
            get {
                return GetDisplayable<ApplicationAssociationDefinition>(typeof(ApplicationAssociationDefinition));
            }
        }

        [JsonIgnore]
        public virtual IList<ApplicationRelationshipDefinition> Relationships {
            get {
                return GetDisplayable<ApplicationRelationshipDefinition>(typeof(ApplicationRelationshipDefinition));
            }
        }

        [JsonIgnore]
        public virtual IList<OptionField> OptionFields {
            get {
                return GetDisplayable<OptionField>(typeof(OptionField));
            }
        }

        [JsonIgnore]
        public virtual IList<IDependableField> DependableFields {
            get {
                return GetDisplayable<IDependableField>(typeof(IDependableField));
            }
        }

        [JsonIgnore]
        public virtual IList<IDataProviderContainer> DataProviderContainers {
            get {
                return GetDisplayable<IDataProviderContainer>(typeof(IDataProviderContainer));
            }
        }

        [JsonIgnore]
        public IEnumerable<ApplicationFieldDefinition> NonRelationshipFields {
            get {
                long before = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                var applicationFieldDefinitions = Fields.Where(f => !f.Attribute.Contains(".") && (!f.Attribute.Contains("#") || f.Attribute.StartsWith("#null")));
                long after = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                return applicationFieldDefinitions;
            }
        }

        public IList<T> GetDisplayable<T>(Type displayableType, bool fetchInner = true) {
            return DisplayableUtil.GetDisplayable<T>(displayableType, Displayables, fetchInner);
        }


        public IEnumerable<ApplicationRelationshipDefinition> CollectionRelationships() {
            IEnumerable<ApplicationRelationshipDefinition> applicationAssociations = Associations.Where(a => a.Collection);
            IEnumerable<ApplicationRelationshipDefinition> applicationCompositions = Compositions.Where(a => a.Collection);
            return applicationAssociations.Union(applicationCompositions);
        }


        public IEnumerable<int> TabsIdxs {

            get {
                if (_tabs != null) {
                    return _tabs;
                }
                var list = new List<int>();
                for (var i = 0; i < Displayables.Count; i++) {
                    var displayable = Displayables[i];
                    if (displayable is ApplicationTabDefinition) {
                        list.Add(i);
                    }
                }
                _tabs = list;
                return list;
            }
        }


        /// <summary>
        /// returns all the non inline composition this application holds, which will indicate to the screen 
        /// to place a navigator component enclosing the detail page.
        /// 
        /// This List must be returned in json, as this elements won´t respect default displayables ordering. 
        /// We´re returning indexes, in order to keep the json small. The compositions can be then fetched from the Displayables field
        /// 
        /// </summary>
        public IEnumerable<int> NonInlineCompositionIdxs {
            get {
                return GetCompositionIdx(false, _nonInlineCompositions);
            }
        }

        /// <summary>
        /// returns all the inline composition this application holds.
        /// 
        /// This List must be returned in json, as this elements won´t respect default displayables ordering. 
        /// We´re returning indexes, in order to keep the json small. The compositions can be then fetched from the Displayables field
        /// 
        /// </summary>
        public IEnumerable<int> InlineCompositionIdxs {

            get {
                return GetCompositionIdx(true, _inlineCompositions);
            }
        }

        private IEnumerable<int> GetCompositionIdx(bool inline, IList<int> cacheList) {
            if (cacheList != null) {
                return cacheList;
            }
            var list = new List<int>();
            for (var i = 0; i < Displayables.Count; i++) {
                var displayable = Displayables[i];
                if (displayable is ApplicationCompositionDefinition &&
                    ((ApplicationCompositionDefinition)displayable).Inline == inline) {
                    list.Add(i);
                }
            }
            cacheList = list;
            return list;
        }




        /// <summary>
        /// Indicates wheter there is any non inline composition in this application, which will indicate to the screen 
        /// to place a navigator component enclosing the detail page.
        /// </summary>
        public bool HasNonInlineComposition {
            get {
                return Compositions.Any(c => c.Schema.INLINE == false && !c.isHidden);
            }
        }

        /// <summary>
        /// Indicates whether there is any inline composition in this application.
        /// </summary>
        public bool HasInlineComposition {
            get {
                return Compositions.Any(c => c.Schema.INLINE == true && !c.isHidden);
            }
        }


        public IDictionary<string, string> Properties {
            get {
                return _properties;
            }
        }


        public IDictionary<string, ApplicationEvent> Events {
            get {
                return _events;
            }
            set {
                _events = value;
            }
        }

        [JsonIgnore]
        public ISet<ApplicationEvent> EventSet {
            get {
                return new HashSet<ApplicationEvent>(_events.Values);
            }
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

        public string GetApplicationKey() {
            return ApplicationName + "." + SchemaId;
        }

        public ICollection<string> FieldWhichHaveDeps {
            get {
                return _fieldWhichHaveDeps;
            }
            set {
                _fieldWhichHaveDeps = value;
            }
        }

        /// <summary>
        /// For each field, a list of fields that depends upon it. 
        /// Both Associations or OptionField can be marked depending on fields on metadata.xml
        /// Ex: when we select the siteID we might need to update both asset and location.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, ISet<string>> DependantFields() {
            return _depandantFields;
        }

        public bool IsWebPlatform() {
            //TODO: multi level hierarchy
            return ClientPlatform.Web == Platform || (ParentSchema != null && ParentSchema.IsWebPlatform());
        }

        public bool IsMobilePlatform() {
            return SchemaId != ApplicationMetadataConstants.SyncSchema &&
                (ClientPlatform.Mobile == Platform || null == Platform) || (ParentSchema != null && ParentSchema.IsMobilePlatform());
        }


        public void DepandantFields(IDictionary<string, ISet<string>> fields) {
            _depandantFields = fields;
        }

        public override string ToString() {
            return string.Format("Application:{0} ,SchemaId: {1}, Stereotype: {2}, Mode: {3}, Platform:{4} ", ApplicationName, SchemaId, Stereotype, Mode, Platform);
        }

        public string GetProperty(string propertyKey) {
            if (!Properties.ContainsKey(propertyKey)) {
                return null;
            }
            return Properties[propertyKey];
        }

        public ApplicationSchemaDefinition PaginationSize() {
            throw new NotImplementedException();
        }
    }
}