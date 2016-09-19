using System;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.Hapag.Data.Init {
    class HapagProfileInitializer {

        private readonly SWDBHibernateDAO _dao;

        public HapagProfileInitializer(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public void SaveProfiles(HapagRoleInitializer.RoleSaveResult roles) {
            var profileTypes = Enum.GetValues(typeof(ProfileType)).Cast<ProfileType>();
            foreach (var profileType in profileTypes) {
                var profileName = profileType.GetName();
                var userProfile = _dao.FindSingleByQuery<UserProfile>(UserProfile.UserProfileByName, profileName) ?? new UserProfile {
                    Name = profileName,
                    Description = profileType.GetDescription(),
                    Deletable = false
                };
                if (string.IsNullOrEmpty(userProfile.Description)) {
                    userProfile.Description = profileType.GetDescription();
                }
                AssociateRoles(profileType, userProfile, roles.OrdinaryRoles);
                userProfile.Deletable = false;
                _dao.Save(userProfile);
            }
        }

        private void AssociateRoles(ProfileType profileType, UserProfile userProfile, IDictionary<RoleType, Role> ordinaryRoles) {
            switch (profileType) {
                case ProfileType.Itc:
                    userProfile.Roles = new LinkedHashSet<Role>(){
                        ordinaryRoles[RoleType.Incident],
                        ordinaryRoles[RoleType.Asset],
                        ordinaryRoles[RoleType.Faq],
                        ordinaryRoles[RoleType.ITCSearch],
                        ordinaryRoles[RoleType.ImacGrid],
                        ordinaryRoles[RoleType.NewImac],
                        ordinaryRoles[RoleType.TapeBackupReport],
                        ordinaryRoles[RoleType.HardwareRepairReport],
                        ordinaryRoles[RoleType.IncidentDetailsReport],
                        ordinaryRoles[RoleType.IncidentPerLocationReport],
                        ordinaryRoles[RoleType.AssetCategoriesReport],
                        ordinaryRoles[RoleType.ITCReport]
                    };
                    break;
            }
        }
    }
}
