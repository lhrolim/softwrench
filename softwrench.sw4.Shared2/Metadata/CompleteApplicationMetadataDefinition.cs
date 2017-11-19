using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sW4.Shared2.Metadata {

    public class CompleteApplicationMetadataDefinition : BaseDefinition, IApplicationIdentifier, IPropertyHolder {
        public CompleteApplicationMetadataDefinition() {
        }

        public Guid? Id {
            get; set;
        }
        public string ApplicationName {
            get; set;
        }
        public string Entity {
            get; set;
        }
        public string Title {
            get; set;
        }
        public string IdFieldName {
            get; set;
        }
        public bool? AuditFlag {
            get; set;
        }

        public string UserIdFieldName {
            get; set;
        }

        public string Service {
            get; set;
        }

        private string _role;

        public string Role {
            get {
                return _role;
            }
            set {
                _role = value;
            }
        }

        public IDictionary<string, string> Parameters {
            get; set;
        }
        public int? FetchLimit {
            get; set;
        }
        //        public ToStringExpression _toStringExpression;
        //        public MobileApplicationSchema _mobileSchema;
        private IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> _schemas = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();

        private ISet<ApplicationSchemaDefinition> _schemasList = new HashSet<ApplicationSchemaDefinition>();

        public IEnumerable<DisplayableComponent> DisplayableComponents = new List<DisplayableComponent>();

        /// <summary>
        /// Override schema to be used on the url /{applicationname}/
        /// </summary>
        public ApplicationMetadataSchemaKey MainListSchemaKey { get; set; }

        /// <summary>
        ///  Override schema to be used on the url /{applicationname}/{userid} and /{applicationname}/uid/{id}
        /// </summary>
        public ApplicationMetadataSchemaKey MainDetailSchemaKey { get; set; }

        /// <summary>
        ///  Override schema to be used on the url /{applicationname}/new
        /// </summary>
        public ApplicationMetadataSchemaKey MainNewDetailSchemaKey { get; set; }

        public delegate ApplicationSchemaDefinition LazySchemaResolverDelegate(ApplicationMetadataSchemaKey key);

        [JsonIgnore]
        public static LazySchemaResolverDelegate LazySchemaResolver;

        public SchemaFilters AppFilters;


        public CompleteApplicationMetadataDefinition(Guid? id, string applicationName, string title, string entity,
             string idFieldName, string userIdFieldName,
            IDictionary<string, string> paramters,
            IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> schemas,
            IEnumerable<DisplayableComponent> components, SchemaFilters appFilters,
            string service,
            string role,
            bool? auditFlag = false) {
            if (applicationName == null)
                throw new ArgumentNullException(nameof(applicationName));
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (idFieldName == null)
                throw new ArgumentNullException(nameof(idFieldName));

            Id = id;
            ApplicationName = applicationName;
            Title = title;
            Entity = entity;
            IdFieldName = idFieldName;
            UserIdFieldName = userIdFieldName;
            Service = service;
            Parameters = paramters;
            AppFilters = appFilters ?? SchemaFilters.BlankInstance();
            if (paramters.ContainsKey(ApplicationMetadataConstants.FetchLimitProperty)) {
                FetchLimit = int.Parse(paramters[ApplicationMetadataConstants.FetchLimitProperty]);
            }
            SetSchemas(schemas);

            DisplayableComponents = components;
            Role = role ?? applicationName;
            AuditFlag = auditFlag;
            //            _mobileSchema = BuildMobileSchema();
        }

        public void MergeSchemaPropertiesWithApplicationProperties(ApplicationSchemaDefinition schema, IDictionary<string, string> parameters) {
            if (parameters == null || !parameters.Any()) {
                return;
            }
            foreach (var parameterKey in parameters.Keys) {
                //if the schema already contains a definition, keep it. this way just the non overriden properties would be set
                if (!schema.Properties.ContainsKey(parameterKey)) {
                    schema.Properties.Add(parameterKey, parameters[parameterKey]);
                } else {
                    schema.Properties[parameterKey] = parameters[parameterKey];
                }
            }
        }

        public void AddSchema(ApplicationMetadataSchemaKey key, ApplicationSchemaDefinition schema) {
            _schemas.Add(key, schema);
            _schemasList.Add(schema);
        }

        //Method to avoid serialization
        public virtual IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> Schemas() {
            return _schemas;
        }

        public IEnumerable<ApplicationSchemaDefinition> SchemasList {
            get {
                return _schemasList;
            }
        }

        public IEnumerable<ApplicationSchemaDefinition> MobileSchemas() {
            var resultSchemas = new Dictionary<string, ApplicationSchemaDefinition>();
            var mobileDeclaredSchemas = _schemasList.Where(s => s.IsMobilePlatform());
            foreach (var schema in mobileDeclaredSchemas) {
                //first add the one which are explicitely marked as mobile schemas
                resultSchemas.Add(schema.SchemaId, schema);
            }
            var nonWebSchemas = _schemasList.Where(s => !s.IsWebPlatform());
            foreach (var schema in nonWebSchemas) {
                //then, adding possible "none-declared" schemas, so that if the same is declared the mobile one stands
                if (!resultSchemas.ContainsKey(schema.SchemaId)) {
                    resultSchemas.Add(schema.SchemaId, schema);
                }
            }
            return resultSchemas.Values;
        }



        public ApplicationSchemaDefinition Schema(ApplicationMetadataSchemaKey key, bool throwException = false) {
            if (Schemas().ContainsKey(key)) {
                return Schemas()[key];
            }
            if (throwException) {
                throw key.NotFoundException();
            }
            return null;
        }

        //        public MobileApplicationSchema MobileSchema {
        //            get { return _mobileSchema; }
        //        }





        protected bool Equals(CompleteApplicationMetadataDefinition other) {
            return string.Equals(ApplicationName, other.ApplicationName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((CompleteApplicationMetadataDefinition)obj);
        }

        public override int GetHashCode() {
            return (ApplicationName != null ? ApplicationName.GetHashCode() : 0);
        }

        public override string ToString() {
            return string.Format("Name: {0}", ApplicationName);
        }

        public ApplicationSchemaDefinition GetListSchema() {
            var listSchema = SchemasList.FirstOrDefault(a => a.SchemaId.Equals("list"));
            if (listSchema != null) {
                return listSchema;
            }

            listSchema = SchemasList.FirstOrDefault(a => SchemaStereotype.List.Equals(a.Stereotype));
            if (listSchema != null) {
                return listSchema;
            }
            listSchema = SchemasList.FirstOrDefault(a => SchemaStereotype.CompositionList.Equals(a.Stereotype));
            return listSchema;
        }

        public ApplicationMetadata StaticFromSchema(string schemaId) {
            return ApplicationMetadata.FromSchema(Schema(new ApplicationMetadataSchemaKey(schemaId)), Title);
        }


        public IDictionary<string, string> Properties {
            get {
                return Parameters;
            }
            set {
                Parameters = value;
            }
        }

        public void SetSchemas([CanBeNull]IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> schemas) {
            if (schemas == null) return;

            _schemas = schemas;
            foreach (var schema in _schemas.Values) {
                schema.IdFieldName = IdFieldName;
                MergeSchemaPropertiesWithApplicationProperties(schema, Parameters);
            }
            _schemasList = new HashSet<ApplicationSchemaDefinition>(_schemas.Values);
        }

        public IEnumerable<ApplicationSchemaDefinition> CachedNonInternalSchemas {
            get; set;
        }
        public bool HasCreationSchema {
            get; set;
        }

        public string GetProperty(string propertyKey) {
            if (Properties == null || !Properties.ContainsKey(propertyKey)) {
                return null;
            }
            return Properties[propertyKey];
        }

        public bool IsPropertyTrue(string propertyKey) {
            if (Properties == null || !Properties.ContainsKey(propertyKey)) {
                return false;
            }
            return Properties[propertyKey].Equals("true");
        }


        public ApplicationSchemaDefinition MainListSchema => MainListSchemaKey != null ? LazySchemaResolver(MainListSchemaKey) : null;

        public ApplicationSchemaDefinition MainDetailSchema => MainDetailSchemaKey != null ? LazySchemaResolver(MainDetailSchemaKey) : null;


        /// <summary>
        ///  Override schema to be used on the url /{applicationname}/new
        /// </summary>
        public ApplicationSchemaDefinition MainNewDetailSchema => MainNewDetailSchemaKey != null ? LazySchemaResolver(MainNewDetailSchemaKey) : null;

    }
}
