using FluentMigrator;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201712051641)]
    public class Swweb3285Migration : Migration {

        public override void Up() {

            if (!ApplicationConfiguration.IsLocal()) {
                return;
            }

            Execute.Sql("INSERT INTO dbo.GFED_SITE (gfedid,facilityname,facilitytitle,locationprefix,siteid,orgid,address,city,state,postalcode,country,sitecontact,sitecontactphone,supportphone,primarycontact,primarycontactphone,escalationcontact,escalationcontactphone,gpslatitude,gpslongitude,primarycontactemail,escalationcontactemail,maintenaceprovider,wherehouseaddress,singlelineaddress,primarycontactsmsemail,escalationcontactsmsemail) VALUES (1,'BLY1','Blythe 1 - First Solar PPA','BLY','1803','ORG6','19151 15th Avenue','Blythe','CA','92225','USA','Test Contact','(000); 000 0000','(000); 000 0000','Test Primary Contact','(000); 000 0000','Test Escalation Contact','(000); 000 0000',33.592104,-114.742250,null,null,null,null,null,null,null)");
            Execute.Sql("INSERT INTO dbo.GFED_SITE (gfedid,facilityname,facilitytitle,locationprefix,siteid,orgid,address,city,state,postalcode,country,sitecontact,sitecontactphone,supportphone,primarycontact,primarycontactphone,escalationcontact,escalationcontactphone,gpslatitude,gpslongitude,primarycontactemail,escalationcontactemail,maintenaceprovider,wherehouseaddress,singlelineaddress,primarycontactsmsemail,escalationcontactsmsemail) VALUES (21752,'AVV1','Avra Valley','AVV','1801','ORG4','9602 N. Garvey Rd.,','Marana','AZ','85653','USA','Test Contact','(000); 000 0000','(000); 000 0000','Test Primary Contact','(000); 000 0000','Test Escalation Contact','(000); 000 0000',33.592104,-114.742250,null,null,null,null,null,null,null)");
            Execute.Sql("INSERT INTO dbo.GFED_SITE (gfedid,facilityname,facilitytitle,locationprefix,siteid,orgid,address,city,state,postalcode,country,sitecontact,sitecontactphone,supportphone,primarycontact,primarycontactphone,escalationcontact,escalationcontactphone,gpslatitude,gpslongitude,primarycontactemail,escalationcontactemail,maintenaceprovider,wherehouseaddress,singlelineaddress,primarycontactsmsemail,escalationcontactsmsemail) VALUES (103956,'AES1','Adams East Solar','AES','1803','ORG6','32111 W. South Ave.','Mendota','CA','93460','USA','Test Contact','(000); 000 0000','(000); 000 0000','Test Primary Contact','(000); 000 0000','Test Escalation Contact','(000); 000 0000',33.592104,-114.742250,null,null,null,null,null,null,null)");
            Execute.Sql("INSERT INTO dbo.GFED_SITE (gfedid,facilityname,facilitytitle,locationprefix,siteid,orgid,address,city,state,postalcode,country,sitecontact,sitecontactphone,supportphone,primarycontact,primarycontactphone,escalationcontact,escalationcontactphone,gpslatitude,gpslongitude,primarycontactemail,escalationcontactemail,maintenaceprovider,wherehouseaddress,singlelineaddress,primarycontactsmsemail,escalationcontactsmsemail) VALUES (1,'PDS','PDS 11','BLY','FSDP','DPORG','19151 15th Avenue','Blythe','CA','92225','USA','Test Contact','(000); 000 0000','(000); 000 0000','Test Primary Contact','(000); 000 0000','Test Escalation Contact','(000); 000 0000',33.592104,-114.742250,null,null,null,null,null,null,null)");
            Execute.Sql("INSERT INTO dbo.GFED_SITE (gfedid,facilityname,facilitytitle,locationprefix,siteid,orgid,address,city,state,postalcode,country,sitecontact,sitecontactphone,supportphone,primarycontact,primarycontactphone,escalationcontact,escalationcontactphone,gpslatitude,gpslongitude,primarycontactemail,escalationcontactemail,maintenaceprovider,wherehouseaddress,singlelineaddress,primarycontactsmsemail,escalationcontactsmsemail) VALUES (1,'PDS','PDS 11','BLY','FSDP','DPORG','19151 15th Avenue','Blythe','CA','92225','USA','Test Contact','(000); 000 0000','(000); 000 0000','Test Primary Contact','(000); 000 0000','Test Escalation Contact','(000); 000  0000',33.592104,-114.742250,null,null,null,null,null,null,null)");
            Execute.Sql("INSERT INTO dbo.GFED_SITE (gfedid,facilityname,facilitytitle,locationprefix,siteid,orgid,address,city,state,postalcode,country,sitecontact,sitecontactphone,supportphone,primarycontact,primarycontactphone,escalationcontact,escalationcontactphone,gpslatitude,gpslongitude,primarycontactemail,escalationcontactemail,maintenaceprovider,wherehouseaddress,singlelineaddress,primarycontactsmsemail,escalationcontactsmsemail) VALUES (1,'PDS','PDS 11','BLY','FSDP','DPORG','19151 15th Avenue','Blythe','CA','92225','USA','Test Contact','(000); 000 00','(000); 000 0000','Test Primary Contact','(000); 000 0000','Test Escalation Contact','(000); 000  0000',33.592104,-114.742250,null,null,null,null,null,null,null)");
        }

        public override void Down() {

        }

    }
}
