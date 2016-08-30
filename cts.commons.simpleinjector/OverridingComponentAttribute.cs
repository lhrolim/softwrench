using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cts.commons.simpleinjector {

    [AttributeUsage(AttributeTargets.Class)]
    public class OverridingComponentAttribute :ComponentAttribute{
    }
}
