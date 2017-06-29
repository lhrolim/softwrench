using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._4._0 {

    [Migration(201404221149)]
    public class Migration20140422Swweb112 : FluentMigrator.Migration {

        public override void Up()
        {

            Create.Table("SEC_PERSONGROUP")
                .WithIdColumn()
                .WithColumn("name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("description").AsString(MigrationUtil.StringLarge).Nullable();

            Create.Table("SEC_PERSONGROUPASSOCIATION")
                .WithIdColumn()
                .WithColumn("persongroup_id").AsInt32().ForeignKey("fk_persongroup_id", "SEC_PERSONGROUP", "id")
                .WithColumn("delegate").AsBoolean().Nullable().WithDefaultValue(false)
                .WithColumn("user_id").AsInt32().ForeignKey("fk_pga_user_id", "SW_USER2", "id");

        }

        public override void Down() {

        }
    }
}