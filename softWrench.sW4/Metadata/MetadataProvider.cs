using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
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
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Exception;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sw4.Shared2.Metadata.Modules;
using softWrench.sW4.Util;
using System.Net;

namespace softWrench.sW4.Metadata {
    public class MetadataProvider {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProvider));

        private static MetadataProperties _globalProperties;
        private static EntityQueries _entityQueries;
        private static ICollection<EntityMetadata> _entityMetadata;
        private static IReadOnlyCollection<CompleteApplicationMetadataDefinition> _applicationMetadata;
        private static IDictionary<string, CommandBarDefinition> _commandBars;
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
            if (ApplicationConfiguration.IgnoreWsCertErrors) {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
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
                    var instance = SlicedEntityMetadataBuilder.GetInstance(entityMetadata, schema, app.FetchLimit);
                    SlicedEntityMetadataCache[new SlicedEntityMetadataKey(webSchema.Key, entityName)] = instance;
                    if (schema.CommandSchema !=null &&schema.CommandSchema.HasDeclaration) {
                        //mobile schemas, doesnt have command schema for now...
                        foreach (var overridenBarKey in schema.CommandSchema.ApplicationCommands.Keys)
                        {
                            //adding overriding command bars here
                            var overridenKey = "{0}_{1}_{2}.{3}".Fmt(schema.ApplicationName, schema.SchemaId, schema.Mode.ToString().ToLower(), overridenBarKey);
                            _commandBars[overridenKey] = schema.CommandSchema.ApplicationCommands[overridenBarKey];
                        }
                    }
                }

            }
        }



        [NotNull]
        public static EntityMetadata Entity([NotNull] string name) {
            if (name == null) throw new ArgumentNullException("name");
            var entityMetadata = _metadataXmlInitializer != null ? _metadataXmlInitializer.Entities : _entityMetadata;
            return entityMetadata.FirstWithException(a => String.Equals(a.Name, name, StringComparison.CurrentCultureIgnoreCase), "entity {0} not found", name);


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
        [NotNull]
        public static CompleteApplicationMetadataDefinition Application([NotNull] string name) {
            Validate.NotNull(name, "name");

            return _applicationMetadata
                .FirstWithException(a => String.Equals(a.ApplicationName, name, StringComparison.CurrentCultureIgnoreCase), "application {0} not found", name);
        }

        /// <summary>
        ///     Returns the metadata related to the given
        ///     application, specified by its name.
        /// </summary>
        /// <param name="commandId"></param>
        [NotNull]
        public static ICommandDisplayable Command(string commandId) {
            Validate.NotNull(commandId, "commandId");
            if (commandId.StartsWith("crud_")){
                //TODO: This is workaround to avoid exception when crud_
                return null;
            }
            var commandParts = commandId.Split('.');
            if (commandParts.Length != 2) {
                throw new InvalidOperationException("command Id should be in the form 'bar.command'");
            }
            var barKey = commandParts[0];
            if (!_commandBars.ContainsKey(barKey)) {
                throw MetadataException.CommandBarNotFound(barKey);
            }
            var commandBar = _commandBars[barKey];
            var commanddisplayableId = commandParts[1];
            var command = commandBar.FindById(commanddisplayableId);
            if (command == null) {
                throw MetadataException.CommandNotFound(commanddisplayableId, barKey);
            }
            return command;
        }

        /// <summary>
        ///     Returns the metadata related to the given
        ///     application, specified by its id.
        /// </summary>
        /// <param name="id">The unique identifier of the application.</param>
        [NotNull]
        public static CompleteApplicationMetadataDefinition Application(Guid id) {
            return _applicationMetadata.First(a => a.Id == id);
        }

        public static MenuDefinition Menu(ClientPlatform platform) {
            return _menus.ContainsKey(platform) ? _menus[platform] : null;
        }

        public static IList<string> SiteIds {
            get { return _siteIds; }
        }

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
        public void SaveStatus([NotNull] Stream data, ClientPlatform platform = ClientPlatform.Web)
        {
            try
            {
                //var newMenu = new MenuXmlInitializer().InitializeMenu(platform, data);
                using (var stream = File.Create(Path.GetFullPath("statuscolors.json")))
                {
                    data.CopyTo(stream);
                    stream.Flush();
                }
                _menus[platform] = newMenu;
            }
            catch (Exception e)
            {
                Log.Error("error saving menu", e);
                throw;
            }
        }

        private static void FillFields() {
            _siteIds = BuildSiteIds();
            _entityMetadata = _metadataXmlInitializer.Entities;
            _applicationMetadata = _metadataXmlInitializer.Applications;
            _commandBars = _metadataXmlInitializer.CommandBars;
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

        public static IDictionary<string, CommandBarDefinition> CommandBars() {
            return _commandBars;
        }
    }
}