using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;

namespace softWrench.sW4.Security.Init.Com {
    class ComProfileInitializer {

        private readonly SWDBHibernateDAO _dao;

        public ComProfileInitializer(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public void SaveProfiles() {
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
                userProfile.Deletable = false;
                _dao.Save(userProfile);
            }
        }
    }
}
