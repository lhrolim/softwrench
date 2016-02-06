using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201312241700)]
    public class Issue20140106Configuration : FluentMigrator.Migration {
        public override void Up()
        {

            Create.Table("CONF_PROPERTYDEFINITION")
                .WithColumn("fullkey").AsString(255).PrimaryKey()
                .WithColumn("key_").AsString(255).NotNullable()
                .WithColumn("defaultvalue").AsString(4000).NotNullable()
                .WithColumn("description").AsString(65535).Nullable()
                .WithColumn("datatype").AsString(50).NotNullable()
                .WithColumn("renderer").AsString(50).Nullable()
                .WithColumn("visible").AsBoolean().WithDefaultValue(true)
                .WithColumn("contextualized").AsBoolean().WithDefaultValue(false);


            Create.Table("CONF_PROPERTYVALUE")
                .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                .WithColumn("condition_").AsString(4000).Nullable()
                .WithColumn("value").AsString(4000).NotNullable()
                .WithColumn("definition_id").AsString().ForeignKey("propval_definition_def", "CONF_PROPERTYDEFINITION", "fullkey").NotNullable();

            
//            Create.UniqueConstraint("CONF_DEF_VAL_KEY").OnTable("conf_definition_value").Columns(new[] { "definition_id", "value_id" });



        }

        public override void Down() {
            Delete.Table("CONF_PROPERTYDEFINITION");
            Delete.Table("CONF_PROPERTYVALUE");
        }
    }
}