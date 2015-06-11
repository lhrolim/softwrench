using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Web.Models.MyProfile {
    public class MyProfileModel {

        private readonly InMemoryUser _user;
        private readonly List<LocationCostCenterRestriction> _restrictions;
        private readonly bool _canViewRestrictions;
        private readonly bool _canChangeLanguage;

        public String RolesAndFunctions { get; set; }

        public MyProfileModel(InMemoryUser user, List<LocationCostCenterRestriction> locationCostCenterRestrictions, bool canViewRestrictions, bool canChangeLanguage) {
            _user = user;
            _restrictions = locationCostCenterRestrictions;
            _canViewRestrictions = canViewRestrictions;
            _canChangeLanguage = canChangeLanguage;
        }

        public InMemoryUser User {
            get { return _user; }
        }

        public List<LocationCostCenterRestriction> Restrictions {
            get { return _restrictions; }
        }

        public bool CanViewRestrictions {
            get {
                return _canViewRestrictions;
            }
        }
        public bool CanChangeLanguage {
            get {
                return _canChangeLanguage;
            }
        }

    }
}