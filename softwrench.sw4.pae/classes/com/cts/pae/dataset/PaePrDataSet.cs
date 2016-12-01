using System;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softwrench.sw4.pae.classes.com.cts.pae.dataset {
    public class PaePrDataSet : MaximoApplicationDataSet {
        public override TargetResult DoExecute(OperationWrapper operationWrapper) {
            var json = operationWrapper.JSON;
            var operationData = operationWrapper.GetOperationData;
            operationData.Holder.Add("NP_STATUSMEMO", json["memo"].ToObject<string>());
            return Engine().Execute(operationWrapper);
        }

        public override string ApplicationName() {
            return "pr";
        }

        public override string ClientFilter() {
            return "pae";
        }
    }
}
