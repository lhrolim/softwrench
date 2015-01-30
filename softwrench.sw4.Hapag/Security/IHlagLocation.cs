using System;
using System.Collections.Generic;
using softwrench.sw4.Shared2.Data.Association;

namespace softwrench.sw4.Hapag.Security {

    public interface IHlagLocation : IAssociationOption {

        String SubCustomer { get;}
        String SubCustomerSuffix { get; }

        String CostCentersForQuery(string columnName);

        ISet<string> CostCenters { get; }
    }
}