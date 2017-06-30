using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using cts.commons.persistence;
using softwrench.sw4.user.classes.entities.security;
using System.Linq;
using System.Text;
using cts.commons.persistence.Transaction;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using FluentMigrator;
using log4net;
using softwrench.sw4.api.classes.migration;

namespace softwrench.sw4.user.Migration {
    //    [Migration(201706162232)]

    [Migration(201706291303)]
    public class MigrationSwweb3049 : FluentMigrator.Migration {

        public override void Up() {
            Create.Table("SEC_SECTION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("sectionid").AsString().NotNullable()
                .WithColumn("permission").AsString()
                .WithColumn("schema_id").AsInt32().ForeignKey("fk_secp_sp", "SEC_CONTAINER_PER", "id").Nullable();
        }

        public override void Down() {
        }
    }
}




