﻿using System;
using System.Linq;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.Util;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "SW_USER2", Lazy = false)]
    public sealed class User : IBaseEntity {

        private string _userName;

        public const string UserByUserName = "from User where lower(UserName) = lower(?)";
        public const string UserByMaximoPersonIds = "from User where MaximoPersonId in (:p0)";
        public const string UserByMaximoPersonId = "from User where lower(MaximoPersonId) = lower(?)";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string UserName {
            get {
                return _userName == null ? null : _userName.ToLower();
            }
            set {
                _userName = value;
            }
        }

        public Person Person {
            get; set;
        }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public Boolean IsActive {
            get; set;
        }

        [Property]
        public string CriptoProperties {
            get; set;
        }

        [Property]
        public string MaximoPersonId {
            get; set;
        }

        [Property]
        public string Password {
            get; set;
        }

        [Property]
        public string FirstName {
            get; set;
        }

        [Property]
        public string LastName {
            get; set;
        }

        [Property]
        public string Email {
            get; set;
        }


        [Property]
        public string SiteId {
            get; set;
        }

        [Property]
        public string OrgId {
            get; set;
        }


        [JsonIgnore]
        [Set(0, Table = "SEC_PERSONGROUPASSOCIATION",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(PersonGroupAssociation))]
        public ISet<PersonGroupAssociation> PersonGroups {
            get; set;
        }


        [Set(0, Table = "sw_user_userprofile",
        Lazy = CollectionLazy.False)]
        [Key(1, Column = "user_id")]
        [ManyToMany(2, Column = "profile_id", ClassType = typeof(UserProfile))]
        public ISet<UserProfile> Profiles {
            get; set;
        }

        [Set(0, Table = "sw_user_customrole",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(UserCustomRole))]
        public ISet<UserCustomRole> CustomRoles {
            get; set;
        }


        [Set(0, Table = "SW_USER_CUSTOMDATACONSTRAINT",
          Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "user_id")]
        [OneToMany(2, ClassType = typeof(UserCustomConstraint))]
        public ISet<UserCustomConstraint> CustomConstraints {
            get; set;
        }
        
        [OneToOne(ClassType = typeof(UserPreferences), Lazy = Laziness.False, PropertyRef = "UserId", Cascade = "all")]
        public UserPreferences UserPreferences {
            get; set;
        }

        //public string DisplayName {
        //    get { return Person.FirstName + " " + Person.LastName; }
        //}


        public User() {
            Person = new Person();
            Profiles = new HashedSet<UserProfile>();
            CustomRoles = new HashedSet<UserCustomRole>();
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

        public static User CreateAdminUser(string userName, string firstName, string lastName, string siteId, string orgId, string department, string phone, string language, string password, string storeloc, string email) {
            return new User() {
                UserName = userName,
                Person = new Person(),
                FirstName = firstName,
                LastName = lastName,
                SiteId = siteId,
                OrgId = orgId,
                IsActive = true,
                Password = !string.IsNullOrEmpty(password) ? AuthUtils.GetSha1HashData(password) : null,
                Email = email
            };
        }



        public void MergeFromDBUser(User dbUSer) {
            //keep password unchanged
            Id = Id ?? dbUSer.Id;
            UserName = UserName ?? dbUSer.UserName;
            Password = Password ?? dbUSer.Password;
            FirstName = FirstName ?? dbUSer.FirstName;
            LastName = LastName ?? dbUSer.LastName;
            SiteId = SiteId ?? dbUSer.SiteId;
            OrgId = OrgId ?? dbUSer.OrgId;

            PersonGroups = dbUSer.PersonGroups;
            CustomRoles = dbUSer.CustomRoles;
            Profiles = !Profiles.IsEmpty ? Profiles : dbUSer.Profiles;
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




        public static User FromJson(JObject jObject) {
            var user = new User();
            user.CustomRoles = new HashedSet<UserCustomRole>();
            user.CustomConstraints = new HashedSet<UserCustomConstraint>();
            user.Profiles = new HashedSet<UserProfile>();
            var roles = jObject["customRoles"];
            if (roles != null) {
                foreach (var jToken in roles.ToArray()) {
                    user.CustomRoles.Add(jToken.ToObject<UserCustomRole>());
                }
            }
            var customConstraints = jObject["customConstraints"];
            if (customConstraints != null) {
                foreach (var jToken in customConstraints.ToArray()) {
                    user.CustomConstraints.Add(jToken.ToObject<UserCustomConstraint>());
                }
            }
            var profiles = jObject["profiles"];
            if (profiles != null) {
                foreach (var jToken in profiles.ToArray()) {
                    user.Profiles.Add(UserProfile.FromJson(jToken));
                }
            }
            var personGroups = jObject["personGroups"];
            if (personGroups != null) {
                foreach (var jToken in personGroups.ToArray()) {
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

        public string FullName {
            get {
                if (Person == null || (Person.FirstName == null && Person.LastName == null)) {
                    return FirstName + " " + LastName;
                }
                return Person.FirstName + " " + Person.LastName;
            }
        }

        private bool Equals(User other) {
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((User)obj);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public class UserNameEqualityUser {
            public readonly User user;

            public UserNameEqualityUser(User user) {
                this.user = user;
            }

            protected bool Equals(UserNameEqualityUser other) {
                return Equals(user.UserName, other.user.UserName);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((UserNameEqualityUser)obj);
            }

            public override int GetHashCode() {
                return (user != null ? user.GetHashCode() : 0);
            }
        }
    }
}