using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Security.Entities {

    [Class(Table = "SEC_PERSONGROUPASSOCIATION", Lazy = false)]
    public class PersonGroupAssociation {

        public const string PersonGroupAssociationByUserId = "from PersonGroupAssociation where User.Id =?";

        public const string UserMatchingGroups = "select distinct(p.User) from PersonGroupAssociation p where p.PersonGroup.Name like ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        //        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        //        public virtual User User{ get; set; }

        [ManyToOne(Column = "persongroup_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual PersonGroup PersonGroup { get; set; }

        [Property]
        public virtual Boolean Delegate { get; set; }

        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual User User { get; set; }


        protected bool Equals(PersonGroupAssociation other) {
            return Equals(PersonGroup, other.PersonGroup) && Equals(User, other.User);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersonGroupAssociation)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((PersonGroup != null ? PersonGroup.GetHashCode() : 0) * 397) ^ (User != null ? User.GetHashCode() : 0);
            }
        }

        public override string ToString() {
            return string.Format("PersonGroup: {0}, User: {1}", PersonGroup, User);
        }

        public String GroupName {
            get { return PersonGroup.Name; }
        }
    }
}
