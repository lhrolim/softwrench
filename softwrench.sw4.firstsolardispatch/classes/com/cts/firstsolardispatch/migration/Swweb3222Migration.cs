using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {
    [Migration(201711271700)]
    public class Swweb3222Migration : Migration {

        public override void Up()
        {

            Alter.Table("DISP_TICKET").AddColumn("arrivedtime").AsDateTime().Nullable();
            Alter.Table("DISP_TICKET").AddColumn("lastsent").AsDateTime().Nullable();


            Execute.Sql(
@"
create view disp_countdown as
select 
d.id,
case when exists((select 1 from DISP_INVERTER where ticketid = d.id and failureclass = '1' and d.status not in ('ARRIVED','RESOLVED'))) then DATEDIFF(hour,GetDate(),DateAdd(HOUR,24,dispatchexpecteddate)) 
when exists((select 1 from DISP_INVERTER where ticketid = d.id and failureclass = '1' and d.status in ('ARRIVED','RESOLVED'))) then DATEDIFF(hour,d.arrivedtime,DateAdd(HOUR,24,dispatchexpecteddate)) 
when exists((select 1 from DISP_INVERTER where ticketid = d.id and failureclass = '2' and d.status not in ('ARRIVED','RESOLVED'))) then DATEDIFF(hour,GetDate(),DateAdd(HOUR,48,dispatchexpecteddate))  
when exists((select 1 from DISP_INVERTER where ticketid = d.id and failureclass = '2' and d.status in ('ARRIVED','RESOLVED'))) then DATEDIFF(hour,d.arrivedtime,DateAdd(HOUR,48,dispatchexpecteddate)) end as countdown
from disp_ticket d where dispatchexpecteddate is not null");
        }

        public override void Down() {

        }

    }
}
