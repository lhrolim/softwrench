using System;
using FluentMigrator;
using FluentMigrator.Runner.Extensions;


namespace softwrench.sw4.manchester.classes.com.cts.manchester.migration {
    [Migration(201503311424)]
    public class Migration20150331Comsw74 : FluentMigrator.Migration
    {
        public override void Up() {
            // Insert swdashboard into user table
            Insert.IntoTable("SW_USER2")
                .WithIdentityInsert()
                .Row(
                    new {
                        id = 3,
                        username = "swdashboard",
                        firstname = "dashboard",
                        lastname = "dashboard",
                        isactive = 1
                    });

            // Insert user profile laborers
            Insert.IntoTable("SW_USERPROFILE")
                .WithIdentityInsert()
                .Row(
                    new {
                        id = 16,
                        name = "Laborers",
                        deletable = "0",
                        description = "Laborers"
                    });

            // Insert a new dashboard
            Insert.IntoTable("DASH_DASHBOARD")
                .WithIdentityInsert()
                .Row(
                    new {
                        id = 1,
                        layout = "1,1",
                        title = "SRs and WOs",
                        createdby = 3,
                        creationdate = DateTime.Now,
                        updatedate = DateTime.Now
                    });

            // Insert active SR and WO widgets
            Insert.IntoTable("DASH_BASEPANEL")
                .WithIdentityInsert()
                .Row(
                    new {
                        id = 1,
                        alias_ = "ActiveSRs",
                        title = "Active Service Requests",
                        createdby = 3,
                        creationdate = DateTime.Now,
                        updatedate = DateTime.Now
                    })
                .Row(
                    new {
                        id = 2,
                        alias_ = "ActiveWOs",
                        title = "Active Work Orders",
                        createdby = 3,
                        creationdate = DateTime.Now,
                        updatedate = DateTime.Now
                    });

            Insert.IntoTable("DASH_GRIDPANEL")
                .Row(
                    new {
                        gpid = 1,
                        application = "servicerequest",
                        schemaref = "list",
                        defaultsortfield = "ticketid"
                    })
                .Row(
                    new {
                        gpid = 2,
                        application = "workorder",
                        schemaref = "list",
                        defaultsortfield = "wonum"
                    });

            // Define layout
            Insert.IntoTable("DASH_DASHBOARDREL")
                .Row(
                    new {
                        position = 0,
                        panel_id = 2,
                        dashboard_id = 1
                    })
                .Row(
                    new {
                        position = 0,
                        panel_id = 1,
                        dashboard_id = 1
                    });

            // Define where clause
            Insert.IntoTable("CONF_CONDITION")
                .WithIdentityInsert()
                .Row(
                    new {
                        id = 2,
                        alias_ = "active_sr_dashboard",
                        fullkey = @"/_whereclauses/servicerequest/",
                        global = 0
                    })
                .Row(
                    new {
                        id = 3,
                        alias_ = "active_wo_dashboard",
                        fullkey = @"/_whereclauses/workorder/",
                        global = 0
                    });

            Insert.IntoTable("CONF_WCCONDITION")
                .Row(
                    new {
                        wcwcid = 2,
                        metadataid = "ActiveSRs",
                    })
                .Row(
                    new {
                        wcwcid = 3,
                        schema_ = "list",
                        metadataid = "ActiveWOs"
                    });

            Insert.IntoTable("CONF_PROPERTYDEFINITION")
                .Row(
                    new {
                        fullkey = @"/_whereclauses/servicerequest/whereclause",
                        key_ = "whereclause",
                        defaultvalue = "",
                        datatype = "String",
                        renderer = "whereclause",
                        visible = 1,
                        contextualized = 1,
                        alias_ = ""
                    })
                .Row(
                    new {
                        fullkey = @"/_whereclauses/workorder/whereclause",
                        key_ = "whereclause",
                        defaultvalue = "",
                        datatype = "String",
                        renderer = "whereclause",
                        visible = 1,
                        contextualized = 1,
                        alias_ = ""
                    }); 

            Insert.IntoTable("CONF_PROPERTYVALUE")
                .Row(
                    new {
                      value = @"((status in ('INPROG','SR WO COMP','WO CREATED' ,'NEW','PENDING','QUEUED'))) and ((owner = @username  or upper(reportedby) = @username ) or ((ownergroup is not null) and ownergroup  in (select persongroup from persongroupteam where respparty = @username ))) and siteid = 'DPW'",  
                      definition_id = @"/_whereclauses/servicerequest/whereclause",
                      condition_id = 2
                    })
                .Row(
                    new {
                      value = @"((workorder.status in ('APPR','BASE','FIELD WORK COMP','NEW','QUEUED','INPRG','INV','POSTPONED','RFI','SAW','WAPPR','WMATL','WPCOND','WSCH')) and ((owner = 'swadmin' or upper(reportedby) = 'swadmin') or ((ownergroup is not null) and ownergroup  in (select persongroup from persongroupteam where respparty = 'swadmin'))) and historyflag = 0 and istask = 0 and workorder.siteid = 'DPW')",  
                      definition_id = @"/_whereclauses/workorder/whereclause",
                      condition_id = 3
                    });
        }

        public override void Down() {
            
        }
    }
}