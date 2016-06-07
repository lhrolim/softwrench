using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class SolutionsHandler {


        public static void HandleSolutions(CrudOperationData crudDataEntity, object sr) {
            var sympton = crudDataEntity.GetStringAttribute("symptom_.ldtext");
            var cause = crudDataEntity.GetStringAttribute("cause_.ldtext");
            var resolution = crudDataEntity.GetStringAttribute("resol_.ldtext");
            if (resolution == null) {
                resolution = crudDataEntity.GetStringAttribute("resolution_.ldtext");
            }
            if (string.IsNullOrEmpty(sympton) && string.IsNullOrEmpty(cause) && string.IsNullOrEmpty(resolution)) {
                return;
            }
            WsUtil.SetValue(sr, "FR1CODE_LONGDESCRIPTION", cause);
            WsUtil.SetValue(sr, "FR2CODE_LONGDESCRIPTION", resolution);
            WsUtil.SetValue(sr, "PROBLEMCODE_LONGDESCRIPTION", sympton);
        }

    }
}
