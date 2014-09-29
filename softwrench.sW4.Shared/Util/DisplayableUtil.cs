using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sW4.Shared.Util {
    class DisplayableUtil {
        public static IList<T> GetDisplayable<T>(Type displayableType,IEnumerable<IApplicationDisplayable>originalDisplayables ) {
            var resultingDisplayables = new List<T>();
            foreach (IApplicationDisplayable displayable in originalDisplayables) {
                if (displayable.GetType() == displayableType) {
                    resultingDisplayables.Add((T)displayable);
                }
            }
            return resultingDisplayables;
        }
    }
}
