using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201404281310)]
    public class Migration20140428Swweb115 : FluentMigrator.Migration {
        public override void Up() {
            Create.Column("maximopersonid").OnTable("SW_USER2").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {
            Delete.Column("maximopersonid").FromTable("SW_USER2");
        }
    }

    [Migration(201405021753)]
    public class Test : FluentMigrator.Migration {
        public override void Up() {
            Create.Index("idx_active_roles").OnTable("SW_ROLE").OnColumn("isactive");
        }

        public override void Down() {
        }
    }

    [Migration(201405141945)]
    public class Migration20140514Swweb115 : Migration {
        public override void Up() {
            if (ApplicationConfiguration.ClientName == "hapag") {
                Execute.Sql(
                    "create view hlag_location as select u.maximopersonid,u.username,p.name,p.description from sec_persongroupassociation ass inner join sec_persongroup p on ass.persongroup_id = p.id inner join sw_user2 u on ass.user_id = u.id;");
            }
        }

        public override void Down() {
        }
    }
}