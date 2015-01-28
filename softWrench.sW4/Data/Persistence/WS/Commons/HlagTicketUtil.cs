using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class HlagTicketUtil {

        public static bool IsIBMTicket(CrudOperationData datamap) {
            return datamap.ContainsAttribute("ownergroup") && 
                datamap.GetAttribute("ownergroup") !=null &&
                !datamap.GetAttribute("ownergroup").ToString().StartsWith("C-");
        }
    }
}
