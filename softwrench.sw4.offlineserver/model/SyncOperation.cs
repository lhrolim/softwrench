using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.audit.classes.Model;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.offlineserver.model {

    [Class(Table = "OFF_SYNCOPERATION", Lazy = false)]
    public class SyncOperation : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [ManyToOne(Column = "trail_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "save-update")]
        public AuditTrail AuditTrail { get; set; }

        [Property]
        public string ServerVersion { get; set; }

        [Property]
        public string ServerEnv { get; set; }

        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public User User { get; set; }

        [Property(Column = "user_tzoffset")]
        public int? TimezoneOffset { get; set; }

        [Property(Column = "user_properties")]
        public string UserProperties { get; set; }

        [ComponentProperty]
        public DeviceData DeviceData { get; set; }

        [Property]
        public int? CompositionCounts { get; set; }

        [Property]
        public int? AssociationCounts { get; set; }

        [Property]
        public int? TopAppCounts { get; set; }

        [Property]
        public bool? MetadataDownload { get; set; }

        [Property]
        public bool? HasUploadOperation { get; set; }

        [Property]
        public DateTime? RegisterTime { get; set; }


        public static string GenerateKey(InMemoryUser user, string clientOperationId) {
            return user.DBId + clientOperationId;
        }

    }
}
