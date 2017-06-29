using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;
using softWrench.sW4.Util;

namespace softwrench.sw4.user.Migration {
    [Migration(201508221123)]
    public class MigrationSwweb1331 : FluentMigrator.Migration {

        public override void Up() {
            Create.Table("USER_ACTIVATIONLINK")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("token").AsString(MigrationUtil.StringMedium).NotNullable().NotNullable()
                .WithColumn("user_id").AsInt32().ForeignKey("userlink_user_fk", "sw_user2", "id").NotNullable()
                .WithColumn("sentdate").AsDateTime().NotNullable()
                .WithColumn("expirationdate").AsDateTime().NotNullable();

            Create.UniqueConstraint("uq_userlinktoken").OnTable("USER_ACTIVATIONLINK").Column("token");
            Create.UniqueConstraint("uq_userlinkuser").OnTable("USER_ACTIVATIONLINK").Column("user_id");

            if (!MigrationContext.IsOracle) {
                Create.Index("idx_userlink_token").OnTable("USER_ACTIVATIONLINK").OnColumn("token");
            }

        }

        public override void Down() {
        }
    }
}
