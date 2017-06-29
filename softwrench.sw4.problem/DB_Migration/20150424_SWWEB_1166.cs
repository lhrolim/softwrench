using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {

    [Migration(201504241500)]
    public class Migration20150424SWWEB1166 : Migration {

        public override void Up()
        {

            Create.Table("PROB_PROBLEM")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("RecordType").AsString().NotNullable()
                .WithColumn("RecordId").AsString().NotNullable()
                .WithColumn("Data").AsBinary().NotNullable()
                .WithColumn("CreatedDate").AsDateTime().NotNullable()
                .WithColumn("CreatedBy").AsString().NotNullable()
                .WithColumn("Assignee").AsString().Nullable()
                .WithColumn("Priority").AsInt32().NotNullable()
                .WithColumn("StackTrace").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("Profiles").AsString().Nullable()
                .WithColumn("ProblemHandler").AsString().Nullable()
                .WithColumn("Status").AsString().NotNullable().WithDefaultValue("OPEN");

            Create.Table("PROB_ADDITIONALARGS")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("ProblemId").AsInt64().NotNullable()
                .WithColumn("Position").AsInt32().NotNullable()
                .WithColumn("Data").AsBinary().NotNullable();

            Create.Index("PROB_PROBLEM_ASSIGNEE").OnTable("PROB_PROBLEM")
                .OnColumn("Assignee");

            Create.Index("PROB_PROBLEM_TYPE").OnTable("PROB_PROBLEM")
                .OnColumn("RecordType");

            Create.ForeignKey("FK_PROBLEM").FromTable("PROB_ADDITIONALARGS")
                .ForeignColumn("ProblemId").ToTable("PROB_PROBLEM")
                .PrimaryColumn("Id");
        }

        public override void Down() {

        }
    }
}