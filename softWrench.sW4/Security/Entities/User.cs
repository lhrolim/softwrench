using System;
using System.Linq;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using Newtonsoft.Json.Linq;
using softWrench.sW4.AUTH;
using softWrench.sW4.Security.Interfaces;


namespace softWrench.sW4.Security.Entities {
    [Class(Table = "SW_USER2", Lazy = false)]
    public class User : IBaseEntity {
        private string _userName;

        public const string UserByUserName = "from User where lower(userName) = lower(?)";
        public const string UserByMaximoPersonIds = "from User where MaximoPersonId in (:p0)";
        public const string UserByMaximoPersonId = "from User where lower(MaximoPersonId) = lower(?)";

        //public static string UserByMaximoPersonIdsWhereIn(List<string> maximoPersonIdsList) {
        //    const string where = "from User where MaximoPersonId in";
        //    var maximoPersonIds = string.Empty;
        //    var i = 1;
        //    foreach (var maximoPersonId in maximoPersonIdsList) {
        //        if (i == maximoPersonIdsList.Count) {
        //            maximoPersonIds += "'" + maximoPersonId + "'";
        //        }
        //        else {
        //            maximoPersonIds += "'" + maximoPersonId + "'" + ",";
        //        }
        //        i++;
        //    }
        //    var query = where + " (" + maximoPersonIds + ")";
        //    return query;
        //}

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

        private string _language;

        [Property]
        public virtual string Password { get; set; }

        [Property]
        public virtual string FirstName { get; set; }
        [Property]
        public virtual string LastName { get; set; }

        [Property]
        public virtual string Email { get; set; }

        [Property]
        public virtual string SiteId { get; set; }
        [Property]
        public virtual string OrgId { get; set; }
        [Property]
        public virtual Boolean IsActive { get; set; }

        [Property]
        public virtual string Department { get; set; }

        [Property]
        public virtual string Phone { get; set; }

        [Property]
        public virtual string Language {
            get { return _language == null ? null : _language.Trim().ToUpper(); }
            set { _language = value; }
        }

        [Property]
        public virtual string CriptoProperties { get; set; }

        [Property]
        public virtual string MaximoPersonId { get; set; }

        [JsonIgnore]
        [Set(0, Table = "SEC_PERSONGROUPASSOCIATION",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(PersonGroupAssociation))]
        public virtual Iesi.Collections.Generic.ISet<PersonGroupAssociation> PersonGroups { get; set; }

        [Set(0, Table = "sw_user_userprofile",
        Lazy = CollectionLazy.False, Cascade = "all")]
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

        public string DisplayName {
            get { return FirstName + " " + LastName; }
        }


        public User() {

        }
        /// <summary>
        /// used for nhibernate to generate a "view" of user entity to list screen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="isActive"></param>
        public User(int? id, string userName, string firstName, string lastName, bool isActive) {
            Id = id;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            IsActive = isActive;
            CustomConstraints = new HashedSet<UserCustomConstraint>();
            CustomRoles = new HashedSet<UserCustomRole>();
            Profiles = new HashedSet<UserProfile>();
        }

        public User(string userName, string firstName, string lastName, string siteId, string orgId, string department, string phone, string language, string password) {
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            SiteId = siteId;
            OrgId = orgId;
            IsActive = true;
            Department = department;
            Phone = phone;
            Language = language;
            Password = !string.IsNullOrEmpty(password) ? AuthUtils.GetSha1HashData(password) : null;
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
            LastName = UpdateIfNotNull(LastName, newUser.LastName);
            FirstName = UpdateIfNotNull(FirstName, newUser.FirstName);
            Department = UpdateIfNotNull(Department, newUser.Department);
            Language = UpdateIfNotNull(Language, newUser.Language);
            Phone = UpdateIfNotNull(Phone, newUser.Phone);
            Email = UpdateIfNotNull(Email, newUser.Email);
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

            //fixme: find a better solution, maybe 2 jsons from client
            user.UserName = (String)jObject["userName"];
            user.FirstName = (String)jObject["firstName"];
            user.LastName = (String)jObject["lastName"];
            user.OrgId = (String)jObject["orgId"];
            user.SiteId = (String)jObject["siteId"];
            user.IsActive = (bool)jObject["isActive"];
            user.Id = (int?)jObject["id"];
            user.Email = (String)jObject["email"];
            user.Department = (String)jObject["department"];
            user.Phone = (String)jObject["phone"];
            user.Language = (String)jObject["language"];
            var inputPassword = (String)jObject["password"];
            if (inputPassword != null) {
                user.Password = AuthUtils.GetSha1HashData(inputPassword);
            }
            user.MaximoPersonId = (String)jObject["maximopersonid"];

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
