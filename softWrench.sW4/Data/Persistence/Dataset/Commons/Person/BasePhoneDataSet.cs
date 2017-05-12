using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Person {
    public class BasePhoneDataSet : MaximoApplicationDataSet {

        public IEnumerable<AssociationOption> GetPhoneTypeOptions(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT value, description as label FROM alndomain WHERE domainid = 'PHONETYPE'").ToList();
            return rows.Select(row => new AssociationOption(row["value"], row["label"])).ToList();
        }

        public override Task<TargetResult> DoExecute(OperationWrapper wrapper) {
            if (!wrapper.OperationName.EqualsIc(OperationConstants.CRUD_DELETE)) {
                return base.DoExecute(wrapper);
            }

            var data = (CrudOperationData)wrapper.OperationData();
            if (1.Equals(data.GetAttribute("isprimary")) || true.Equals(data.GetAttribute("isprimary"))) {
                var personId = data.GetStringAttribute("personid");
                var count = MaxDAO.FindSingleByNativeQuery<object>("select count(*) from phone where personid = ?", personId);
                if (Int32.Parse(count.ToString()) != 1) {
                    throw new MaximoException("Cannot delete primary phone unless it´s the only entry. Please mark another phone as primary first");
                }

            }

            return base.DoExecute(wrapper);
        }


        public override string ApplicationName() {
            return "phone";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
