using cts.commons.persistence;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.AUTH;
using softWrench.sW4.Util;
using System;
using System.Linq;


namespace softWrench.sW4.Security.Entities {
    [Class(Table = "SW_USER2", Lazy = false)]
    public class User : IBaseEntity {
        private string _userName;

        public const string UserByUserName = "from User where lower(userName) = lower(?)";
        public const string UserByMaximoPersonIds = "from User where MaximoPersonId in (:p0)";
        public const string UserByMaximoPersonId = "from User where lower(MaximoPersonId) = lower(?)";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string UserName {
            get {
                return _userName == null ? "" : _userName.ToLower();
            }
            set {
                _userName = value;
            }
        }

        private Person _person;

        public Person Person {
            get { return _person; }
            set { _person = value; }
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean IsActive { get; set; }

        [Property]
        public virtual string CriptoProperties { get; set; }

        [Property]
        public virtual string MaximoPersonId { get; set; }

        [Property]
        public virtual string Password { get; set; }

        [JsonIgnore]
        [Set(0, Table = "SEC_PERSONGROUPASSOCIATION",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(PersonGroupAssociation))]
        public virtual Iesi.Collections.Generic.ISet<PersonGroupAssociation> PersonGroups { get; set; }

        [Set(0, Table = "sw_user_userprofile",
        Lazy = CollectionLazy.False, Cascade = "none")]
        [Key(1, Column = "user_id")]
        [ManyToMany(2, Column = "profile_id", ClassType = typeof(UserProfile))]
        public virtual Iesi.Collections.Generic.ISet<UserProfile> Profiles { get; set; }

        [Set(0, Table = "sw_user_customrole",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(UserCustomRole))]
        public virtual Iesi.Collections.Generic.ISet<UserCustomRole> CustomRoles { get; set; }


        [Set(0, Table = "SW_USER_CUSTOMDATACONSTRAINT",
          Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(UserCustomConstraint))]
        public virtual Iesi.Collections.Generic.ISet<UserCustomConstraint> CustomConstraints { get; set; }

        //public string DisplayName {
        //    get { return Person.FirstName + " " + Person.LastName; }
        //}


        public User() {
            Person = new Person();
            Profiles = new HashedSet<UserProfile>();
        }
        /// <summary>
        /// used for nhibernate to generate a "view" of user entity to list screen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="isActive"></param>
        public User(int? id, string userName, bool isActive) {
            Id = id;
            UserName = userName;
            IsActive = isActive;
            CustomConstraints = new HashedSet<UserCustomConstraint>();
            CustomRoles = new HashedSet<UserCustomRole>();
            Profiles = new HashedSet<UserProfile>();
        }

        public User(string userName, string firstName, string lastName, string siteId, string orgId, string department, string phone, string language, string password, string storeloc) {
            UserName = userName;
            Person = new Person();
            Person.FirstName = firstName;
            Person.LastName = lastName;
            Person.SiteId = siteId;
            Person.OrgId = orgId;
            IsActive = true;
            Person.Department = department;
            Person.Phone = phone;
            Person.Language = language;
            Person.Password = !string.IsNullOrEmpty(password) ? AuthUtils.GetSha1HashData(password) : null;
            Person.Storeloc = storeloc;
        }

        public void MergeFromDBUser(User dbUSer) {
            //keep password unchanged
            Password = Password ?? dbUSer.Password;
            CriptoProperties = CriptoProperties ?? dbUSer.CriptoProperties;
            PersonGroups = PersonGroups ?? dbUSer.PersonGroups;
            CustomRoles = CustomRoles ?? dbUSer.CustomRoles;
            Profiles = Profiles ?? dbUSer.Profiles;
            MaximoPersonId = MaximoPersonId ?? dbUSer.MaximoPersonId;
        }

        public void MergeMaximoWithNewUser(User newUser) {
            Person.LastName = UpdateIfNotNull(Person.LastName, newUser.Person.LastName);
            Person.FirstName = UpdateIfNotNull(Person.FirstName, newUser.Person.FirstName);
            Person.Department = UpdateIfNotNull(Person.Department, newUser.Person.Department);
            Person.Language = UpdateIfNotNull(Person.Language, newUser.Person.Language);
            Person.Phone = UpdateIfNotNull(Person.Phone, newUser.Person.Phone);
            Person.Email = UpdateIfNotNull(Person.Email, newUser.Person.Email);
            Password = UpdateIfNotNull(Password, newUser.Password);
            MaximoPersonId = UpdateIfNotNull(MaximoPersonId, newUser.MaximoPersonId);
        }


        public string UpdateIfNotNull(string oldValue, string newValue) {
            return String.IsNullOrEmpty(newValue) ? oldValue : newValue;
        }




        public static User fromJson(JObject jObject) {
            var user = new User();
            user.CustomRoles = new HashedSet<UserCustomRole>();
            user.CustomConstraints = new HashedSet<UserCustomConstraint>();
            user.Profiles = new HashedSet<UserProfile>();
            JToken roles = jObject["customRoles"];
            if (roles != null) {
                foreach (JToken jToken in roles.ToArray()) {
                    user.CustomRoles.Add(jToken.ToObject<UserCustomRole>());
                }
            }
            JToken customConstraints = jObject["customConstraints"];
            if (customConstraints != null) {
                foreach (JToken jToken in customConstraints.ToArray()) {
                    user.CustomConstraints.Add(jToken.ToObject<UserCustomConstraint>());
                }
            }
            JToken profiles = jObject["profiles"];
            if (profiles != null) {
                foreach (JToken jToken in profiles.ToArray()) {
                    user.Profiles.Add(UserProfile.FromJson(jToken));
                }
            }
            JToken personGroups = jObject["personGroups"];
            if (personGroups != null) {
                foreach (JToken jToken in personGroups.ToArray()) {
                    if (user.PersonGroups == null) {
                        user.PersonGroups = new HashedSet<PersonGroupAssociation>();
                    }
                    user.PersonGroups.Add(jToken.ToObject<PersonGroupAssociation>());
                }
            }

            user.Id = (int?)jObject["id"];
            user.UserName = (String)jObject["username"];
            user.MaximoPersonId = (String)jObject["maximopersonid"];
            user.IsActive = jObject["isactive"] == null || (bool)jObject["isactive"];
            var inputPassword = (String)jObject["password"];
            if (inputPassword != null) {
                user.Password = AuthUtils.GetSha1HashData(inputPassword);
            }


            return user;
        }

        protected bool Equals(User other) {
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User)obj);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public class UserNameEqualityUser {
            public User user;

            public UserNameEqualityUser(User user) {
                this.user = user;
            }

            protected bool Equals(UserNameEqualityUser other) {
                return Equals(user.UserName, other.user.UserName);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UserNameEqualityUser)obj);
            }

            public override int GetHashCode() {
                return (user != null ? user.GetHashCode() : 0);
            }
        }
    }
}
