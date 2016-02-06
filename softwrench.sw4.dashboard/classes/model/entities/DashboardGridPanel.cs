using System;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {


    [JoinedSubclass(
        NameType = typeof(DashboardGridPanel), 
        Lazy = false, 
        ExtendsType = typeof(DashboardBasePanel), 
        Table = "DASH_GRIDPANEL")]
    //[Class(Table = "DASH_GRIDPANEL", Lazy = false)]
    public class DashboardGridPanel : DashboardBasePanel {
      

        [Key(-1, Column = "GPID")]
        public virtual int? GpId { get; set; }


        [Property]
        public virtual String Application { get; set; }

        [Property]
        public virtual String SchemaRef { get; set; }

        /// <summary>
        /// List of semicolon separated list of fields to use, each of them needs to be defined on the schema
        /// </summary>
        [Property(Column = "fields")]
        public virtual String AppFields { get; set; }

        [Property]
        public virtual String DefaultSortField { get; set; }


        [Property(Column = "limit_")]
        public virtual int? Limit { get; set; }

      
    }
}
