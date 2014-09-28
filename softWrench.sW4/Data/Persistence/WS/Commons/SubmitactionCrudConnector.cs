using softWrench.sW4.Data.Persistence.WS.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Util;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.SimpleInjector;
using System.Windows.Controls;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    /// <summary>
    /// this code is called from sr_service.js to change status of 
    /// </summary>
    class SubmitactionCrudConnector : BaseMaximoCustomConnector {
        public partial class UpdateStatusOperationData : CrudOperationDataContainer {
            //[NotNull]
            //public string ticketid;
            [NotNull]
            public string status;

        }

        public Object SubmitAction(UpdateStatusOperationData opData) {
            opData.CrudData.Attributes["status"] = opData.status;
            opData.CrudData.Attributes["#submittingaction"] = "true";
            return Maximoengine.Update(opData.CrudData);
        }
    }
}
