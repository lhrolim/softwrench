using cts.commons.persistence;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Email {
    [Class(Table = "EMAIL_HISTORY", Lazy = false)]
    class EmailHistory : IBaseEntity
    {
        public const string byUserIdEmailAddess = "FROM EmailHistory WHERE lower(UserID) IN (:p0) AND lower(EmailAddress) IN (:p1)";
        public const string byUserId = "FROM EmailHistory WHERE lower(UserID) = lower(?)";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string UserID { get; set; }

        [Property]
        public virtual string EmailAddress { get; set; }

        public EmailHistory() {
            
        }

        public EmailHistory(int? id, string userid, string emailaddress) {
            Id = id;
            UserID = userid;
            EmailAddress = emailaddress;
        }

        protected bool Equals(EmailHistory other) {
            return UserID.EqualsIc(other.UserID) && EmailAddress.EqualsIc(other.EmailAddress);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmailHistory) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (UserID.GetHashCode()*397) ^ EmailAddress.GetHashCode();
            }
        }

        public override string ToString() {
            return "EmailAddress: {0}, UserID: {1}, Id: {2}".Fmt(EmailAddress,UserID,Id);
        }
    }
}
