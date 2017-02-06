using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {


    public class SlaHolderService : ISingletonComponent {
        private const string HoldTime = "ACCUMULATEDHOLDTIME";


        /// <summary>
        /// SWWEB-2920 --> this will set the values on the fly, applying the same logic maximo would, without storing any info. 
        /// The first time the ticket is seen at Maximo these values would get populated
        /// </summary>
        /// <param name="resultObject"></param>
        public virtual void HandleAdjustedTimes(DataMap resultObject) {
            if (resultObject.ContainsAttribute("ADJUSTEDTARGETRESPONSETIME", true) && resultObject.ContainsAttribute("ADJUSTEDTARGETRESOLUTIONTIME", true)) {
                return;
            }


            if (!resultObject.ContainsAttribute("ADJUSTEDTARGETRESPONSETIME", true) && resultObject.ContainsAttribute("TARGETSTART", true)) {
                var targetStart = (DateTime)resultObject.GetAttribute("TARGETSTART");
                resultObject.SetAttribute("ADJUSTEDTARGETRESPONSETIME", targetStart);
            }

          

            if (!resultObject.ContainsAttribute(HoldTime, true)) {
                if (!resultObject.ContainsAttribute("ADJUSTEDTARGETRESOLUTIONTIME", true) && resultObject.ContainsAttribute("TARGETFINISH", true)) {
                    var targetStart = (DateTime)resultObject.GetAttribute("TARGETFINISH");
                    resultObject.SetAttribute("ADJUSTEDTARGETRESOLUTIONTIME", targetStart);
                }
                return;
            }

            var accHoldTime = (double)resultObject.GetAttribute(HoldTime);
            var hours = Math.Truncate(accHoldTime);
            var minutes = Convert.ToInt32(Math.Round(60 * (accHoldTime - Math.Truncate(accHoldTime))));

            if (minutes == 60) {
                //validating edgy cases
                hours++;
                minutes = 0;
            }



            if (!resultObject.ContainsAttribute("ADJUSTEDTARGETRESOLUTIONTIME", true) && resultObject.ContainsAttribute("TARGETFINISH", true) && resultObject.ContainsAttribute(HoldTime, true)) {
                var tagetFinish = (DateTime)resultObject.GetAttribute("TARGETFINISH");
                tagetFinish = tagetFinish.AddHours(hours);
                tagetFinish = tagetFinish.AddMinutes(minutes);
                resultObject.SetAttribute("ADJUSTEDTARGETRESOLUTIONTIME", tagetFinish);
            }
        }

    }
}
