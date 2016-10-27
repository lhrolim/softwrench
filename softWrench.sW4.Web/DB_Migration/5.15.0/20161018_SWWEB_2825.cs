using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._5._15._0 {
    [Migration(201610181600)]

    public class MigrationSWWEB2825 : Migration {
        public override void Up()
        {
            Create.Table("MAP_DEFINITION")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("key_").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("description").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("sourcealias").AsString(MigrationUtil.StringLarge)
                .WithColumn("destinationalias").AsString(MigrationUtil.StringLarge);


            Rename.Table("SW_MAPPING").To("MAP_MAPPING");

            Alter.Table("MAP_MAPPING").AddColumn("definition_id").AsInt64().ForeignKey("MAP_DEFINITION", "id");

            Create.Index("sw_mapping_def_idx").OnTable("MAP_MAPPING").OnColumn("definition");

            Delete.Index("sw_mapping_key_idx").OnTable("SW_MAPPING");
            Delete.Column("key").FromTable("SW_MAPPING");
            


        }

        public override void Down() {
    
        }
    }
}