using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    /// <summary>
    /// This entity represents links to activate the user on softwrench, via an email confirmation
    /// 
    /// Each user should have only one link at a time.
    /// 
    /// The links should expire after a certain amount of time, and the token should be generated in a way that no one could guess it by trial and error
    /// 
    /// </summary>
    [Class(Table = "USER_ACTIVATIONLINK", Lazy = false)]
    public class UserActivationLink : IBaseEntity {

        public const string TokenByUser = "From UserActivationLink where User = ?";

        public const string UserByToken = "From UserActivationLink where Token = ?";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public string Token { get; set; }

        //one to one here, but marking like that due to a nhibernate bug
        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public User User { get; set; }

        [Property]
        public DateTime SentDate { get; set; }

        [Property]
        public DateTime ExpirationDate { get; set; }

        public bool HasExpired() {
            return ExpirationDate <= DateTime.Now;
        }
    }
}
