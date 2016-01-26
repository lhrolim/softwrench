using System;
using FluentMigrator;
using System.Configuration;


namespace softwrench.sw4.manchester.classes.com.cts.manchester.migration {
    [Migration(201503311424)]
    public class Migration20150331Comsw74 : FluentMigrator.Migration
    {
        public override void Up() {
            var key = ConfigurationManager.AppSettings["clientkey"];

            if (key.Equals("manchester")) {
                // Derive path for the SQL script
                var filepath = String.Format("{0}Content\\Customers\\manchester\\scripts\\{1}", AppDomain.CurrentDomain.BaseDirectory, "com_dataimport_com74.sql");
                // Execute script using FluentMigration
                Execute.Script(filepath);
            }
        }

        public override void Down() {
            
        }
    }
}