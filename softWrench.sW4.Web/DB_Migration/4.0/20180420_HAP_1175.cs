using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201804201400)]
    public class Migration20180420HAP1175 : FluentMigrator.Migration {

        public override void Up()
        {
            Create.Table("sw_cimapping")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("serviceitvalue").AsString(255).NotNullable()
                .WithColumn("maximovalue").AsString(255).NotNullable();


            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('IP-Address HLAG LAN','IPADDRESS_MANAGEDSYSTEMNAME')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('Serial Number','COMPUTERSYSTEM_SERIALNUMBER')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('Start of Production','SLA_GENERALCIROLE')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('Hardware','COMPUTERSYSTEM_MODEL')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('ÄV/IOV','COMMENT')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('DC, Room','ROOM')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('Rack in Frankfurt','RACK_SERVER_NAME')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('IP-Address TSM-Adapter','IPADDRESS_STRINGNOTATION')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('IP-Address RSA','IPADDRESS_DOTNOTATION')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('IP-Address IP Mgmt','IPADDRESS_ISPLACEHOLDER')  ");
            Execute.Sql("insert into sw_cimapping(serviceitvalue,maximovalue) values ('Region','GEOGRAPHY_REGIONNAME')  ");





        }

        public override void Down() {

        }
    }
}