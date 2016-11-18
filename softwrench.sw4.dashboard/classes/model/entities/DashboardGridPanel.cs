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
        public virtual int? GpId {
            get; set;
        }


        [Property]
        public virtual string Application {
            get; set;
        }

        [Property]
        public virtual string SchemaRef {
            get; set;
        }

        /// <summary>
        /// List of semicolon separated list of fields to use, each of them needs to be defined on the schema
        /// </summary>
        [Property(Column = "fields")]
        public virtual string AppFields {
            get; set;
        }

        /// <summary>
        /// Sort field(s) to be used.
        /// 
        /// Valid syntax(es):
        /// 
        /// xxx
        /// xxx asc
        /// xxx asc, yyy desc
        /// xxx desc, yyy asc, zzz asc
        /// 
        /// 
        /// </summary>
        [Property]
        public virtual string DefaultSortField {
            get; set;
        }





        [Property(Column = "limit_")]
        public virtual int? Limit {
            get; set;
        }


        public override string GetApplicationName() {
            return Application;
        }
    }
}
