using cts.commons.persistence;
using Iesi.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    [Class(Table = "DASH_USERPREFERENCES", Lazy = false)]
    public class UserDashBoardPreferences : IBaseEntity
    {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }


        [Property]
        public virtual int? UserId { get; set; }

        [Property(Column = "preferred_id")]
        public virtual int? PreferredDashboardId { get; set; }

        

    }
}
