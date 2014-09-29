using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared.Metadata.Applications;
using softwrench.sW4.Shared.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared.Metadata {

    public class CompleteApplicationMetadataDefinition : IDefinition {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Entity { get; set; }
        public string Title { get; set; }
        public string IdFieldName { get; set; }
        public int? FetchLimit { get; set; }
        //        public ToStringExpression _toStringExpression;
        //        public MobileApplicationSchema _mobileSchema;
        private readonly IDictionary<string, object> _parameters;
        private readonly IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> _schemas = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();
        public IDictionary<string, object> ExtensionParameters { get; set; }

        public CompleteApplicationMetadataDefinition(Guid? id, string name, string title, string entity,
             string idFieldName,
            IDictionary<string, object> paramters,
            IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> schemas) {
            if (name == null) throw new ArgumentNullException("name");
            if (title == null) throw new ArgumentNullException("title");
            if (entity == null) throw new ArgumentNullException("entity");
            if (idFieldName == null) throw new ArgumentNullException("idFieldName");

            Id = id;
            Name = name;
            Title = title;
            Entity = entity;
            IdFieldName = idFieldName;
            foreach (ApplicationSchemaDefinition schema in schemas.Values) {
                schema.IdFieldName = IdFieldName;
            }
            _schemas = schemas;
            //            _mobileSchema = mobileSchema;
            _parameters = paramters;
            if (paramters.ContainsKey(ApplicationMetadataConstants.FetchLimitProperty)) {
                FetchLimit = int.Parse(paramters[ApplicationMetadataConstants.FetchLimitProperty].ToString());
            }
            //            _mobileSchema = BuildMobileSchema();
        }



        public virtual IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> Schemas {
            get { return _schemas; }
        }

        public IDictionary<string, object> Parameters {
            get { return _parameters; }
        }


        public ApplicationSchemaDefinition Schema(ApplicationMetadataSchemaKey key, bool throwException = false) {
            if (Schemas.ContainsKey(key)) {
                return Schemas[key];
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
            get { return Name; }
        }

        public override string ToString() {
            return string.Format("Name: {0}", Name);
        }
    }
}