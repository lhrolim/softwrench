﻿using System;
using System.Linq;
using cts.commons.persistence.Util;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using NHibernate.Linq.Visitors;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.user.classes.entities.security;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "SW_USERPROFILE")]
    public class UserProfile {
        public const string UserProfileByName = "from UserProfile where Name =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id {
            get; set;
        }

        [Property]
        public virtual string Name {
            get; set;
        }

        [Property(Column = "deletable", TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean Deletable {
            get; set;
        }

        [Property(Column = "description")]
        public virtual string Description {
            get; set;
        }

        [Set(0, Table = "sw_userprofile_role",
        Lazy = CollectionLazy.False)]
        [Key(1, Column = "profile_id")]
        [ManyToMany(2, Column = "role_id", ClassType = typeof(Role))]
        public virtual ISet<Role> Roles {
            get; set;
        }


        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "profile_id")]
        [OneToMany(2, ClassType = typeof(ApplicationPermission))]
        public virtual ISet<ApplicationPermission> ApplicationPermission {
            get; set;
        }




        //
        //        [Set(0, Table = "sw_userprofile_dataconstraint",
        //        Lazy = CollectionLazy.False, Cascade = "all")]
        //        [Key(1, Column = "profile_id")]
        //        [ManyToMany(2, ClassType = typeof(DataConstraint), Column = "constraint_id")]
        //        public virtual ISet<DataConstraint> DataConstraints { get; set; }

        protected bool Equals(UserProfile other) {
            return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserProfile)obj);
        }

        public override int GetHashCode() {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static UserProfile FromJson(JToken jObject) {
            var profile = new UserProfile();
            profile.Roles = new HashedSet<Role>();

            JToken roles = jObject["#basicroles_"];
            if (roles != null) {
                foreach (var jToken in roles.ToArray()) {
                    if ((bool)jToken["_#selected"]) {
                        profile.Roles.Add(new Role {
                            Id = (int?)jToken["id"],
                            Name = (string)jToken["name"]
                        });
                    }
                }
            }
            profile.Name = (string)jObject["name"];
            profile.Description = (string)jObject["description"];
            profile.Id = (int?)jObject["id"];
            return profile;
        }

        public static UserProfile TestInstance(int? id, string name) {
            return new UserProfile { Id = id, Name = name };
        }

        public virtual UserProfileDTO ToDTO() {
            if (Id == null) {
                return null;
            }

            return new UserProfileDTO(Id.Value, Name);

        }

        /// <summary>
        /// Lightweight implementation to return to client side to avoid any performance implications
        /// </summary>
        public class UserProfileDTO {

            public UserProfileDTO(int id, string name) {
                Id = id;
                Name = name;

            }

            public int Id {
                get; set;
            }
            public string Name {
                get; set;
            }
        }
    }
}
