using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Data.EL;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications {
    public class ApplicationMetadata : ApplicationMetadataDefinition {



        public ApplicationMetadata(Guid? id, [NotNull] string name, [NotNull] string title, [NotNull] string entity,
            [NotNull] string idFieldName, [NotNull] ApplicationSchemaDefinition schema, string service, bool auditFlag)
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


    }
}