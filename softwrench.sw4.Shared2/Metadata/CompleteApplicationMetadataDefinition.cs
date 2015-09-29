using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Notification;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Notification;

namespace softwrench.sW4.Shared2.Metadata {

    public class CompleteApplicationMetadataDefinition : BaseDefinition, IApplicationIdentifier {
        public CompleteApplicationMetadataDefinition() { }

        public Guid? Id { get; set; }
        public string ApplicationName { get; set; }
        public string Entity { get; set; }
        public string Title { get; set; }
        public string IdFieldName { get; set; }
        public bool AuditFlag { get; set; }

        public string UserIdFieldName { get; set; }

        public string Service { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public int? FetchLimit { get; set; }
        //        public ToStringExpression _toStringExpression;
        //        public MobileApplicationSchema _mobileSchema;
        private readonly IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> _schemas = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();

        private readonly IEnumerable<ApplicationSchemaDefinition> _schemasList = new List<ApplicationSchemaDefinition>();

        public IEnumerable<DisplayableComponent> DisplayableComponents = new List<DisplayableComponent>();

        [JsonIgnore]
        public IDictionary<ApplicationNotificationKey, ApplicationNotificationDefinition> Notifications { get; set; }


        public CompleteApplicationMetadataDefinition(Guid? id, string applicationName, string title, string entity,
             string idFieldName, string userIdFieldName,
            IDictionary<string, string> paramters,
            IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> schemas,
            IEnumerable<DisplayableComponent> components,
            string service,
            IDictionary<ApplicationNotificationKey, ApplicationNotificationDefinition> notifications,
            bool auditFlag = false
            ) {
            if (applicationName == null) throw new ArgumentNullException("name");
            if (title == null) throw new ArgumentNullException("title");
            if (entity == null) throw new ArgumentNullException("entity");
            if (idFieldName == null) throw new ArgumentNullException("idFieldName");

            Id = id;
            ApplicationName = applicationName;
            Title = title;
            Entity = entity;
            IdFieldName = idFieldName;
            UserIdFieldName = userIdFieldName;
            Service = service;
            Parameters = paramters;
            foreach (ApplicationSchemaDefinition schema in schemas.Values) {
                schema.IdFieldName = IdFieldName;
                MergeSchemaPropertiesWithApplicationProperties(schema, Parameters);
            }
            _schemas = schemas;
            if (paramters.ContainsKey(ApplicationMetadataConstants.FetchLimitProperty)) {
                FetchLimit = int.Parse(paramters[ApplicationMetadataConstants.FetchLimitProperty].ToString());
            }
            _schemasList = _schemas.Values;
            DisplayableComponents = components;
            Notifications = notifications;
            AuditFlag = auditFlag;
            //            _mobileSchema = BuildMobileSchema();
        }

        private void MergeSchemaPropertiesWithApplicationProperties(ApplicationSchemaDefinition schema, IDictionary<string, string> parameters) {
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


        //Method to avoid serialization
        public virtual IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> Schemas() {
            return _schemas;
        }

        public IEnumerable<ApplicationSchemaDefinition> SchemasList {
            get { return _schemasList; }
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



        public string Role {
            get { return ApplicationName; }
        }

        protected bool Equals(CompleteApplicationMetadataDefinition other) {
            return string.Equals(ApplicationName, other.ApplicationName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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


    }
}