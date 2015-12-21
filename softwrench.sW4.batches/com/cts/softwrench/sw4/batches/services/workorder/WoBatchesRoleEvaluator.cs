using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {




    /// <summary>
    ///  this class implements https://controltechnologysolutions.atlassian.net/browse/SWWEB-529 checks
    /// 
    ///Users have to pass the following checks to have permissions to run batches (see batches?). If any of those fail an appropriate error message is shown.
    ///The user must have a row in the MAXUSER table Maximo.
    ///Error message: "You cannot use the Work Order Closure application without a valid login for Maximo."
    ///The user status must be ACTIVE.
    ///Error message: "You cannot use the Work Order Closure application without an Active login for Maximo."
    ///The user must have a default site.
    ///Error message: "You cannot use the Work Order Closure application without a Default Site identified in Maximo."
    ///The user must have one or more of the MAXGROUP.GROUPNAMEs listed in this table.
    ///Error message: "You cannot use the Work Order Closure application without being a member of a Maximo group with Work Order update privilages."
    /// 
    /// </summary>
    public class WoBatchesRoleEvaluator : AfterLoginRoleEvaluator {

        private const string Group1 = "MAXIMO.MXWMWOM";
        /// TODO: This needs to be set back to MAXIMO.MXWMPTLA once there is a maximo persongroup with that name
        /// private const string Group2 = "MAXIMO.MXWMPTLA";
        private const string Group2 = "MXWMPTLA";
        private const string Group3 = "MAXIMO.MXWMFMGR";
        private const string Maintenance = "MAXIMO.MXWMMNTN";
        private const string Operations = "MAXIMO.MXWMOPER";
        private const string Planer = "MAXIMO.MXWMPLAN";
        private const string Scheduler = "MAXIMO.MXWMSCHD";
        private const string Admin = "MAXIMO.MXWMADMN";
        private const string Feedback = "MAXIMO.MXWMFDBK";
        private const string Workorder = "MAXIMO.MXWORKORDER";
        private const string WoTargetUpdate = "MAXIMO.MXWMWOTGT";
        private const string WoUpdateTargetDates = "MAXIMO.MXWMWOTGTDATE";
        private const string WorkorderUpdate = "MAXIMO.MXWMWOUP";

        private readonly MaximoHibernateDAO _dao;

        private const string RoleNameCnst = "allowwobatches";

        public WoBatchesRoleEvaluator(MaximoHibernateDAO dao) {
            _dao = dao;
        }

        private static readonly ISet<String> GroupsToCheck = new HashSet<string>()
        {
            Group1,
            Group2,
            Group3,
            Maintenance,
            Operations,
            Planer,
            Scheduler,
            Admin,
            Feedback,
            Workorder,
            WoTargetUpdate,
            WoUpdateTargetDates,
            WorkorderUpdate
        };

        private readonly RoleWithErrorMessage _noUserRole = new RoleWithErrorMessage { Name = RoleNameCnst, Authorized = false, UnauthorizedMessage = "You cannot use the Work Order Closure application without a valid login for Maximo." };
        private readonly RoleWithErrorMessage _notActiveRole = new RoleWithErrorMessage { Name = RoleNameCnst, Authorized = false, UnauthorizedMessage = "You cannot use the Work Order Closure application without an Active login for Maximo." };
        private readonly RoleWithErrorMessage _noDefSite = new RoleWithErrorMessage { Name = RoleNameCnst, Authorized = false, UnauthorizedMessage = "You cannot use the Work Order Closure application without a Default Site identified in Maximo." };
        private readonly RoleWithErrorMessage _noMatchingGroup = new RoleWithErrorMessage { Name = RoleNameCnst, Authorized = false, UnauthorizedMessage = "You cannot use the Work Order Closure application without being a member of a Maximo group with Work Order update privilages." };

        public override string RoleName { get { return RoleNameCnst; } }
        public override Role Eval(InMemoryUser user) {
            if (user.IsSwAdmin() && ApplicationConfiguration.IsLocal() || !ApplicationConfiguration.IsClient("tva")) {
                return null;
            }

            if (user.MaximoPersonId == null) {
                return _noUserRole;
            }
            //we could optimize the left join here to do a in on the group names, but I guess that doesnt worth 
            var list = _dao.FindByNativeQuery("select u.status,u.defsite,p.persongroup from maxuser u left join persongroupview p on u.personid = p.personid where u.personid = ?", user.MaximoPersonId);
            if (!list.Any()) {
                return _noUserRole;
            }
            var matchedGroup = false;
            foreach (var row in list) {
                if (row["status"] == null || (!row["status"].EqualsIc("active"))) {
                    return _notActiveRole;
                }
                if (row["defsite"] == null) {
                    return _noDefSite;
                }
                if (row["persongroup"] != null && GroupsToCheck.Contains(row["persongroup"])) {
                    matchedGroup = true;
                    break;
                }
            }

            if (!matchedGroup) {
                return _noMatchingGroup;
            }

            return new RoleWithErrorMessage { Name = RoleName, Authorized = true };
        }
    }
}
