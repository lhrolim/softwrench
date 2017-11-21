using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201711211700)]
    public class Swweb3242Migration : Migration {

        public override void Up() {

            Alter.Table("DISP_TICKET")
                .AddColumn("immediatedispatch").AsBoolean().WithDefaultValue(false)
                .AddColumn("dispatchexpecteddate").AsDateTime().Nullable();

        }

        public override void Down() {

        }

    }
}
