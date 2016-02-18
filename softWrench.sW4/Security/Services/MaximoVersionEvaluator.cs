using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Services
{
    public class MaximoVersionEvaluator : ISingletonComponent
    {
        public bool IsSCCD() {
            var result = ApplicationConfiguration.IsSCCD();
            return result;
        }
    }
}
