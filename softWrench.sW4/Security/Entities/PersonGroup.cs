using System;
using System.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Security.Entities {

    [Class(Table = "SEC_PERSONGROUP", Lazy = false)]
    public class PersonGroup {

        public const string PersonGroupByName = "from PersonGroup where Name =?";
        public const string PersonGroupByNames = "from PersonGroup where Name in (:p0)";
        public const string SuperGroups = "from PersonGroup where supergroup=true";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Name { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Property]
        public virtual long Rowstamp { get; set; }

        [Property]
        public virtual Boolean SuperGroup { get; set; }



        public void MergeMaximoWithPersonGroupUpdated(PersonGroup personGroupUpdated) {
            Description = UpdateIfNotNull(Description, personGroupUpdated.Description);
        }

        public string UpdateIfNotNull(string oldValue, string newValue) {
            return String.IsNullOrEmpty(newValue) ? oldValue : newValue;
        }

        protected bool Equals(PersonGroup other) {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersonGroup)obj);
        }

        public override int GetHashCode() {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString() {
            return string.Format("Name: {0}, Description: {1}, SuperGroup: {2}", Name, Description, SuperGroup);
        }
    }
}
