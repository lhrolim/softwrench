using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201607141540)]
    public class MigrationSwoff197 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Table("PREF_GENERICPROPERTIES")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("key").AsString(MigrationUtil.StringSmall)
                .WithColumn("value").AsString(MigrationUtil.StringLarge)
                .WithColumn("preference_id").AsInt32().ForeignKey("fk_propprefid", "PREF_GENERALUSER", "id").NotNullable();
        }

        public override void Down() {
        }
    }
}
