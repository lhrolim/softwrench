using System;
using System.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Entities {
    [Class(Table = "MAX_COMMREADFLAG", Lazy = false)]
    public class MaxCommReadFlag {
        
        public const String ByItemIdAndUserId = "from MaxCommReadFlag where Application =? and ApplicationItemId =? and UserId =?";

        public const String ByItemIdAndUserIdAndCommlogId = "from MaxCommReadFlag where Application =? and ApplicationItemId =? and UserId =? and CommlogId =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Application { get; set; }

        [Property]
        public virtual string ApplicationItemId { get; set; }

        [Property]
        public virtual int UserId { get; set; }

        [Property]
        public virtual int CommlogId { get; set; }

        [Property]
        public virtual bool ReadFlag { get; set; }

        public override string ToString()
        {
            return string.Format("Application: {0}, ApplicationItemId: {1}, UserId: {2}, CommlogId: {3}, ReadFlag: {4}", Application, ApplicationItemId, UserId, CommlogId, ReadFlag);
        }

        private sealed class IdEqualityComparer : IEqualityComparer<MaxCommReadFlag> {
            public bool Equals(MaxCommReadFlag x, MaxCommReadFlag y) {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(MaxCommReadFlag obj) {
                return obj.Id.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<MaxCommReadFlag> IdComparerInstance = new IdEqualityComparer();

        public static IEqualityComparer<MaxCommReadFlag> IdComparer {
            get { return IdComparerInstance; }
        }

    }
}