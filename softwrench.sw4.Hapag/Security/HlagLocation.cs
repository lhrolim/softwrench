using System;
using softwrench.sw4.Hapag.Data.Sync;

namespace softwrench.sw4.Hapag.Security {
    public class HlagLocation {
        public String CostCenter { get; set; }
        public String SubCustomer { get; set; }

        public Boolean FromSuperGroup { get; set; }

        public String DashedCostCenter {
            get { return CostCenter == null ? null : CostCenter.Replace('/', '-'); }
        }

        public String SubCustomerSuffix {
            get { return SubCustomer.Split(HapagPersonGroupConstants.PersonGroupPrefix.ToCharArray(), StringSplitOptions.None)[1]; }
        }

        public override string ToString() {
            return string.Format("CostCenter: {0}, SubCustomer: {1}", CostCenter, SubCustomer);
        }


        public HlagLocation CloneForParent() {
            return new HlagLocation() {
                CostCenter = CostCenter,
                FromSuperGroup = true,
                SubCustomer = SubCustomer
            };
        }
    }
}
