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
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using EntityUtil = softWrench.sW4.Util.EntityUtil;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Data.Persistence.SWDB;
using cts.commons.persistence.Util;
using softWrench.sW4.Data.Persistence;

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
        private static System.Collections.Generic.ISet<string> _appsAndEntitiesUsedCache = null;

        private static readonly IList<CompleteApplicationMetadataDefinition> TransientApplicationMetadataDefinitions = new List<CompleteApplicationMetadataDefinition>();



        /// <summary>
        /// Holds, for each application a corresponding role name that have been used on the menu to filter it (ex: application=servicerequest, but role was sr)
        /// </summary>
        public static IDictionary<string, string> ApplicationRoleAlias = new Dictionary<string, string>();

        private static IDictionary<ClientPlatform, MenuDefinition> _menus;
        private static readonly IDictionary<SlicedEntityMetadataKey, SlicedEntityMetadata> SlicedEntityMetadataCache = new Dictionary<SlicedEntityMetadataKey, SlicedEntityMetadata>();

        public static MetadataProviderInternalCache InternalCache {
            get; set;
        }

        public const string METADATA_FILE = "metadata.xml";
        public const string STATUS_COLOR_FILE = "statuscolors.json";
        public const string CLASSIFICATION_COLOR_FILE = "classificationcolors.json";
        public const string MENU_WEB_FILE = "menu.web.xml";
        public const string PROPERTIES_FILE = "properties.xml";

        public static bool FinishedParsing {
            get; set;
        }
        
        //before the application is fully merged
        public static IDictionary<string, HashSet<DisplayableComponent>> ComponentsDictionary = new Dictionary<string, HashSet<DisplayableComponent>>();

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
                ServicePointManager.ServerCertificateValidationCallback = delegate {
                    return true;
                };
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void InitializeMetadata() {
            try {
                _appsAndEntitiesUsedCache = null;
                ComponentsDictionary.Clear();
                TransientApplicationMetadataDefinitions.Clear();
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
                TransientApplicationMetadataDefinitions.Clear();
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


        private static void ApplyListSpecificLogic(CompleteApplicationMetadataDefinition app, ApplicationSchemaDefinition schema) {
            schema.DeclaredFilters.Merge(app.AppFilters);
            schema.RelatedCompositions = BuildRelatedCompositionsList(schema);
            schema.NewSchemaRepresentation = LocateNewSchema(schema.ApplicationName);
        }

        public static IEnumerable<AssociationOption> BuildRelatedCompositionsList(ApplicationSchemaDefinition schema) {
            var relatedDetail = LocateRelatedDetailSchema(schema);
            if (relatedDetail == null) {
                return null;
            }

            var toExcludeSet = new HashSet<string>();
            var toExclude = schema.GetProperty(ApplicationSchemaPropertiesCatalog.ListQuickSearchCompositionsToExclude);
            if (toExclude != null) {
                toExcludeSet.AddAll(toExclude.Split(',').Select(EntityUtil.GetRelationshipName));
            }

            return relatedDetail.Compositions()
                .Where(c => !c.Inline && !toExcludeSet.Contains(c.Relationship) && c.HasAtLeastOneVisibleFieldForSearch())
                .Select(c => new AssociationOption(c.Relationship, c.Label));
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
                    app.AddSchema(ApplicationMetadataSchemaKey.GetSyncInstance(),
                        ApplicationSchemaFactory.GetSyncInstance(entityName, app.ApplicationName, app.IdFieldName,
                            app.UserIdFieldName));
                }
                foreach (var webSchema in app.Schemas()) {
                    var schema = webSchema.Value;

                    schema.DepandantFields(DependencyBuilder.BuildDependantFields(schema.Fields, schema.DependableFields));
                    schema._fieldWhichHaveDeps = schema.DependantFields().Keys;
                    if (schema.Stereotype.Equals(SchemaStereotype.List) || schema.Stereotype.Equals(SchemaStereotype.CompositionList)) {
                        ApplyListSpecificLogic(app, schema);
                    }

                    var instance = SlicedEntityMetadataBuilder.GetInstance(entityMetadata, schema, app.FetchLimit);
                    SlicedEntityMetadataCache[new SlicedEntityMetadataKey(webSchema.Key, entityName, app.ApplicationName)] = instance;

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
        /// <param name="throwException">If no mathcing application is found, the method throws <see cref="InvalidOperationException"/> if true; Default is false.</param>
        /// <param name="tryToLocateByEntity">The method tries to locate the application based on the name of its associated entities.</param>
        public static CompleteApplicationMetadataDefinition Application([NotNull] string name, bool throwException = true, bool tryToLocateByEntity = false) {
            Validate.NotNull(name, "name");

            IEnumerable<CompleteApplicationMetadataDefinition> apps = name.StartsWith("_") ? _swdbapplicationMetadata : _applicationMetadata;
            if (!FinishedParsing) {
                //if we are parsing schemas from an application, let큦 retrieve the app from this map instead
                apps = TransientApplicationMetadataDefinitions;
            }

            var application = apps.FirstOrDefault(
                        a => String.Equals(a.ApplicationName, name, StringComparison.CurrentCultureIgnoreCase));

            if (tryToLocateByEntity && application == null) {
                application = apps.FirstOrDefault(a => a.Entity.EqualsIc(name));
            }

            if (throwException && application == null) {
                throw ExceptionUtil.InvalidOperation("application {0} not found", name);
            }

            return application;
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


        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchTopLevelApps(ClientPlatform? platform, [CanBeNull]ISWUser user) {
            var watch = Stopwatch.StartNew();
            var result = new HashSet<CompleteApplicationMetadataDefinition>();
            var leafs = new List<MenuBaseDefinition>();
            if (platform == null) {
                leafs.AddRange(Menu(ClientPlatform.Web).ExplodedLeafs);
                var mobileMenu = Menu(ClientPlatform.Mobile);
                if (mobileMenu != null) {
                    leafs.AddRange(mobileMenu.ExplodedLeafs);
                }
            } else {
                leafs.AddRange(Menu(platform.Value).ExplodedLeafs);
            }


            foreach (var menuBaseDefinition in leafs) {
                if (menuBaseDefinition is ApplicationMenuItemDefinition) {
                    var applicationMenu = (menuBaseDefinition as ApplicationMenuItemDefinition);
                    var application = Application(applicationMenu.Application, false);

                    if (application != null && (user == null || user.IsAllowedInApp(application.ApplicationName))) {
                        if (applicationMenu.PermissionExpresion != null) {
                            if (!GenericSwMethodInvoker.Invoke<bool>(null, applicationMenu.PermissionExpresion)) {
                                //filtering applications based on permission expressions
                                continue;
                            }
                        }
                        result.Add(application);
                    }
                }
            }

            Log.DebugFormat("fetching top level apps took: {0} ", LoggingUtil.MsDelta(watch));
            //TODO: add hidden menu items
            return result;
        }

        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchSecuredTopLevelApps(ClientPlatform platform, InMemoryUser user) {
            return FetchTopLevelApps(platform, user).Select(metadata => metadata.CloneSecuring(user));
        }

        /// <summary>
        /// Return all schemas that are considered non internal for a given application. A schema can be considered non internal if it큦 either referenced on the menu, or if it holds the 
        /// <see cref="ApplicationSchemaPropertiesCatalog.NonInternalSchema" /> property
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        /// 
        /// 
        [NotNull]
        public static IEnumerable<ApplicationSchemaDefinition> FetchNonInternalSchemas(ClientPlatform platform, string applicationName) {
            var application = Application(applicationName);

            if (application.CachedNonInternalSchemas != null) {
                return application.CachedNonInternalSchemas;
            }

            System.Collections.Generic.ISet<string> schemaSet = new HashSet<string>();

            var menu = Menu(platform);
            var leafs = menu.ExplodedLeafs;
            foreach (var menuBaseDefinition in leafs) {
                if (menuBaseDefinition is ApplicationMenuItemDefinition) {
                    var menuApplication = menuBaseDefinition as ApplicationMenuItemDefinition;
                    if (menuApplication.Application.EqualsIc(applicationName)) {
                        schemaSet.Add(menuApplication.Schema);
                    }
                }
            }

            var menuReacheableSchemas = application.SchemasList.Where(s => schemaSet.Contains(s.SchemaId));
            var internalReacheableSchemas = application.SchemasList.Where(s => "true" == s.GetProperty(ApplicationSchemaPropertiesCatalog.NonInternalSchema));

            var cachedNonInternalSchemas = new List<ApplicationSchemaDefinition>(menuReacheableSchemas);
            cachedNonInternalSchemas.AddRange(internalReacheableSchemas);




            var resultSchemas = new HashSet<ApplicationSchemaDefinition>(cachedNonInternalSchemas);


            var singleDetailSchema = application.SchemaByStereotype("detail");
            if (singleDetailSchema != null) {
                //if there큦 only one schema marked with detail stereotype it will be used automatically upon grid routing, so let큦 add it
                resultSchemas.Add(singleDetailSchema);
            }

            foreach (var schema in cachedNonInternalSchemas) {
                //adding also schemas referenced by list schemas via metadata properties
                if (!SchemaStereotype.List.Equals(schema.Stereotype) || (schema.StereotypeAttr != null && !schema.StereotypeAttr.StartsWith("list"))) {
                    continue;
                }
                if (schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.ListClickSchema)) {
                    var applicationSchemaDefinition = application.SchemasList.FirstOrDefault(s => s.SchemaId == schema.Properties[ApplicationSchemaPropertiesCatalog.ListClickSchema]);
                    if (applicationSchemaDefinition != null) {
                        resultSchemas.Add(applicationSchemaDefinition);
                    }
                }
                if (schema.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.RoutingNextSchemaId)) {
                    var applicationSchemaDefinition = application.SchemasList.FirstOrDefault(s => s.SchemaId == schema.Properties[ApplicationSchemaPropertiesCatalog.RoutingNextSchemaId]);
                    if (applicationSchemaDefinition != null) {
                        resultSchemas.Add(applicationSchemaDefinition);
                    }
                }
            }

            application.CachedNonInternalSchemas = resultSchemas;
            application.HasCreationSchema = resultSchemas.Any(s => (s.Stereotype.Equals(SchemaStereotype.DetailNew) || (s.StereotypeAttr != null && s.StereotypeAttr.StartsWith("detailnew")))
            || menuReacheableSchemas.Any(m => (m.Stereotype.Equals(SchemaStereotype.Detail) || (m.StereotypeAttr != null && m.StereotypeAttr.StartsWith("detail")))));

            return cachedNonInternalSchemas;

        }

        [NotNull]
        public static SlicedEntityMetadata SlicedEntityMetadata(ApplicationMetadata applicationMetadata) {
            return SlicedEntityMetadataCache[new SlicedEntityMetadataKey(applicationMetadata.Schema.GetSchemaKey(), applicationMetadata.Entity, applicationMetadata.Name)];
        }

        [NotNull]
        public static SlicedEntityMetadata SlicedEntityMetadata(ApplicationSchemaDefinition applicationSchemaDefinition) {
            return SlicedEntityMetadataCache[new SlicedEntityMetadataKey(applicationSchemaDefinition.GetSchemaKey(), applicationSchemaDefinition.EntityName, applicationSchemaDefinition.Name)];
        }

        [NotNull]
        public StreamReader GetStream(string resource) {
            return MetadataParsingUtils.GetStreamImpl(resource);
        }

        [NotNull]
        public StreamReader GetTemplateStream(string path) {
            return MetadataParsingUtils.DoGetStream(path, true);
        }

        public void Save([NotNull] string data, bool internalFramework = false, string path = null) {
            try {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                    Save(memoryStream, internalFramework, path);
                }
            } catch (Exception e) {
                Log.Error("error saving metadata", e);
                throw;
            } finally {
            }
        }

        public void Save([NotNull] Stream data, bool internalFramework = false, string path = null) {
            try {
                _metadataXmlInitializer = new MetadataXmlSourceInitializer();
                _metadataXmlInitializer.Validate(_commandBars, data);
                _swdbmetadataXmlInitializer = new SWDBMetadataXmlSourceInitializer();
                _swdbmetadataXmlInitializer.Validate(_commandBars);

                var metadataPath = string.IsNullOrWhiteSpace(path) ? MetadataParsingUtils.GetPath(METADATA_FILE, internalFramework) : path;

                using (var stream = File.Create(metadataPath)) {
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

        /// <summary>
        /// Save the changes to the properties.xml file.
        /// This method also created a backup of the old file.
        /// </summary>
        /// <param name="fileData">The new file data</param>
        /// <param name="internalFramework">is internal framework</param>
        public void SavePropertiesFile([NotNull] string fileData, bool internalFramework = false) {
            var filePath = MetadataParsingUtils.GetPath(MetadataProvider.PROPERTIES_FILE);

            try {
                using (var data = new MemoryStream(Encoding.UTF8.GetBytes(fileData))) {
                    // Create a backup of the old file. 
                    using (var backupFile = File.Create(string.Format("{0}.orig", filePath))) {
                        using (var currentFile = File.OpenRead(filePath)) {
                            currentFile.CopyTo(backupFile);
                            backupFile.Flush();
                        }
                    }

                    // Write the new data to the file.
                    using (var stream = File.Create(filePath)) {
                        data.CopyTo(stream);
                        stream.Flush();
                    }
                }

                // Check if everything is OK. 
                DoInit();

                var conf = new ApplicationConfigurationAdapter();
                new SWDBHibernateDAO(conf, new HibernateUtil(conf));
                new MaximoHibernateDAO(conf, new HibernateUtil(conf));
            } catch (Exception e) {
                // restore the backup in case things go bad.
                using (var file = File.Create(filePath)) {
                    using (var origFile = File.OpenRead(string.Format("{0}.orig", filePath))) {
                        origFile.CopyTo(file);
                        file.Flush();
                    }
                }

                DoInit();
                Log.Error("error saving properties file! roperty file may be invalid.", e);
                throw new Exception(string.Format("Message: {0} , Inner exception: {1}", e.Message, e.InnerException != null ? e.InnerException.Message : "None"));
            } finally {
            }
        }

        public void SaveColor([NotNull] string data, string filePath, bool internalFramework = false) {
            try {
                using (var stream = File.Create(filePath)) {
                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                        memoryStream.CopyTo(stream);
                    }

                    stream.Flush();
                }
            } catch (Exception e) {
                Log.Error("error saving statuscolor", e);
                throw;
            }
        }

        public void SaveMenu([NotNull] string data, ClientPlatform platform = ClientPlatform.Web) {
            try {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                    var newMenu = new MenuXmlInitializer().InitializeMenu(platform, memoryStream);
                    using (var stream = File.Create(MenuXmlInitializer.GetMenuPath(platform))) {
                        memoryStream.CopyTo(stream);
                        stream.Flush();
                    }

                    _menus[platform] = newMenu;
                }
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

        public static IDictionary<string, CommandBarDefinition> CommandBars(ClientPlatform? platform = null, bool includeNulls = true) {
            return platform == null ? _commandBars : _commandBars.Where(c => c.Value.Platform == platform || (includeNulls && c.Value.Platform == null)).ToDictionary(kv => kv.Key, kv => kv.Value);
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

        public static System.Collections.Generic.ISet<string> FetchAvailableAppsAndEntities(bool includeSWDB = true) {
            if (_appsAndEntitiesUsedCache != null) {
                return _appsAndEntitiesUsedCache;
            }

            var result = new HashSet<string>();

            var applications = MetadataProvider.Applications(true);

            var completeApplicationMetadataDefinitions = applications as IList<CompleteApplicationMetadataDefinition> ?? applications.ToList();
            foreach (var application in completeApplicationMetadataDefinitions) {
                if (application.ApplicationName.StartsWith("_") && !includeSWDB) {
                    continue;
                }

                result.Add(application.ApplicationName);
                foreach (var schema in application.Schemas()) {
                    foreach (var association in schema.Value.Associations()) {
                        var entityName = association.EntityAssociation.To;
                        var associationApplication =
                            completeApplicationMetadataDefinitions.FirstOrDefault(a => a.Entity == entityName);
                        var toAdd = associationApplication == null ? association.EntityAssociation.To : associationApplication.ApplicationName;
                        result.Add(toAdd);
                    }
                    if (schema.Value.Properties.ContainsKey(ApplicationSchemaPropertiesCatalog.SchemaRelatedEntities)) {
                        var props = schema.Value.Properties[ApplicationSchemaPropertiesCatalog.SchemaRelatedEntities];
                        var relatedEntities = props.Split(',').Select(s => s.ToLower());
                        result.AddAll(relatedEntities);
                    }


                }
            }
            return result;
        }


        public static bool IsApplicationEnabled(string application) {
            return Application(application, false) != null;
        }

        [CanBeNull]
        public static string RoleByApplication(string applicationName) {
            var application = Application(applicationName);
            if (application == null) {
                return applicationName;
            }
            return application.Role;

        }

        /// <summary>
        /// Gets the entity metadata for an application.
        /// </summary>
        /// <param name="applicationName">The application name</param>
        /// <returns>The <see cref="EntityMetadata"/> object</returns>
        public static EntityMetadata EntityByApplication(string applicationName) {
            var application = Application(applicationName);
            return Entity(application.Entity);
        }

        /// <summary>
        /// Gets the sliced entity metadata for an application.
        /// </summary>
        /// <param name="applicationName">The application name</param>
        /// <param name="stereotypeToFilter">A stereotype to lookup, filtering list of results</param>
        /// <returns>A collection of <see cref="EntityMetadata"/> objects.</returns>
        public static List<EntityMetadata> SlicedEntityByApplication(string applicationName, string stereotypeToFilter = null) {
            var entityMetaDatas = new List<EntityMetadata>();

            var application = Application(applicationName, false, true);
            if (stereotypeToFilter == null) {
                //all of them
                foreach (var schema in application.SchemasList) {
                    entityMetaDatas.Add(SlicedEntityMetadata(schema));
                }
            } else {
                //just the ones which have the same stereotype
                entityMetaDatas.AddRange(application.AllSchemasByStereotype(stereotypeToFilter).Select(SlicedEntityMetadata));
            }

            return entityMetaDatas;
        }

        [CanBeNull]
        public static ApplicationSchemaDefinition Schema(string application, string schema, ClientPlatform platform) {
            var app = MetadataProvider.Application(application);
            if (app == null) {
                return null;
            }
            return app.Schema(new ApplicationMetadataSchemaKey(schema, SchemaMode.None, platform));
        }

        [CanBeNull]
        public static ApplicationSchemaDefinition LocateRelatedDetailSchema([NotNull]ApplicationSchemaDefinition listSchema) {
            var clickSchemaProperty = listSchema.GetProperty(ApplicationSchemaPropertiesCatalog.ListClickSchema);
            var application = Application(listSchema.ApplicationName);
            if (clickSchemaProperty != null) {
                return application.SchemasList.FirstOrDefault(s => s.SchemaId.EqualsIc(clickSchemaProperty));
            }

            //if there큦 only one schema marked with detail stereotype it will be used automatically upon grid routing, so let큦 add it
            return application.SchemaByStereotype("detail");
        }

        [CanBeNull]
        public static SchemaRepresentation LocateNewSchema([NotNull]string applicationName) {

            var application = Application(applicationName);
            //if there큦 only one schema marked with detail stereotype it will be used automatically upon grid routing, so let큦 add it
            var newSchema = application.SchemaByStereotype("detailnew");
            if (newSchema == null) {
                return null;
            }
            if (newSchema.MenuTitle != null) {
                return new SchemaRepresentation() {
                    Label = newSchema.MenuTitle,
                    SchemaId = newSchema.SchemaId
                };
            }

            var menu = Menu(ClientPlatform.Web);
            foreach (var leaf in menu.ExplodedLeafs) {
                if (leaf is ApplicationMenuItemDefinition) {
                    ApplicationMenuItemDefinition menuItem = (ApplicationMenuItemDefinition)leaf;
                    if (menuItem.Schema.EqualsIc(newSchema.SchemaId) &&
                        menuItem.Application.EqualsIc(newSchema.ApplicationName)) {
                        newSchema.MenuTitle = leaf.Title;
                        return new SchemaRepresentation() {
                            Label = newSchema.MenuTitle,
                            SchemaId = newSchema.SchemaId
                        };
                    }
                }
            }
            return new SchemaRepresentation() {
                Label = newSchema.Title,
                SchemaId = newSchema.SchemaId
            };

        }

        public static void AddComponents(string name, List<DisplayableComponent> appComponents) {
            if (!ComponentsDictionary.ContainsKey(name)) {
                ComponentsDictionary.Add(name, new HashSet<DisplayableComponent>(appComponents));
            } else {
                ComponentsDictionary[name].AddAll(appComponents);
            }
        }

        public static void AddTransientApplication(CompleteApplicationMetadataDefinition completeApplicationMetadataDefinition) {
            TransientApplicationMetadataDefinitions.Add(completeApplicationMetadataDefinition);
        }
    }
}