using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration.Reports {
    class ChicagoReportProvider {

        private const string COUNT_BY_DEPARTMENT =
@"SELECT CONVERT(char(10), creationdate,126) as creationdate,department as department,count(department) as countnumber
from sr 
where creationdate is not null and department is not null
and year(creationdate) = {0}
group by CONVERT(char(10), creationdate,126),department
order by creationdate,department
";

        private const string TicketTypeByDepartment =
@"SELECT department as department,tickettype as tickettype,count(department)as countnumber
from sr 
where tickettype is not null and department is not null
and year(creationdate) = 2016
group by department,tickettype
order by department,tickettype
";

        public static string GetTicketByDepartmentQuery() {
            return COUNT_BY_DEPARTMENT.Fmt(DateTime.Now.Year);
        }

        public static string GetTicketTypeByDepartment() {
            return TicketTypeByDepartment.Fmt(DateTime.Now.Year);
        }

    }
}
