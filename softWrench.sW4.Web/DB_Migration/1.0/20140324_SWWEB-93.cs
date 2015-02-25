using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration {

    [Migration(201403241517)]
    public class Issue20140324Hap113 : FluentMigrator.Migration {

        public override void Up() {
            Create.Table("SEC_ROLEGROUP")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("label").AsString(MigrationUtil.StringMedium)
                .WithColumn("description").AsString(MigrationUtil.StringLarge);

            Alter.Table("SW_ROLE").AddColumn("label").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("SW_ROLE").AddColumn("description").AsString(MigrationUtil.StringLarge).Nullable();
            Alter.Table("SW_ROLE").AddColumn("rolegroup_id").AsInt32().Nullable().ForeignKey("fk_role_rolegroup", "SEC_ROLEGROUP", "id");


        }

        public override void Down() {
            Delete.Column("label").FromTable("SW_ROLE");
            Delete.Column("description").FromTable("SW_ROLE");
            Delete.Column("rolegroup_id").FromTable("SW_ROLE");
        }
    }
}