using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.Util;

namespace softWrench.sW4.Util {
    public class TokenUtil {

        public static string GenerateDateTimeToken(bool shortVersion = false) {

            var token = "" + new Random(100).Next(10000);
            token += DateTime.Now.TimeInMillis().ToString(CultureInfo.InvariantCulture);
            if (shortVersion) {
                token = AuthUtils.GetMd5HashData(token);
            } else {
                token += AuthUtils.GetMd5HashData(token);
            }
            return token;
        }

    }
}
