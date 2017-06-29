using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration {

    [Migration(201404090900)]
    public class Migration20140409Swweb118 : FluentMigrator.Migration {

        public override void Up() {
//            Delete.Column("condition_").FromTable("conf_propertyvalue");

            Create.Table("CONF_CONDITION")
                .WithIdColumn()
                .WithColumn("alias_").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("siteid").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("metadataid").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("fullkey").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("description").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("environment").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("global").AsBoolean().WithDefaultValue(true).NotNullable();

            Alter.Table("conf_propertyvalue")
                .AddColumn("condition_id").AsInt32().Nullable().ForeignKey("fk_conf_pv_cond", "conf_condition", "id")
                .AddColumn("systemvalue").AsString(MigrationUtil.StringLarge).Nullable()
                .AddColumn("module").AsString(MigrationUtil.StringSmall).Nullable()
                .AddColumn("userprofile").AsInt32().Nullable()
                .AddColumn("systemblobvalue").AsBinary().Nullable();

            Create.Table("CONF_WCCONDITION")
                .WithColumn("WcWcId").AsInt32().PrimaryKey().ForeignKey("wccondition_parent", "conf_condition", "id")
                .WithColumn("mode_").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("schema_").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("fullkey").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("parentschema").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("parentapplication").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("parentmode").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("attributename").AsString(MigrationUtil.StringMedium).Nullable();

            Alter.Table("conf_propertydefinition").AddColumn("alias_").AsString(MigrationUtil.StringSmall).Nullable();

        }

        public override void Down() {

        }
    }
}