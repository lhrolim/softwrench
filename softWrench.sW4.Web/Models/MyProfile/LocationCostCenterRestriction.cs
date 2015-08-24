using System.Linq;

namespace softWrench.sW4.Web.Models.MyProfile {
    public class LocationCostCenterRestriction {
        private readonly string _customer;
        private readonly string _customerDescription;
        private readonly string _costCenter;
        private readonly string _costCenterDescription;

        public LocationCostCenterRestriction(string customer, string customerDescription, string costCenter, string costCenterDescription) {
            _customer = customer;
            _customerDescription = customerDescription;
            _costCenter = costCenter;
            _costCenterDescription = costCenterDescription;
        }

        public string Customer {
            get { return _customer.Split('-').LastOrDefault(); }
        }

        public string CustomerDescription {
            get { return _customerDescription; }
        }

        public string CostCenter {
            get { return _costCenter; }
        }

        public string CostCenterDescription {
            get { return _costCenterDescription; }
        }

        public bool IsValidRestriction() {
            return !string.IsNullOrEmpty(_customer) && !string.IsNullOrEmpty(_costCenter);
        }
    }
}