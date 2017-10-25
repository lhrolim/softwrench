using System;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "SW_ROLE")]
    public class Role {
        public const string RoleByName = "from Role where Name =?";
        public static string RoleByNames = "from Role where Name in (:p0)";


        public const string SysAdmin = "sysadmin";
        public const string ClientAdmin = "clientadmin";
        public const string SysJob = "sysjob";
        public const string ClientJob = "clientjob";
        public const string DynamicAdmin = "dynamicadmin";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Name { get; set; }

        [Property]
        public virtual string Label { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property(Column = "deletable", TypeType = typeof(BooleanToIntUserType))]
        public virtual bool Deletable { get; set; }

        [ManyToOne(Column = "rolegroup_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual RoleGroup RoleGroup { get; set; }


        [Property(Column = "isactive", TypeType = typeof(BooleanToIntUserType))]
        public virtual bool Active { get; set; }

        protected bool Equals(Role other) {
            return Name.EqualsIc(other.Name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((Role)obj);
        }

        public override int GetHashCode() {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString() {
            return string.Format("Name: {0}", Name);
        }
    }
}
