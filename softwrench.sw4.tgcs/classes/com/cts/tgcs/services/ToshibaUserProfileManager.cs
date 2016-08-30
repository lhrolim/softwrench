using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Mapping;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.services {

    [OverridingComponent(ClientFilters = "tgcs")]
    public class ToshibaUserProfileManager : UserProfileManager {

        private readonly RestEntityRepository _restEntityRepository;
        private readonly EntityMetadata _entityMetadata;
        private readonly ToshibaAuthUtils _toshibaAuthUtils;

        private readonly IMappingResolver _mappingResolver;

        public ToshibaUserProfileManager(ISWDBHibernateDAO dao, RestEntityRepository restEntityRepository, ToshibaAuthUtils toshibaAuthUtils, IMappingResolver mappingResolver) : base(dao) {
            _restEntityRepository = restEntityRepository;
            _toshibaAuthUtils = toshibaAuthUtils;
            _mappingResolver = mappingResolver;
            _entityMetadata = MetadataProvider.Entity("groupuser");
        }

        //TODO: verify whether it makes sense also to clean up profiles that were associated on SWDB, or even to forbid such association
        public override List<UserProfile> FindUserProfiles(User dbUser) {
            var profiles = base.FindUserProfiles(dbUser);

            SearchRequestDto dto = new PaginatedSearchRequestDto();
            var translatePersonId = _toshibaAuthUtils.TranslatePersonId(dbUser.MaximoPersonId);
            dto.AppendSearchEntry("USERID", translatePersonId);
            try {
                Log.DebugFormat("Fetching user profiles from ISM for person {0} ", translatePersonId);
                var groups = _restEntityRepository.Get(_entityMetadata, dto);

                var groupNames = groups.Select(g => g.GetStringAttribute("GROUPNAME"));

                var swGroupNames = _mappingResolver.Resolve("tgcs.authentication", groupNames);
                profiles.AddRange(swGroupNames.Select(FindByName).Where(profile => profile != null));

                return profiles;
            } catch (Exception e) {
                Log.Error("error contacting ISM for fetching profiles. Returning only user associated profiles", e);
                return profiles;
            }





        }
    }
}
