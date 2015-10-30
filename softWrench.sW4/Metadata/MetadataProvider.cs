using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Menu;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Metadata.Validator;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sw4.Shared2.Metadata.Modules;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata {
    public class MetadataProvider {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProvider));

        private static MetadataProperties _globalProperties;
        private static EntityQueries _entityQueries;
        private static ICollection<EntityMetadata> _entityMetadata;
        private static IReadOnlyCollection<CompleteApplicationMetadataDefinition> _applicationMetadata;
        private static IDictionary<ClientPlatform, MenuDefinition> _menus;
        private static readonly IDictionary<SlicedEntityMetadataKey, SlicedEntityMetadata> SlicedEntityMetadataCache = new Dictionary<SlicedEntityMetadataKey, SlicedEntityMetadata>();


        private static IList<string> _siteIds;
        private const string Metadata = "metadata.xml";
        private const string MenuPattern = "menu.{0}.xml";
        public static bool FinishedParsing { get; set; }

        private static MetadataXmlSourceInitializer _metadataXmlInitializer;

        public static void DoInit() {
            var before = Stopwatch.StartNew();
            InitializeMetadata();
            //force eager initialization to allow eager catching of errors.
//            DataSetProvider.GetInstance();
            var msDelta = LoggingUtil.MsDelta(before);
            Log.Info(String.Format("Finished metadata registry in {0}", msDelta));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void InitializeMetadata() {
            try {
                FinishedParsing = false;
                //this is needed because we may access the API method inside the validation process
                //                _metadataValidator = new MetadataValidator();
                _globalProperties = new PropertiesXmlInitializer().Initialize();
                _metadataXmlInitializer = new MetadataXmlSourceInitializer();
                _metadataXmlInitializer.Validate();
                _menus = new MenuXmlInitializer().Initialize();
                FillFields();
                FinishedParsing = true;
                new MetadataXmlTargetInitializer().Validate();
                BuildSlicedMetadataCache();
            } catch (Exception e) {
                Log.Error("error reading metadata", e);
                throw;
            } finally {
                _metadataXmlInitializer = null;
            }
        }

        private static IList<string> BuildSiteIds() {
            string siteIds = GlobalProperty(MetadataProperties.MaximoSiteIds, false);
            if (String.IsNullOrEmpty(siteIds)) {
                return new List<string>();
            }
            return siteIds.Split(',');
        }


        private static void BuildSlicedMetadataCache() {
            SlicedEntityMetadataCache.Clear();
            foreach (var app in _applicationMetadata) {
                var entityName = app.Entity;
                var entityMetadata = Entity(entityName);
                if (app.IsMobileSupported()) {
                    app.Schemas().Add(ApplicationMetadataSchemaKey.GetSyncInstance(),
                        ApplicationSchemaFactory.GetSyncInstance(app.ApplicationName, app.IdFieldName));
                }
                foreach (var webSchema in app.Schemas()) {
                    var schema = webSchema.Value;
                    schema.DepandantFields(DependencyBuilder.BuildDependantFields(schema.Fields, schema.DependableFields));
                    schema._fieldWhichHaveDeps = schema.DependantFields.Keys;
                    var instance = SlicedEntityMetadataBuilder.GetInstance(entityMetadata, schema, app.FetchLimit);
                    SlicedEntityMetadataCache[new SlicedEntityMetadataKey(webSchema.Key, entityName)] = instance;
                }

            }
        }



        [NotNull]
        public static EntityMetadata Entity([NotNull] string name) {
            if (name == null) throw new ArgumentNullException("name");
            var entityMetadata = _metadataXmlInitializer != null ? _metadataXmlInitializer.Entities : _entityMetadata;
            return entityMetadata.FirstWithException(a => String.Equals(a.Name, name, StringComparison.CurrentCultureIgnoreCase), "entity {0} not found", name);


        }

        public static ApplicationSchemaDefinition FindSchemaDefinition(string fullKey) {
            var appAndSchema = SchemaUtil.ParseApplicationAndKey(fullKey);
            var appName = appAndSchema.Item1;
            var app = Application(appName);
            return app.Schema(appAndSchema.Item2);
        }

        /// <summary>
        ///     Returns metadata related to all applications in the catalog.
        /// </summary>
        [NotNull]
        public static IEnumerable<CompleteApplicationMetadataDefinition> Applications() {
            return _applicationMetadata;
        }

        internal static ICollection<EntityMetadata> Entities() {
            return _metadataXmlInitializer != null ? _metadataXmlInitializer.Entities : _entityMetadata;
        }

        public static List<ModuleDefinition> Modules(ClientPlatform platform) {
            return Menu(platform).Modules;
        }

        public static MetadataProperties GlobalProperties {
            get {
                return _globalProperties;
            }
        }

        /// <summary>
        ///     Returns metadata related to all applications in the catalog
        ///     available for the specified client platform.
        /// </summary>
        /// <param name="platform">The client platform.</param>
        [NotNull]
        public static IEnumerable<CompleteApplicationMetadataDefinition> Applications(ClientPlatform platform) {
            return _applicationMetadata
                .Where(a => a.IsSupportedOnPlatform(platform));
        }

        /// <summary>
        ///     Returns the metadata related to the given
        ///     application, specified by its name.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="throwException"></param>
        public static CompleteApplicationMetadataDefinition Application([NotNull] string name, bool throwException=true) {
            if (name == null) throw new ArgumentNullException("name");
            if (!throwException) {
                return
                    _applicationMetadata.FirstOrDefault(
                        a => String.Equals(a.ApplicationName, name, StringComparison.CurrentCultureIgnoreCase));
            }
            return _applicationMetadata
                .FirstWithException(a => String.Equals(a.ApplicationName, name, StringComparison.CurrentCultureIgnoreCase), "application {0} not found", name);
        }

        /// <summary>
        ///     Returns the metadata related to the given
        ///     application, specified by its id.
        /// </summary>
        /// <param name="id">The unique identifier of the application.</param>
        [NotNull]
        public static CompleteApplicationMetadataDefinition Application(Guid id) {
            return _applicationMetadata
                .First(a => a.Id == id);
        }

        public static MenuDefinition Menu(ClientPlatform platform) {
            return _menus.ContainsKey(platform) ? _menus[platform] : null;
        }

        public static IList<string> SiteIds {
            get { return _siteIds; }
        }

        public static MetadataProviderInternalCache InternalCache { get; set; }


        [NotNull]
        public static SlicedEntityMetadata SlicedEntityMetadata(ApplicationMetadata applicationMetadata) {
            return SlicedEntityMetadataCache[new SlicedEntityMetadataKey(applicationMetadata.Schema.GetSchemaKey(), applicationMetadata.Entity)];
        }

        [NotNull]
        public StreamReader GetStream(string resource) {
            return MetadataParsingUtils.GetStreamImpl(resource);
        }

        public void Save([NotNull] Stream data, bool internalFramework = false) {
            try {
                _metadataXmlInitializer = new MetadataXmlSourceInitializer();
                _metadataXmlInitializer.Validate(data);
                using (var stream = File.Create(MetadataParsingUtils.GetPath(Metadata, internalFramework))) {
                    data.CopyTo(stream);
                    stream.Flush();
                }
                FillFields();
            } catch (Exception e) {
                Log.Error("error saving metadata", e);
                throw;
            } finally {
                _metadataXmlInitializer = null;

            }
        }

        public void SaveMenu([NotNull] Stream data, ClientPlatform platform = ClientPlatform.Web) {
            try {
                var newMenu = new MenuXmlInitializer().InitializeMenu(platform, data);
                using (var stream = File.Create(MenuXmlInitializer.GetMenuPath(platform))) {
                    data.CopyTo(stream);
                    stream.Flush();
                }
                _menus[platform] = newMenu;
            } catch (Exception e) {
                Log.Error("error saving menu", e);
                throw;
            }
        }

        private static void FillFields() {
            _siteIds = BuildSiteIds();
            _entityMetadata = _metadataXmlInitializer.Entities;
            _applicationMetadata = _metadataXmlInitializer.Applications;
            _entityQueries = _metadataXmlInitializer.Queries;
        }

        #region Validate


        #endregion

        public static void StubReset() {
            FinishedParsing = false;
            //TODO: remove this stub, monitor changes on XML?
            //TODO: create some sort of clear cache event, for distributing responsabilities in an easier way
            InitializeMetadata();
            DynamicProxyUtil.ClearCache();
        }

        public static string GlobalProperty(string key, bool throwException = false, bool testRequired = false) {
            return GlobalProperties.GlobalProperty(key, throwException, testRequired);
        }

        public static string EntityQuery(string key, bool throwException = true) {
            return _entityQueries.GetQuery(key, throwException);
        }

        public static string TargetMapping() {
            var targetMapping = GlobalProperty(MetadataProperties.Target);
            return targetMapping ?? GlobalProperty(MetadataProperties.Source);
        }

        public static CompleteApplicationMetadataDefinition GetCompositionApplication(ApplicationSchemaDefinition schema, string relationship) {
            var application = Application(EntityUtil.GetApplicationName(relationship), false);
            if (application != null) {
                return application;
            }
            var parentAppName = schema.ApplicationName;
            var entityName = Application(parentAppName).Entity;
            var entity = Entity(entityName);
            var association = entity.Associations.FirstWithException(f => f.Qualifier == relationship, "could not locate relationship with qualifier {0}", relationship);
            var realName = association.To;
            return Application(realName);
        }
    }
}