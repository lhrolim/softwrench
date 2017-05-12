﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Person {
    public class BaseEmailDataSet : MaximoApplicationDataSet {

        public IEnumerable<AssociationOption> GetEmailTypeOptions(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT value, description as label FROM alndomain WHERE domainid = 'EMAILTYPE'").ToList();
            return rows.Select(row => new AssociationOption(row["value"], row["label"])).ToList();
        }


        public override Task<TargetResult> DoExecute(OperationWrapper wrapper) {
            if (!wrapper.OperationName.EqualsIc(OperationConstants.CRUD_DELETE)) {
                return base.DoExecute(wrapper);
            }

            var data = (CrudOperationData)wrapper.OperationData();
            if (1.Equals(data.GetAttribute("isprimary")) || true.Equals(data.GetAttribute("isprimary"))) {
                var personId = data.GetStringAttribute("personid");
                var count = MaxDAO.FindSingleByNativeQuery<object>("select count(*) from email where personid = ?", personId);
                if (Int32.Parse(count.ToString()) != 1) {
                    throw new MaximoException("Cannot delete primary email unless it´s the only entry. Please mark another email as primary first");
                }

            }

            return base.DoExecute(wrapper);
        }

        public override string ApplicationName() {
            return "email";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
