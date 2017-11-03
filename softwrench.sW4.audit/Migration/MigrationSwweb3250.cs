using FluentMigrator;

namespace softwrench.sW4.audit.Migration {
    [Migration(201711031554)]
    public class MigrationSwweb3250 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Index("idx_aud_aaid").OnTable("audit_entry")
                .OnColumn("RefApplication").Ascending()
                .OnColumn("RefId").Ascending()
                .OnColumn("action").Ascending();

        }

        public override void Down() {
            
        }
    }
}
