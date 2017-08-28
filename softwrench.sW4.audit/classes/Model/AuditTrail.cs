using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.Attributes;

namespace softwrench.sW4.audit.classes.Model {

    [Class(Table = "AUDI_TRAIL", Lazy = false)]
    public class AuditTrail {

        public AuditTrail() {

        }

        public AuditTrail(string name, string operation, int? sessionId) {
            Name = name;
            Operation = operation;
            BeginTime = DateTime.Now;
            Session = new AuditSession(sessionId);
        }


        public string ByExternalId = "from AuditTrail where ExternalId = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        /// <summary>
        /// The name of the application/module that dispatched the transaction
        /// </summary>
        [Property]
        public virtual string Name { get; set; }

        [Property]
        public virtual DateTime BeginTime { get; set; }

        [Property]
        public virtual DateTime EndTime { get; set; }


        /// <summary>
        /// an operation such as creation/update/sync, or a custom entry
        /// </summary>
        [Property]
        public virtual string Operation { get; set; }

        /// <summary>
        /// An external global identifier used to fetch this entry. Ex: upon an offline synchronization, 
        /// the id would be generated at the client side in order to allow the multiple threads to keep track of it.
        /// </summary>
        [Property]
        public virtual string ExternalId { get; set; }

        //        [Property]
        //        public virtual int? SessionId { get; set; }

        [ManyToOne(Column = "sessionId", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public AuditSession Session { get; set; }

        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "trail_id")]
        [OneToMany(2, ClassType = typeof(AuditEntry))]
        public ISet<AuditEntry> Entries { get; set; } = new HashSet<AuditEntry>();



        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "trail_id", NotNull = true)]
        [OneToMany(2, ClassType = typeof(AuditQuery))]
        public ISet<AuditQuery> Queries { get; set; } = new HashSet<AuditQuery>();


        private bool _shouldPersist;


        public virtual bool ShouldPersist {
            get { return _shouldPersist || (Entries.Any() || Queries.Any()); }
            set { _shouldPersist = value; }
        }


//        public virtual bool ShouldPersist {
//            get => _shouldPersist || (Entries.Any() || Queries.Any());
//            set => _shouldPersist = value;
//        }

    }
}


