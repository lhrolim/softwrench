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

    /// <summary>
    /// Migration for the Outage OPT data
    /// </summary>
    [Migration(201704181103)]
    public class SWWEB_2930Migration : Migration {

        public override void Up() {


            Create.Table("OPT_WORKPACKAGE")
                .WithIdColumn()
                .WithColumn("workorderid").AsInt64().NotNullable()
                .WithColumn("wonum").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("createddate").AsDateTime().NotNullable()
                .WithColumn("tier").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("interconnectdocs").AsString(MigrationUtil.StringSmall).Nullable();

            Create.Table("OPT_SUBCONTRACTOR")
                .WithIdColumn()
                .WithColumn("name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("description").AsString(MigrationUtil.StringMedium).Nullable();


            Create.Table("OPT_CALLOUT")
                .WithIdColumn()
                .WithColumn("workpackageid").AsInt32().NotNullable()
                .WithColumn("subcontractorid").AsInt32().NotNullable()
                .WithColumn("sendtime").AsDateTime().NotNullable()
                .WithColumn("status").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("expirationdate").AsDateTime().Nullable()
                .WithColumn("ponumber").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("tonumber").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("email").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("sitename").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("billingentity").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("nottoexceedamount").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("remainingfunds").AsString(MigrationUtil.StringSmall).Nullable()
                .WithColumn("serviceprovider").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("scopeofwork").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("plantcontacts").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("otherinfo").AsString(MigrationUtil.StringLarge).Nullable();

        }

        public override void Down() {

        }

    }

    /// <summary>
    /// Migration for the Outage OPT data
    /// </summary>
    [Migration(201705071711)]
    public class SWWEB_29302Migration : Migration {

        public override void Up() {


            Create.Table("GEN_LISTRELATIONSHIP")
                .WithIdColumn(true)
                .WithColumn("parententity").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("parentid").AsInt64().NotNullable()
                .WithColumn("parentcolumn").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("value").AsString(MigrationUtil.StringSmall).NotNullable();

            Create.Index("gen_rel_triple_idx").OnTable("GEN_LISTRELATIONSHIP")
                .OnColumn("parententity").Ascending()
                .OnColumn("parentid").Ascending()
                .OnColumn("parentcolumn").Ascending();

            Create.Index("gen_rel_double_idx").OnTable("GEN_LISTRELATIONSHIP")
                .OnColumn("parententity").Ascending()
                .OnColumn("parentid").Ascending();


        }

        public override void Down() {

        }

    }

    /// <summary>
    /// Migration for the Outage OPT data
    /// </summary>
    [Migration(201705102111)]
    public class SWWEB_29303Migration : Migration {

        public override void Up() {
            Alter.Table("OPT_WORKPACKAGE")
                .AddColumn("TestResultReviewEnabled").AsBoolean().Nullable()
                .AddColumn("SubContractorEnabled").AsBoolean().Nullable()
                .AddColumn("MaintenanceEnabled").AsBoolean().Nullable()
                .AddColumn("ResultsForReview").AsString(MigrationUtil.StringMedium).Nullable()
                .AddColumn("DaysUponClosure").AsInt16().Nullable()
                .AddColumn("RequestExplanation").AsString(MigrationUtil.StringMax).Nullable();


        }

        public override void Down() {

        }

    }

    /// <summary>
    /// Migration for the Outage OPT data
    /// </summary>
    [Migration(201705111400)]
    public class SWWEB_29304Migration : Migration {

        public override void Up() {
            Create.Table("OPT_MAINTENANCE_ENG")
                .WithIdColumn()
                .WithColumn("workpackageid").AsInt32().NotNullable()
                .WithColumn("engineer").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("sendtime").AsDateTime().NotNullable()
                .WithColumn("status").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("reason").AsString(MigrationUtil.StringLarge).Nullable();
        }

        public override void Down() {

        }
    }

    /// <summary>
    /// Migration for email column of maintenance engineering
    /// </summary>
    [Migration(201705171800)]
    public class SWWEB_29305Migration : Migration {

        public override void Up() {
            Create.Column("email").OnTable("OPT_MAINTENANCE_ENG").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {

        }
    }

    /// <summary>
    /// Migration for outagetype column of work package
    /// </summary>
    [Migration(201705252300)]
    public class SWWEB_29306Migration : Migration {

        public override void Up() {
            Execute.Sql("delete from GEN_LISTRELATIONSHIP where parententity = 'WorkPackage' and parentcolumn = 'outagetype'");
            Create.Column("outagetype").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringSmall).Nullable();
        }

        public override void Down() {

        }
    }

    /// <summary>
    /// Adding a few more columns
    /// </summary>
    [Migration(201705311130)]
    public class SWWEB_2930_7Migration : Migration {

        public override void Up() {
            Create.Column("token").OnTable("OPT_CALLOUT").AsString(MigrationUtil.StringMedium).Nullable();
            Create.Column("notes").OnTable("OPT_CALLOUT").AsString(MigrationUtil.StringMax).Nullable();


            Execute.Sql("update opt_callout set status = 'Scheduled' where status like 'submited'");
            Execute.Sql("update opt_callout set status = 'Sent' where status like 'completed'");

            Execute.Sql("update OPT_MAINTENANCE_ENG set status = 'Scheduled' where status like 'submited'");
            Execute.Sql("update OPT_MAINTENANCE_ENG set status = 'Sent' where status like 'completed'");

        }

        public override void Down() {

        }

        /// <summary>
        /// Adding a few more columns
        /// </summary>
        [Migration(201705312330)]
        public class SWWEB_2930_8Migration : Migration {

            public override void Up() {
                Create.Column("actualsendtime").OnTable("OPT_CALLOUT").AsDateTime().Nullable();
                Create.Column("actualsendtime").OnTable("OPT_MAINTENANCE_ENG").AsDateTime().Nullable();
            }

            public override void Down() {

            }
        }

        /// <summary>
        /// Adding send now column
        /// </summary>
        [Migration(201706012020)]
        public class SWWEB_2930_9Migration : Migration {

            public override void Up() {
                Create.Column("sendnow").OnTable("OPT_CALLOUT").AsBoolean().WithDefaultValue(false);
                Create.Column("sendnow").OnTable("OPT_MAINTENANCE_ENG").AsBoolean().WithDefaultValue(false);
            }

            public override void Down() {

            }
        }

        /// <summary>
        /// Adding send now column
        /// </summary>
        [Migration(201706020020)]
        public class SWWEB_2930_10Migration : Migration {

            public override void Up() {

                Create.Column("token").OnTable("OPT_MAINTENANCE_ENG").AsString(MigrationUtil.StringMedium).Nullable();
                Create.Column("notes").OnTable("OPT_MAINTENANCE_ENG").AsString(MigrationUtil.StringMax).Nullable();
            }

            public override void Down() {

            }
        }


        /// <summary>
        /// Adding send now column
        /// </summary>
        [Migration(201706021120)]
        public class SWWEB_2930_2985Migration : Migration {

            public override void Up() {
                Create.Column("contractorstartdate").OnTable("OPT_CALLOUT").AsDateTime().Nullable();
            }

            public override void Down() {

            }
        }

        /// <summary>
        ///Changing email column size on callouts and maintenance engineerings
        /// </summary>
        [Migration(201706051800)]
        public class SWWEB_2930_2955Migration : Migration {

            public override void Up() {
                Alter.Column("email").OnTable("OPT_CALLOUT").AsString(MigrationUtil.StringLarge).Nullable();
                Alter.Column("email").OnTable("OPT_MAINTENANCE_ENG").AsString(MigrationUtil.StringLarge).Nullable();
            }

            public override void Down() {

            }
        }

        /// <summary>
        /// Wpnum column on workpackage
        /// </summary>
        [Migration(201706131500)]
        public class SWWEB_2930_3011Migration : Migration {

            public override void Up() {
                Rename.Column("wonum").OnTable("OPT_WORKPACKAGE").To("wpnum");
            }

            public override void Down() {

            }
        }
    }


}
