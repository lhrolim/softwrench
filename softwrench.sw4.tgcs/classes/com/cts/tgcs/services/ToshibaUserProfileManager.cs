using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Mapping;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.services {

    [OverridingComponent(ClientFilters = "tgcs")]
    public class ToshibaUserProfileManager : UserProfileManager {

        private readonly ToshibaAuthUtils _toshibaAuthUtils;


        public ToshibaUserProfileManager(ISWDBHibernateDAO dao, ToshibaAuthUtils toshibaAuthUtils, IMappingResolver mappingResolver)
            : base(dao, mappingResolver) {
            _toshibaAuthUtils = toshibaAuthUtils;
        }


        protected override IEntityRepository EntityRepositoryForTranslation {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<RestEntityRepository>();
            }
        }

        protected override async Task<List<UserProfile>> TranslateProfiles(User dbUser, string personid, List<UserProfile> profiles) {
            var translatePersonId = _toshibaAuthUtils.TranslatePersonId(dbUser.MaximoPersonId);
            return await base.TranslateProfiles(dbUser, translatePersonId, profiles);
        }
    }
}
