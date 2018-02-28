using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;
using softWrench.sW4.Extension;

namespace softwrench.sw4.offlineserver.migration {

    [Migration(201802271025)]
    public class MigrationSwoff385 : Migration {

        public override void Up() {
            Alter.Table("OFF_SYNCOPERATION").AddColumn("AttachmentCount").AsInt32().Nullable();
        }

        public override void Down() {
        }
    }



}
