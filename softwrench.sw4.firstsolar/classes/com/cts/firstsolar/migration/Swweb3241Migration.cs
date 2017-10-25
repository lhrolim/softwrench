using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201710250745)]
    public class Swweb3241Migration : FluentMigrator.Migration {

        public override void Up() {
            Execute.Sql("insert into SW_ROLE (name,isactive,label,description,deletable) values('onlinefacilityadmin', 1, 'Online Facility Admin', 'This role ensures that the user is able to view all the facilities of the system. This only works for online', 0)");
        }

        public override void Down() {

        }
    }
}




