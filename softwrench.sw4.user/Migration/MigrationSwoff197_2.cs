using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201607151540)]
    public class MigrationSwoff1972 : FluentMigrator.Migration {

        public override void Up() {
       
            //patch to fix column names
            if (Schema.Table("PREF_GENERICPROPERTIES").Column("key").Exists()) {
                Rename.Column("key").OnTable("PREF_GENERICPROPERTIES").To("key_");
            }

            if (Schema.Table("PREF_GENERICPROPERTIES").Column("value").Exists()) {
                Rename.Column("value").OnTable("PREF_GENERICPROPERTIES").To("value_");
            }

            if (Schema.Table("PREF_GENERICPROPERTIES").Column("value").Exists()) {
                Rename.Column("type").OnTable("PREF_GENERICPROPERTIES").To("type_");
            }
        }

        public override void Down() {
        }
    }
}
