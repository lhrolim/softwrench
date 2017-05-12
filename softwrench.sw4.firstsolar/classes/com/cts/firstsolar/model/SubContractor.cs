using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_SUBCONTRACTOR", Lazy = false)]
    public class SubContractor {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Name { get; set; }

        [Property]
        public string Description { get; set; }
    }
}
