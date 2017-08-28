using System;
using cts.commons.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sW4.audit.classes.Model {

    [Class(Table = "AUD_SESSION", Lazy = false)]
    public class AuditSession {

        public static string ByUserIdAndCookie = "from AuditSession where UserId =? and Cookie = ?";

        public static string HashCookie(string cookie) {
            return AuthUtils.HmacShaEncode(cookie);
        }



        public AuditSession() {

        }

        public AuditSession(int? sessionId) {
            Id = sessionId;
        }

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual int? UserId { get; set; }

        [Property]
        public string Cookie { get; set; }

        [Property]
        public int? TimezoneOffSet { get; set; }

        [Property]
        public virtual DateTime? StartDate { get; set; }

        [Property]
        public virtual DateTime? EndDate { get; set; }
    }
}
