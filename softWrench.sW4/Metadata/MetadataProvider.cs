using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net;
using softwrench.sw4.api.classes.user;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Metadata.Validator;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Exception;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sw4.Shared2.Metadata.Modules;
using softWrench.sW4.Util;
using cts.commons.Util;
using System.Net;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Stereotypes;

namespace softWrench.sW4.Metadata {
    public class MetadataProvider {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProvider));

        private static MetadataProperties _globalProperties;
        private static IDictionary<string, MetadataStereotype> _globalStereotypes;
        private static IDictionary<string, MetadataStereotype> _mergedStereotypes;

        // SWDB entities and applications
        private static EntityQueries _swdbentityQueries;
        private static ICollection<EntityMetadata> _swdbentityMetadata;
        private static IReadOnlyCollection<CompleteApplicationMetadataDefinition> _swdbapplicationMetadata;

        // MAximo entities and applications
        private static EntityQueries _entityQueries;
        private static ICollection<EntityMetadata> _entityMetadata;
        private static IReadOnlyCollection<CompleteApplicationMetadataDefinition> _applicationMetadata;
        private static IDictionary<string, CommandBarDefinition> _commandBars;

        /// <summary>
        /// Holds, for each application a corresponding role name that have been used on the menu to filter it (ex: application=servicerequest, but role was sr)
        /// </summary>
        public static IDictionary<string, string> ApplicationRoleAlias = new Dictionary<string, string>();

        private static IDictionary<ClientPlatform, MenuDefinition> _menus;
        private static readonly IDictionary<SlicedEntityMetadataKey, SlicedEntityMetadata> SlicedEntityMetadataCache = new Dictionary<SlicedEntityMetadataKey, SlicedEntityMetadata>();

        public static MetadataProviderInternalCache InternalCache {
            get; set;
        }



        private const string Metadata = "metadata.xml";
        private const string StatusColor = "statuscolors.json";
        private const string MenuPattern = "menu.{0}.xml";
        public static bool FinishedParsing {
            get; set;
        }

        private static MetadataXmlSourceInitializer _metadataXmlInitializer;

        private static SWDBMetadataXmlSourceInitializer _swdbmetadataXmlInitializer;

        public static void DoInit() {
            var before = Stopwatch.StartNew();
            InternalCache = null;
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
                _globalStereotypes = new StereotypesXmlInitializer().Initialize();

                var commandsInitializer = new CommandsXmlSourceInitializer();
                _commandBars = commandsInitializer.Validate();
                _metadataXmlInitializer = new MetadataXmlSourceInitializer();

                _mergedStereotypes = StereotypeFactory.MergeStereotypes(_globalStereotypes, _metadataXmlInitializer.InitializeCustomerStereotypes());

                _metadataXmlInitializer.Validate(_commandBars);
                _swdbmetadataXmlInitializer = new SWDBMetadataXmlSourceInitializer();
                _swdbmetadataXmlInitializer.Validate(_commandBars);

                _menus = new MenuXmlInitializer().Initialize();
                FillFields();
                FillRoleAlias();
                FinishedParsing = true;
                new MetadataXmlTargetInitializer().Validate();
                BuildSlicedMetadataCache();
            } catch (Exception) {
                Log.Error("error reading metadata");
                throw;
            } finally {
                _metadataXmlInitializer = null;
                _swdbmetadataXmlInitializer = null;
            }
        }

        private static void FillRoleAlias() {

            foreach (var leaf in _menus[ClientPlatform.Web].Leafs) {
                if (leaf is MenuContainerDefinition) {
                    var container = (MenuContainerDefinition)leaf;
                    if (container.ApplicationContainer != null && container.Role != null &&
                        container.ApplicationContainer != container.Role) {
                        if (!ApplicationRoleAlias.ContainsKey(container.ApplicationContainer)) {
                            ApplicationRoleAlias.Add(container.ApplicationContainer, container.Role);
                        }

                    }

                }
            }
        }


        private static void BuildSlicedMetadataCache() {
            var watch = Stopwatch.StartNew();
            SlicedEntityMetadataCache.Clear();
            IEnumerable<CompleteApplicationMetadataDefinition> apps = _applicationMetadata;
            if (_swdbapplicationMetadata != null) {
                apps = apps.Union(_swdbapplicationMetadata);
            }
            foreach (var app in apps) {
                var entityName = app.Entity;
                var entityMetadata = Entity(entityName);
                if (isMobileEnabled() && app.IsMobileSupported()) {
                    app.Schemas().Add(ApplicationMetadataSchemaKey.GetSyncInstance(),
                        ApplicationSchemaFactory.GetSyncInstance(entityName, app.ApplicationName, app.IdFieldName, app.UserIdFieldName));
                }
                foreach (var webSchema in app.Schemas()) {
                    var schema = webSchema.Value;

                    schema.DepandantFields(DependencyBuilder.BuildDependantFields(schema.Fields, schema.DependableFields));
                    schema._fieldWhichHaveDeps = schema.DependantFields().Keys;
                    if (schema.Stereotype.Equals(SchemaStereotype.List)) {
                        schema.DeclaredFilters.Merge(app.AppFilters);
                    }


                    var instance = SlicedEntityMetadataBuilder.GetInstance(entityMetadata, schema, app.FetchLimit);
                    SlicedEntityMetadataCache[new SlicedEntityMetadataKey(webSchema.Key, entityName)] = instance;

                    if (schema.CommandSchema != null && schema.CommandSchema.HasDeclaration) {
                        //mobile schemas, dont have command schema for now...
                        foreach (var overridenBarKey in schema.CommandSchema.ApplicationCommands.Keys) {
                            //adding overriding command bars here
                            var overridenKey = "{0}_{1}_{2}.{3}".Fmt(schema.ApplicationName, schema.SchemaId, schema.Mode.ToString().ToLower(), overridenBarKey);
                            _commandBars[overridenKey] = schema.CommandSchema.ApplicationCommands[overridenBarKey];
                        }
                    }
                }
                LoggingUtil.DefaultLog.DebugFormat("finished registering metadata {0}", app.ApplicationName);
            }
            LoggingUtil.DefaultLog.InfoFormat("Sliced metadata cache built in {0}", LoggingUtil.MsDelta(watch));
        }

        public static IEnumerable<EntityMetadata> Entities() {
            return _metadataXmlInitializer.Entities;
        }


        public static EntityMetadata Entity([NotNull] string name, Boolean throwException = true) {
            Validate.NotNull(name, "name");
            ICollection<EntityMetadata> entityMetadata;
            if (name.EndsWith("_")) {
                entityMetadata = _swdbmetadataXmlInitializer != null ? _swdbmetadataXmlInitializer.Entities : _swdbentityMetadata;
            } else {
                entityMetadata = _metadataXmlInitializer != null ? _metadataXmlInitializer.Entities : _entityMetadata;
            }
            if (throwException) {
                return entityMetadata.FirstWithException(a => String.Equals(a.Name, name, StringComparison.CurrentCultureIgnoreCase), "entity {0} not found", name);
            }
            var entity = entityMetadata.FirstOrDefault(a => String.Equals(a.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return entity;
        }


        /// <summary>
        ///     Returns metadata related to all applications in the catalog.
        /// </summary>
        [NotNull]
        public static IEnumerable<CompleteApplicationMetadataDefinition> Applications(bool includeSWDB = false) {
            if (includeSWDB && _swdbapplicationMetadata != null) {
                return _applicationMetadata.Union(_swdbapplicationMetadata);
            }

            return _applicationMetadata;
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
        public static CompleteApplicationMetadataDefinition Application([NotNull] string name, bool throwException = true) {
            if (name == null) throw new ArgumentNullException("name");
            Validate.NotNull(name, "name");
            var apps = name.StartsWith("_") ? _swdbapplicationMetadata : _applicationMetadata;

            if (!throwException) {
                return
                    apps.FirstOrDefault(
                        a => String.Equals(a.ApplicationName, name, StringComparison.CurrentCultureIgnoreCase));
            }
            return apps
                .FirstWithException(a => String.Equals(a.ApplicationName, name, StringComparison.CurrentCultureIgnoreCase), "application {0} not found", name);
        }

        /// <summary>
        ///     Returns the metadata related to the given
        ///     application, specified by its name.
        /// </summary>
        /// <param name="commandId"></param>
        [NotNull]
        public static ICommandDisplayable Command(string commandId, bool throwException = false) {
            Validate.NotNull(commandId, "commandId");
            if (commandId.StartsWith("crud_")) {
                //TODO: This is workaround to avoid exception when crud_
                return null;
            }
            var commandParts = commandId.Split('.');
            if (commandParts.Length != 2) {
                if (!throwException) {
                    return null;
                }
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

        public static bool isMobileEnabled() {
            return _menus.ContainsKey(ClientPlatform.Mobile);
        }


        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchTopLevelApps(ClientPlatform platform, [CanBeNull]ISWUser user) {
            var watch = Stopwatch.StartNew();
            var result = new HashSet<CompleteApplicationMetadataDefinition>();
            var menu = Menu(platform);
            var leafs = menu.ExplodedLeafs;
            foreach (var menuBaseDefinition in leafs) {
                if (menuBaseDefinition is ApplicationMenuItemDefinition) {
                    var application = Application((menuBaseDefinition as ApplicationMenuItemDefinition).Application);
                    if (user == null || user.IsInRole(application.Role) || (user.IsInRole(menuBaseDefinition.Role))) {
                        result.Add(application);
                    }
                }
            }

            Log.DebugFormat("fetching top level apps took: {0} ", LoggingUtil.MsDelta(watch));
            //TODO: add hidden menu items
            return result;
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

                _metadataXmlInitializer.Validate(_commandBars, data);

                _swdbmetadataXmlInitializer = new SWDBMetadataXmlSourceInitializer();
                _swdbmetadataXmlInitializer.Validate(_commandBars);

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
        public void SaveColor([NotNull] Stream data, bool internalFramework = false) {

            try {
                using (var stream = File.Create(MetadataParsingUtils.GetPath(StatusColor, internalFramework))) {

                    data.CopyTo(stream);
                    stream.Flush();
                }

            } catch (Exception e) {
                Log.Error("error saving statuscolor", e);
                throw;
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

            _entityMetadata = _metadataXmlInitializer.Entities;
            _applicationMetadata = _metadataXmlInitializer.Applications;
            _entityQueries = _metadataXmlInitializer.Queries;

            _swdbentityMetadata = _swdbmetadataXmlInitializer.Entities;
            _swdbapplicationMetadata = _swdbmetadataXmlInitializer.Applications;
            _swdbentityQueries = _swdbmetadataXmlInitializer.Queries;

        }

        #region Validate


        #endregion

        public static void StubReset() {
            FinishedParsing = false;
            ApplicationRoleAlias.Clear();
            //TODO: create some sort of clear cache event, for distributing responsabilities in an easier way
            InitializeMetadata();
            DynamicProxyUtil.ClearCache();
        }

        public static string GlobalProperty(string key, bool throwException = false, bool testRequired = false, bool fixedProfile = false) {
            return GlobalProperties.GlobalProperty(key, throwException, testRequired, fixedProfile);
        }

        [NotNull]
        public static IStereotype Stereotype([CanBeNull]string id) {
            if (id != null && _mergedStereotypes.ContainsKey(id)) {
                return _mergedStereotypes[id];
            }
            return new BlankStereotype();
        }

        class BlankStereotype : IStereotype {

            private IDictionary<string, string> _properties = new Dictionary<string, string>();

            public IStereotype Merge(IStereotype stereotype) {
                _properties = stereotype.StereotypeProperties();
                return this;
            }

            public IDictionary<string, string> StereotypeProperties() {
                return _properties;
            }
        }



        public static string EntityQuery(string key, bool throwException = true) {
            return _entityQueries.GetQuery(key, throwException);
        }

        public static string SwdbEntityQuery(string key, bool throwException = true) {
            return _entityQueries.GetQuery(key, throwException);
        }

        public static string TargetMapping() {
            var targetMapping = GlobalProperty(MetadataProperties.Target);
            return targetMapping ?? GlobalProperty(MetadataProperties.Source);
        }

        public static IDictionary<string, CommandBarDefinition> CommandBars() {
            return _commandBars;
        }

        public static CompleteApplicationMetadataDefinition GetCompositionApplication(ApplicationSchemaDefinition schema, string relationship) {
            if (relationship.StartsWith("#")) {
                //self relationship application
                return Application(schema.ApplicationName);
            }

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


        public static bool IsApplicationEnabled(string application) {
            return Application(application, false) != null;
        }

        [CanBeNull]
        public static string RoleByApplication(string applicationName) {
            var application = Application(applicationName);
            if (application == null) {
                return null;
            }
            return application.Role;

        }

        public static EntityMetadata EntityByApplication(string applicationName) {
            var application = Application(applicationName);
            return Entity(application.Entity);
        }
    }
}