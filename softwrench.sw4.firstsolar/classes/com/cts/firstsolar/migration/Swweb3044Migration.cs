using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201706290300)]
    public class Swweb3044Migration : Migration {

        public override void Up() {
            Create.Column("buildcomplete").OnTable("OPT_WORKPACKAGE").AsBoolean().Nullable();
        }

        public override void Down() {
        }
    }
}
