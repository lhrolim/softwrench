using cts.commons.simpleinjector.Events;
using log4net;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Entities.Labor {
    public class UserLaborCraftFetcher : ISWEventListener<UserLoginEvent> {


        private static readonly ILog Log = LogManager.GetLogger(typeof(UserLaborCraftFetcher));

        private readonly EntityRepository _repository;

        private readonly EntityMetadata _entity;

        public UserLaborCraftFetcher(EntityRepository repository) {
            _repository = repository;
            _entity = MetadataProvider.Entity("laborcraftrate",false);

        }


        public void HandleEvent(UserLoginEvent userEvent) {
            if (!MetadataProvider.IsApplicationEnabled("workorder") || _entity == null) {
                Log.DebugFormat("ignoring laborcrafting fetching since application is disabled");
                return;
            }
            var dto = new SearchRequestDto();
            var user = userEvent.InMemoryUser;
            dto.AppendWhereClauseFormat("defaultcraft =1 and laborcode ='{0}'", user.MaximoPersonId);
            var results = _repository.Get(_entity, dto);
            if (results != null && results.Count == 1) {
                var result = results[0];
                var craft = result.GetAttribute("craft");
                var rate = result.GetAttribute("rate");
                user.Genericproperties.Add("defaultcraft", craft);
                user.Genericproperties.Add("defaultcraftrate", rate);
            }
        }
    }
}
