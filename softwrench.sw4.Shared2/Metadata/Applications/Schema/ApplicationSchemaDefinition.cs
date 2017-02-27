using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {

    public class ApplicationSchemaDefinition : BaseDefinition, IApplicationIdentifier, IApplicationDisplayableContainer, IPropertyHolder {


        public Dictionary<string, ApplicationCompositionSchema> CachedCompositions {
            get; set;
        }

        #region cache
        private readonly IDictionary<SchemaFetchMode, IList<ApplicationAssociationDefinition>> _cachedAssociations = new Dictionary<SchemaFetchMode, IList<ApplicationAssociationDefinition>>();
        private readonly IDictionary<SchemaFetchMode, IList<ApplicationCompositionDefinition>> _cachedCompositions = new Dictionary<SchemaFetchMode, IList<ApplicationCompositionDefinition>>();
        private readonly IDictionary<string, IEnumerable<IApplicationAttributeDisplayable>> _cachedFieldsOfTab = new Dictionary<string, IEnumerable<IApplicationAttributeDisplayable>>();

        private readonly IDictionary<SchemaFetchMode, IList<IApplicationIndentifiedDisplayable>> _cachedTabs = new Dictionary<SchemaFetchMode, IList<IApplicationIndentifiedDisplayable>>();

        private readonly IDictionary<SchemaFetchMode, IList<OptionField>> _cachedOptionFields = new Dictionary<SchemaFetchMode, IList<OptionField>>();
        #endregion

        private List<IApplicationDisplayable> _displayables = new List<IApplicationDisplayable>();

        private IDictionary<string, ApplicationEvent> _events = new Dictionary<string, ApplicationEvent>();

        private IDictionary<string, EntityAssociation> _offlineAssociations;

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


        public delegate IDictionary<string, EntityAssociation> LazyOfflineAssociationResolverDelegate(ApplicationSchemaDefinition definition);

        public delegate IEnumerable<IApplicationDisplayable> LazyComponentDisplayableResolver(ReferenceDisplayable reference, ApplicationSchemaDefinition schema, IEnumerable<DisplayableComponent> components);

        /// <summary>
        /// let´s wait to resolve the filters after all customizations, hirarchy merges have been applied
        /// </summary>
        /// <returns></returns>
        public delegate SchemaFilters LazySchemaFilterResolver(ApplicationSchemaDefinition definition);

        [JsonIgnore]
        public LazySchemaFilterResolver SchemaFilterResolver;

        [JsonIgnore]
        public LazyFkResolverDelegate FkLazyFieldsResolver;

        [JsonIgnore]
        public LazyOfflineAssociationResolverDelegate LazyOfflineAssociationResolver;

        [JsonIgnore]
        public LazyComponentDisplayableResolver ComponentDisplayableResolver;

        protected Lazy<IDictionary<string, EntityAssociation>> LazyEntityAssociation;


        public string SchemaId {
            get; set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(SchemaStereotype.None)]
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

        public string ApplicationTitle {
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

        private IDictionary<string, string> _properties = new Dictionary<string, string>();


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

        public string NoResultsNewSchema {
            get; set;
        }

        public bool DeclaredNoResultsNewSchema {
            get; set;
        }

        public bool PreventResultsNewSchema {
            get; set;
        }

        private bool _lazyFksResolved;

        public bool FiltersResolved {
            get; private set;
        }

        private bool _referencesResolved;

        private readonly bool _redeclaringSchema;

        [JsonIgnore]
        public SchemaFilters DeclaredFilters {
            get; set;
        }



        private SchemaFilters _schemaFilters;

        [DefaultValue(true)]
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
            string applicationName, string title, string schemaId, bool redeclaringSchema, string streotypeAttr, SchemaStereotype stereotype,
            SchemaMode? mode, ClientPlatform? platform, bool @abstract,
            List<IApplicationDisplayable> displayables, SchemaFilters declaredFilters, IDictionary<string, string> schemaProperties,
            ApplicationSchemaDefinition parentSchema, ApplicationSchemaDefinition printSchema, ApplicationCommandSchema commandSchema,
            string idFieldName, string userIdFieldName, string unionSchema, IEnumerable<ApplicationEvent> events = null) {
            CompositionSchemas = new Dictionary<string, ApplicationCompositionSchema>();
            if (displayables == null)
                throw new ArgumentNullException("displayables");
            EntityName = entityName;
            ApplicationName = applicationName;
            Platform = platform;
            _displayables = displayables;
            _redeclaringSchema = redeclaringSchema;
            ParentSchema = parentSchema;
            PrintSchema = printSchema;
            SchemaId = schemaId;
            Stereotype = stereotype;
            StereotypeAttr = streotypeAttr ?? stereotype.ToString().ToLower();
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
            DeclaredFilters = declaredFilters ?? SchemaFilters.BlankInstance();
        }

        public string StereotypeAttr {
            get; set;
        }


        [JsonIgnore]
        public virtual IEnumerable<ApplicationFieldDefinition> RelationshipFields {
            get {
                return Fields.Where(f => f.Attribute.Contains(".") && !f.Attribute.StartsWith("#"));
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

        /// <summary>
        /// Map of associations to be returned to the offline client side. Not intended to be used on server side, nor on online mode
        /// </summary>
        [CanBeNull]
        public IDictionary<string, EntityAssociation> OfflineAssociations {
            get {
                if (ClientPlatform.Mobile != Platform) {
                    //avoid unnecessary online overhead
                    return null;
                }
                if (_offlineAssociations != null || Fields == null) {
                    return _offlineAssociations;
                }

                _offlineAssociations = LazyOfflineAssociationResolver(this);
                return _offlineAssociations;

            }
        }

        public IEnumerable<ApplicationFieldDefinition> NonHiddenFields {
            get {
                return Fields.Where(f => !f.IsHidden);
            }
        }


        public IEnumerable<ApplicationFieldDefinition> HiddenFields {
            get {
                return Fields.Where(f => f.IsHidden);
            }
        }

        public IEnumerable<ApplicationFieldDefinition> SortableFields {
            get {
                return Fields.Where(f => (!f.IsHidden || IsForcedToBeSortable(f)) && !IsForcedNotToBeSortable(f));
            }
        }

        private static bool IsForcedToBeSortable(ApplicationFieldDefinition f) {
            if (f == null || f.RendererParameters == null || !f.RendererParameters.ContainsKey("forcesortable")) {
                return false;
            }

            var forcedSortable = f.RendererParameters["forcesortable"] as string;
            return "true".Equals(forcedSortable);
        }

        private static bool IsForcedNotToBeSortable(ApplicationFieldDefinition f) {
            if (f == null || f.RendererParameters == null || !f.RendererParameters.ContainsKey("showsort")) {
                return false;
            }

            var showsort = f.RendererParameters["showsort"] as string;
            return "false".Equals(showsort);
        }

        [CanBeNull]
        public SchemaFilters SchemaFilters {
            get {
                if (Stereotype != SchemaStereotype.List && Stereotype != SchemaStereotype.CompositionList) {
                    //only resolve it for list schemas
                    FiltersResolved = true;
                    return _schemaFilters;
                }

                if (SchemaFilterResolver != null && !FiltersResolved) {
                    _schemaFilters = SchemaFilterResolver(this);
                    FiltersResolved = true;
                }
                return _schemaFilters;
            }
            set {
                _schemaFilters = value;
            }
        }

        /// <summary>
        /// Holds a list of the Related compositions to be choosen from on the QuickSearch operation.
        /// 
        /// Only Make sense for List schemas, null otherwise
        /// 
        /// </summary>
        [CanBeNull]
        public IEnumerable<IAssociationOption> RelatedCompositions {
            get; set;
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


        public virtual IList<ApplicationAssociationDefinition> Associations(bool isShowMoreMode) {
            return Associations(isShowMoreMode ? SchemaFetchMode.SecondaryContent : SchemaFetchMode.MainContent);
        }

        [NotNull]
        public virtual IList<ApplicationAssociationDefinition> Associations(SchemaFetchMode mode = SchemaFetchMode.All) {
            if (_cachedAssociations.ContainsKey(mode)) {
                return _cachedAssociations[mode];
            }
            var result = GetDisplayable<ApplicationAssociationDefinition>(typeof(ApplicationAssociationDefinition), mode);
            _cachedAssociations[mode] = result;
            return result;
        }

        [NotNull]
        public virtual IList<ApplicationCompositionDefinition> Compositions(SchemaFetchMode mode = SchemaFetchMode.All) {
            if (_cachedCompositions.ContainsKey(mode)) {
                return _cachedCompositions[mode];
            }
            var result = GetDisplayable<ApplicationCompositionDefinition>(typeof(ApplicationCompositionDefinition), mode);
            _cachedCompositions[mode] = result;
            return result;
        }


        [NotNull]
        public virtual IList<IApplicationIndentifiedDisplayable> Tabs(SchemaFetchMode mode = SchemaFetchMode.All) {
            if (_cachedTabs.ContainsKey(mode)) {
                return _cachedTabs[mode];
            }
            var result = DisplayableUtil.GetDisplayable<IApplicationIndentifiedDisplayable>(new[] { typeof(ApplicationTabDefinition), typeof(ApplicationCompositionDefinition) }, Displayables, mode, false);
            _cachedTabs[mode] = result;
            return result;
        }

        [NotNull]
        public virtual IEnumerable<IApplicationAttributeDisplayable> NonHiddenFieldsOfTab(string tabId, SchemaFetchMode mode = SchemaFetchMode.All) {
            if (_cachedFieldsOfTab.ContainsKey(tabId)) {
                return _cachedFieldsOfTab[tabId];
            }
            var result = DisplayableUtil.GetDisplayable<IApplicationAttributeDisplayable>(new[] { typeof(IApplicationAttributeDisplayable) }, Displayables, mode, false, tabId == "main" ? null : tabId)
                .Where(f => !f.IsHidden && !f.Type.Equals(typeof(ApplicationSection).Name));
            var nonHiddenFieldsOfTab = result as IList<IApplicationAttributeDisplayable> ?? result.ToList();
            _cachedFieldsOfTab[tabId] = nonHiddenFieldsOfTab;
            return nonHiddenFieldsOfTab;
        }


        public virtual IList<OptionField> OptionFields(bool isShowMoreMode) {
            return OptionFields(isShowMoreMode ? SchemaFetchMode.SecondaryContent : SchemaFetchMode.MainContent);
        }

        public virtual IList<OptionField> OptionFields(SchemaFetchMode mode = SchemaFetchMode.All) {
            if (_cachedOptionFields.ContainsKey(mode)) {
                return _cachedOptionFields[mode];
            }
            var result = GetDisplayable<OptionField>(typeof(OptionField), mode);
            _cachedOptionFields[mode] = result;
            return result;
        }

        [JsonIgnore]
        public virtual IList<ApplicationRelationshipDefinition> Relationships {
            get {
                return GetDisplayable<ApplicationRelationshipDefinition>(typeof(ApplicationRelationshipDefinition));
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

        public IList<T> GetDisplayable<T>(Type displayableType = null, SchemaFetchMode mode = SchemaFetchMode.All) {
            if (displayableType == null) {
                displayableType = typeof(T);
            }

            return DisplayableUtil.GetDisplayable<T>(displayableType, Displayables, mode);
        }


        public IEnumerable<ApplicationRelationshipDefinition> CollectionRelationships() {
            IEnumerable<ApplicationRelationshipDefinition> applicationAssociations = Associations().Where(a => a.Collection);
            IEnumerable<ApplicationRelationshipDefinition> applicationCompositions = Compositions().Where(a => a.Collection);
            return applicationAssociations.Union(applicationCompositions);
        }





        private IEnumerable<int> GetCompositionIdx(bool inline) {
            var list = new List<int>();
            for (var i = 0; i < Displayables.Count; i++) {
                var displayable = Displayables[i];
                if (displayable is ApplicationCompositionDefinition &&
                    ((ApplicationCompositionDefinition)displayable).Inline == inline) {
                    list.Add(i);
                }
            }
            return list;
        }




        /// <summary>
        /// Indicates wheter there is any non inline composition in this application, which will indicate to the screen 
        /// to place a navigator component enclosing the detail page.
        /// </summary>
        public bool HasNonInlineComposition {
            get {
                return Compositions().Any(c => c.Schema.INLINE == false && !c.isHidden);
            }
        }

        /// <summary>
        /// Indicates whether there is any inline composition in this application.
        /// </summary>
        public bool HasInlineComposition {
            get {
                return Compositions().Any(c => c.Schema.INLINE == true && !c.isHidden);
            }
        }


        public IDictionary<string, string> Properties {
            get {
                return _properties;
            }
            set {
                _properties = value;
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
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((ApplicationSchemaDefinition)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (SchemaId != null ? SchemaId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Mode.GetHashCode();
                hashCode = (hashCode * 397) ^ (ApplicationName != null ? ApplicationName.GetHashCode() : 0);
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
            return (ClientPlatform.Web == Platform || ClientPlatform.Both == Platform || null == Platform) || (ParentSchema != null && ParentSchema.IsWebPlatform());
        }

        public bool IsMobilePlatform() {
            return SchemaId != ApplicationMetadataConstants.SyncSchema &&
                (ClientPlatform.Mobile == Platform || null == Platform || ClientPlatform.Both == Platform) || (ParentSchema != null && ParentSchema.IsMobilePlatform());
        }

        public bool IsPlatformSupported(ClientPlatform platform) {
            if (platform.Equals(ClientPlatform.Web)) {
                return IsWebPlatform();
            }
            return IsMobilePlatform();
        }


        public void DepandantFields(IDictionary<string, ISet<string>> fields) {
            _depandantFields = fields;
        }

        public override string ToString() {
            return string.Format("Application:{0} ,SchemaId: {1}, Stereotype: {2}, Mode: {3}, Platform:{4} ", ApplicationName, SchemaId, Stereotype, Mode, Platform);
        }

        public string GetProperty(string propertyKey) {
            if (Properties == null || !Properties.ContainsKey(propertyKey)) {
                return null;
            }
            return Properties[propertyKey];
        }

        public ApplicationSchemaDefinition PaginationSize() {
            throw new NotImplementedException();
        }

        public bool IsCreation() {
            return Stereotype.Equals(SchemaStereotype.DetailNew);
        }

        /// <summary>
        /// The corrsponding menu title of the given schema. Will be used for cached purposes internally by the framework
        /// </summary>
        public string MenuTitle {
            get;
            set;
        }

        public string Id {
            get {
                return SchemaId;
            }
            set {
                SchemaId = value;
            }
        }

        public SchemaRepresentation NewSchemaRepresentation {
            get; set;
        }

        public bool IgnoreCache {
            get; set;
        }


    }
}
