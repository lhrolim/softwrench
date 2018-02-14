using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.swgas.classes.swgas.cts.swgas.migration {

    [Migration(201802131720)]
    public class Swweb3394Migration : Migration {


        public override void Up() {
            Create.Table("SWG_DIVISION")
                .WithIdColumn()
                .WithColumn("division").AsString(MigrationUtil.StringSmall)
                .WithColumn("city").AsString(MigrationUtil.StringMedium)
                .WithColumn("state").AsString(MigrationUtil.StringSmall)
                .WithColumn("building").AsString(MigrationUtil.StringMedium);


            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','43rd','AZ','43rd - Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','43rd','AZ','43rd - Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','43rd','AZ','43rd - Auto Shop/Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Central','AZ','Central')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Northern','AZ','Northern - 1st Floor')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Northern','AZ','Northern - 2nd Floor')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Pinnacle Peak','AZ','Pinnacle Peak - Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Pinnacle Peak','AZ','Pinnacle Peak - Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Pinnacle Peak','AZ','Pinnacle Peak - Auto Shop/Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Pinnacle Peak Apartment','AZ','Apt #1237 Pinnacle Peak')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Tempe','AZ','Tempe - Bldg A')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Tempe','AZ','Tempe - Bldg B/Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Tempe','AZ','Tempe - Auto Shop/Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Wickenburg','AZ','Wickenburg - Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CAZ','Wickenburg','AZ','Wickenburg - Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Casa Grande','AZ','Casa Grande - Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Casa Grande-Ops Ctr','AZ','Casa Grande - Operations')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Douglas','AZ','Douglas')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Globe','AZ','Globe')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Morenci','AZ','Morenci')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Oracle','AZ','Oracle')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Sierra Vista','AZ','Sierra Vista')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Tucson','AZ','Tucson - Bldg A')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Tucson','AZ','Tucson - Bldg B')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Tucson','AZ','Tucson - Bldg C')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Tucson','AZ','Tucson - Bldg D')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Willcox','AZ','Willcox')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SAZ','Yuma','AZ','Yuma')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate','NV','Corp - Bldg A')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate','NV','Corp - Bldg B')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate','NV','Corp - Bldg C')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate','NV','Apt #1079 Noble Park')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate','NV','Apt #1103 Noble Park')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate-Hangar','NV','Hanger')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate-Mesa Vista','NV','Mesa Vista')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('CORP','Corporate-Westwood','NV','Westwood')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','Bullhead City','AZ','Bullhead')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','South Ops','NV','South Ops - Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','South Ops','NV','South Ops - Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','South Ops','NV','South Ops - Auto Shop/Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','Cheyenne','NV','Cheyenne')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','North Ops Center','NV','North Ops - Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','North Ops Center','NV','North Ops - Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SNV','North Ops Center','NV','North Ops - Auto Shop/Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Carson City','NV','Carson City - Admin Office')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Carson City','NV','Carson City - Warehouse')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Carson City','NV','Carson City - Auto Shop/Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Carson City','NV','Carson City - Paiute')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Elko','NV','Commercial Street')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Elko','NV','Industrial')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Elko','NV','Elko')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Fallon','NV','Fallon')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Fernley','NV','Fernley')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Incline Village','NV','Incline Village - Incline Ct')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Incline Village','NV','Incline Village - Winding Way')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Lovelock','NV','Lovelock')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','South Lake Tahoe','CA','S. Lake Tahoe')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','South Lake Tahoe','CA','S. Lake Tahoe - House')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Truckee','CA','Truckee')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Winnemucca','NV','Winnemucca - Traders Way')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Winnemucca','NV','Winnemucca - Broken Hill')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('NNV','Yerington','NV','Yerington')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Barstow','CA','Barstow')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Big Bear','CA','Big Bear')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Victorville','CA','Victorville - Admin Bldg')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Victorville','CA','Victorville - Operations Bldg')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Victorville','CA','Victorville - Warehouse 1')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Victorville','CA','Victorville - Warehouse 2')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Victorville','CA','Victorville - Weld Shop')");
            Execute.Sql("Insert into SWG_DIVISION (division,city,state,building) values ('SCA','Victorville','CA','Victorville - Auto Shop')");


        }

        public override void Down() {
            throw new NotImplementedException();
        }
    }
}
