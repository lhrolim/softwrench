using System;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications {
    public class ApplicationMetadata : ApplicationMetadataDefinition {



        public ApplicationMetadata(Guid? id, string name, string title, string entity,
             string idFieldName, ApplicationSchemaDefinition schema, string service, bool? auditFlag)
            : base(id, name, title, entity, idFieldName, schema, service, auditFlag) {
        }

        //        public bool IsUserInteractionEnabled {
        //            get {
        //                var mobileApplicationSchema = _schema as MobileApplicationSchema;
        //                return mobileApplicationSchema == null || mobileApplicationSchema.IsUserInteractionEnabled;
        //            }
        //        }


        //        public int? FetchLimit {
        //            get { return Schema is MobileApplicationSchema ? ((MobileApplicationSchema)Schema).FetchLimit : (int?)null; }
        //        }


        public static ApplicationMetadata CloneSecuring(CompleteApplicationMetadataDefinition application, ApplicationSchemaDefinition securedSchema) {
            return new ApplicationMetadata(
                application.Id,
                application.ApplicationName,
                application.Title,
                application.Entity,
                application.IdFieldName,
                securedSchema,
                application.Service,
                application.AuditFlag
                );
        }


        public static ApplicationMetadata FromSchema(ApplicationSchemaDefinition schema) {
            //workaround adatper for methods that still require an ApplicationMetadata instance
            return new ApplicationMetadata(null, schema.ApplicationName, "", "", schema.IdFieldName, schema, null, null);
        }
    }
}