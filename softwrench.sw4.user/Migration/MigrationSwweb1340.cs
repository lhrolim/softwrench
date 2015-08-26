using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201508251123)]
    public class MigrationSwweb1340 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Table("USER_STATISTICS")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt32().ForeignKey("userstatistics_user_fk", "sw_user2", "id").NotNullable()
                .WithColumn("lastlogindate").AsDateTime()
                .WithColumn("logincount").AsInt32();
            
        }

        public override void Down() {
        }
    }
}
