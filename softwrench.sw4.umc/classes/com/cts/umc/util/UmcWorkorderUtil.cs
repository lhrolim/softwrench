using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.umc.classes.com.cts.umc.util {
    public class UmcWorkorderUtil {

        public static void PopulateDefaultValues(AttributeHolder dm) {
            dm.SetAttribute("worktype", "RO");
            dm.SetAttribute("classstructureid", "1005");
            dm.SetAttribute("wopriority", "3");
        }

    }
}
