using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration.Reports {
    class ChicagoReportProvider {

        private const string COUNT_BY_DEPARTMENT =
@"SELECT CONVERT(char(10), creationdate,126),department,count(department)
from sr 
where creationdate is not null and department is not null
and year(creationdate) = {0}
group by CONVERT(char(10), creationdate,126),department";

        public string GetTicketByDepartmentQuery() {
            return COUNT_BY_DEPARTMENT.Fmt(DateTime.Now.Year);
        }

    }
}
