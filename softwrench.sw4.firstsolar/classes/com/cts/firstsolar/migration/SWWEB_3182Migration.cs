using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201709181411)]
    public class SWWEB_3182Migration : Migration {

        public override void Up() {
            Execute.Sql("INSERT INTO OPT_DAILY_OUTAGE_ACTION (action,completed,actiontime,workpackageid) SELECT openactionitems,0,meetingtime,workpackageid FROM OPT_DAILY_OUTAGE_MEETING WHERE openactionitems is not null and completedactionitems is null");
            Execute.Sql("INSERT INTO OPT_DAILY_OUTAGE_ACTION (action,completed,actiontime,workpackageid) SELECT completedactionitems,1,meetingtime,workpackageid FROM OPT_DAILY_OUTAGE_MEETING WHERE completedactionitems is not null ");
        }

        public override void Down() {

        }

    }



}
