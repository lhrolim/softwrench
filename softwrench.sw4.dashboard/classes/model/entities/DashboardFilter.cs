using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    [Component]
    public class DashboardFilter {

        [Property]
        public virtual int? UserId { get; set; }

        [Property]
        public virtual int? UserProfiles { get; set; }


    }
}
