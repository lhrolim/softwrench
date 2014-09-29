using System;
using System.Linq;
using Iesi.Collections.Generic;
using NHibernate.Mapping.Attributes;
using Newtonsoft.Json.Linq;

namespace softWrench.sW4.Security.Entities {
    [Class(Table = "SW_USERPROFILE")]
    public class UserProfile {
        public const string UserProfileByName = "from UserProfile where Name =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Name { get; set; }

        [Property(Column = "deletable")]
        public virtual Boolean Deletable { get; set; }

        [Property(Column = "description")]
        public virtual string Description { get; set; }

        [Set(0, Table = "sw_userprofile_role",
        Lazy = CollectionLazy.False)]
        [Key(1, Column = "profile_id")]
        [ManyToMany(2, Column = "role_id", ClassType = typeof(Role))]
        public virtual ISet<Role> Roles { get; set; }


        [Set(0, Table = "sw_userprofile_dataconstraint",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "profile_id")]
        [ManyToMany(2, ClassType = typeof(DataConstraint), Column = "constraint_id")]
        public virtual ISet<DataConstraint> DataConstraints { get; set; }

        protected bool Equals(UserProfile other) {
            return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserProfile)obj);
        }

        public override int GetHashCode() {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static UserProfile FromJson(JToken jObject) {
            var profile = new UserProfile();
            profile.Roles = new HashedSet<Role>();
            profile.DataConstraints = new HashedSet<DataConstraint>();

            JToken roles = jObject["roles"];
            if (roles != null) {
                foreach (var jToken in roles.ToArray()) {
                    profile.Roles.Add(new Role {
                        Id = (int?)jToken["id"],
                        Name = (string)jToken["name"],
                        Description = (string)jToken["description"],
                        Active = (bool)jToken["active"]
                    });
                }
            }
            JToken constraints = jObject["dataConstraints"];
            if (constraints != null) {
                foreach (var jToken in constraints.ToArray()) {
                    profile.DataConstraints.Add(new DataConstraint {
                        ID = (int?)jToken["id"],
                        EntityName = (string)jToken["entityName"],
                        WhereClause = (string)jToken["whereClause"],
                        Isactive = (bool)jToken["isactive"]
                    });
                }
            }
            profile.Name = (string)jObject["name"];
            profile.Description = (string)jObject["description"];
            profile.Id = (int?)jObject["id"];
            return profile;
        }
    }
}
