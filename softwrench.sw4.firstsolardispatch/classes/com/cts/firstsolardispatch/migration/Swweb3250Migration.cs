using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201711021700)]
    public class Swweb3250Migration : Migration {

        public override void Up() {

            Alter.Table("DISP_TICKET")
                .AddColumn("reportedby").AsInt32().ForeignKey("dt_swu_repby", "sw_user2", "id").Nullable()
                .AddColumn("statusreportedby").AsInt32().ForeignKey("dt_swu_srepby", "sw_user2", "id").Nullable()
                .AddColumn("status").AsString().Nullable()
                .AddColumn("statusdate").AsDateTime().Nullable();

        }

        public override void Down() {

        }

    }
}
