using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.dynforms.classes.model.migration {

    [Migration(201709251422)]
    public class Swweb3183Migration : Migration {

        public override void Up() {

            Create.Table("FORM_METADATA")
                .WithColumn("name").AsString(MigrationUtil.StringMedium).PrimaryKey()
                .WithColumn("entity").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("title").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("description").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("changedate").AsDateTime().NotNullable()
                .WithColumn("status").AsString(MigrationUtil.StringSmall).NotNullable();

            Create.Table("FORM_METADATA_DEF").WithIdColumn()
                .WithColumn("form_name").AsString(MigrationUtil.StringMedium).ForeignKey("fk_fmd_fm", "form_metadata", "name").Unique()
                .WithColumn("detaildefinition").AsSwBinary()
                .WithColumn("newdetaildefinition").AsSwBinary()
                .WithColumn("listdefinition").AsSwBinary();


            Create.Table("FORM_DATAMAP")
                .WithColumn("FormDatamapId").AsInt64().NotNullable().PrimaryKey().Identity()
                .WithColumn("form_name").AsString(MigrationUtil.StringMedium).ForeignKey("fk_fd_fm", "form_metadata", "name")
                .WithColumn("userid").AsString().NotNullable()
                .WithColumn("datamap").AsString(MigrationUtil.StringMax).Nullable()
                .WithColumn("changedate").AsDateTime().NotNullable();

            Create.Table("FORM_DATAMAP_IDX")
                .WithIdColumn(true)
                .WithColumn("datamap_id").AsInt64().ForeignKey("fk_fdi_fd", "FORM_DATAMAP", "FormDatamapId")
                .WithColumn("attributename").AsString().NotNullable()
                .WithColumn("form_name").AsString(MigrationUtil.StringMedium).ForeignKey("fk_fdi_fm", "form_metadata", "name")
                .WithColumn("value_").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("numvalue").AsInt64().Nullable()
                .WithColumn("datevalue").AsDateTime().Nullable();

            Create.UniqueConstraint("uq_form_dm_app").OnTable("FORM_DATAMAP").Columns("form_name", "userid");
            Create.UniqueConstraint("uq_form_dmi_app").OnTable("FORM_DATAMAP_IDX").Columns("datamap_id", "form_name", "attributename");

            //for quick search
            Create.Index("form_dmi_val").OnTable("FORM_DATAMAP_IDX").OnColumn("value_").Ascending().OnColumn("form_name").Ascending();
            Create.Index("form_dmi_nval").OnTable("FORM_DATAMAP_IDX").OnColumn("numvalue").Ascending().OnColumn("form_name").Ascending();
            Create.Index("form_dmi_dval").OnTable("FORM_DATAMAP_IDX").OnColumn("datevalue").Ascending().OnColumn("form_name").Ascending();

            //used for non quick search
            Create.Index("form_dmi_aval").OnTable("FORM_DATAMAP_IDX").OnColumn("value_").Ascending().OnColumn("attributename").Ascending().OnColumn("form_name").Ascending();
            Create.Index("form_dmi_anval").OnTable("FORM_DATAMAP_IDX").OnColumn("numvalue").Ascending().OnColumn("attributename").Ascending().OnColumn("form_name").Ascending();
            Create.Index("form_dmi_adval").OnTable("FORM_DATAMAP_IDX").OnColumn("datevalue").Ascending().OnColumn("attributename").Ascending().OnColumn("form_name").Ascending();


            Create.Index("form_dm_meid").OnTable("FORM_DATAMAP").OnColumn("form_name").Ascending();
            Create.Index("form_dm_nauid").OnTable("FORM_DATAMAP").OnColumn("form_name").Ascending().OnColumn("userid").Ascending();


            Execute.Sql("insert into SW_ROLE (name,isactive,label,description,deletable) values('formselection', 1, 'Form Selection', 'This role allows the form selection mechanism', 0)");

        }




        public override void Down() {
        }
    }

    [Migration(201712191422)]
    public class Swweb3183_2Migration : Migration {
        public override void Up() {
            Alter.Table("FORM_DATAMAP").AddColumn("changeby").AsInt32().Nullable().ForeignKey("fk_fd_us", "SW_USER2", "Id");
        }

        public override void Down() {
            throw new System.NotImplementedException();
        }
    }
}

