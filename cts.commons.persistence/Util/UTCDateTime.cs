using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cts.commons.persistence.Util {


    /// <summary>
    /// used to specify that this date is always to be considered on UTC timezone
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UTCDateTime : Attribute {
    }
}
