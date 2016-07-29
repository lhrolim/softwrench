using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201607141540)]
    public class MigrationSwoff197 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Table("PREF_GENERICPROPERTIES")
               .WithColumn("id").AsInt32().PrimaryKey().Identity()
               .WithColumn("key_").AsString(MigrationUtil.StringSmall)
               .WithColumn("value_").AsString(MigrationUtil.StringLarge)
               .WithColumn("type_").AsString(MigrationUtil.StringSmall).Nullable()
               .WithColumn("preference_id").AsInt32().ForeignKey("fk_propprefid", "PREF_GENERALUSER", "id").NotNullable();

            Create.UniqueConstraint("uq_key_prefid").OnTable("PREF_GENERICPROPERTIES").Columns("key_", "preference_id");
        }

        public override void Down() {
        }
    }
}
