using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata {
    public class ApplicationMetadataDefinition {

        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Entity { get; set; }
        public string Title { get; set; }
        public string IdFieldName { get; set; }
        public virtual ApplicationSchemaDefinition Schema { get; set; }
        public string Service { get; set; }

        public ApplicationMetadataDefinition() { }

        public ApplicationMetadataDefinition(Guid? id, string name, string title, string entity,
             string idFieldName, ApplicationSchemaDefinition schema, string service) {
            if (name == null) throw new ArgumentNullException("name");
            if (title == null) throw new ArgumentNullException("title");
            if (entity == null) throw new ArgumentNullException("entity");
            if (idFieldName == null) throw new ArgumentNullException("idFieldName");
            if (schema == null) throw new ArgumentNullException("schema");
            if (service == null) {
                service = name.ToLower() + "_service";
            }

            Id = id;
            Name = name;
            Title = title;
            Entity = entity;
            IdFieldName = idFieldName;
            Schema = schema;
            Schema.IdFieldName = idFieldName;
            Service = service;
        }




        //        public bool IsUserInteractionEnabled {
        //            get {
        //                var mobileApplicationSchema = Schema as MobileApplicationSchema;
        //                return mobileApplicationSchema == null || mobileApplicationSchema.IsUserInteractionEnabled;
        //            }
        //        }
        //        public int? FetchLimit {
        //            get { return Schema is MobileApplicationSchema ? ((MobileApplicationSchema)Schema).FetchLimit : (int?)null; }
        //        }

        public string Role {
            get { return Name; }
        }



        public override string ToString() {
            return string.Format("Name: {0}, Entity: {1}, Schema: {2}", Name, Entity, Schema);
        }




    }
}