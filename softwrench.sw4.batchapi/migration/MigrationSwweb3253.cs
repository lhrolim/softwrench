using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.batch.api.migration {


    [Migration(201712051600)]
    public class MigrationSwweb3253 : Migration{

        public override void Up(){
            Alter.Table("BAT_MULBATCH").AlterColumn("ItemIds").AsString(MigrationUtil.StringMax).NotNullable();
        }

        public override void Down(){
            
        }
    }
}
