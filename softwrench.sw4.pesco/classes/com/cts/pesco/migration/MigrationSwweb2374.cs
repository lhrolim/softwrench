using FluentMigrator;
using softWrench.sW4.Util;

namespace softwrench.sW4.pesco.classes.com.cts.pesco.migration {
    [Migration(201604211500)]
    public class MigrationSwweb2374 : Migration {
        
        public override void Up() {
            if (!"pesco".Equals(ApplicationConfiguration.ClientName)) {
                return;
            }

            Create.Table("PESCO_DEVICE").WithColumn("deviceid").AsString(100).PrimaryKey()
                .WithColumn("name").AsString(100).Nullable()
                .WithColumn("parentdeviceid").AsString(100).Nullable()
                .WithColumn("quantum").AsInt32().Nullable()
                .WithColumn("position").AsInt32().Nullable();

            Create.Table("PESCO_DEVICE_VALUE").WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("deviceid").AsString(100)
                .WithColumn("timestamp").AsInt64()
                .WithColumn("interfaceid").AsString(100)
                .WithColumn("itemid").AsInt32()
                .WithColumn("valuelong").AsInt64().Nullable()
                .WithColumn("valuefloat1").AsFloat().Nullable()
                .WithColumn("valuefloat2").AsFloat().Nullable()
                .WithColumn("valuefloat3").AsFloat().Nullable();

            Create.Table("PESCO_MESSAGE_CACHE").WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("deviceid").AsString(100)
                .WithColumn("start").AsInt64()
                .WithColumn("end").AsInt64();
        }

        public override void Down() {
            if (!"pesco".Equals(ApplicationConfiguration.ClientName)) {
                return;
            }

            Delete.Table("PESCO_DEVICE");
            Delete.Table("PESCO_DEVICE_VALUE");
            Delete.Table("PESCO_MESSAGE_CACHE");
        }
    }
}
