using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    /// <summary>
    /// This entity holds aggregate information for usage of the system by a giving user
    /// 
    /// </summary>
    [Class(Table = "USER_STATISTICS", Lazy = false)]
    public class UserStatistics : IBaseEntity {

        public const string ByUser = "From UserStatistics where User = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        public UserStatistics() {
            LoginCount = 1;
            LastLoginDate = DateTime.Now;
        }

        //one to one here, but marking like that due to a nhibernate bug
        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public User User { get; set; }

        [Property]
        public DateTime LastLoginDate { get; set; }

        [Property]
        public int LoginCount { get; set; }


    }
}
