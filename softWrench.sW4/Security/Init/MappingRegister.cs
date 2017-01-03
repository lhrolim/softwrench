using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Mapping;

namespace softWrench.sW4.Security.Init {
    public class MappingRegister : ISWEventListener<ApplicationStartedEvent> {

        private readonly ISWDBHibernateDAO _dao;

        public MappingRegister(ISWDBHibernateDAO dao) {
            _dao = dao;
        }


        public virtual async void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            await Test();
        }

        [Transactional(DBType.Swdb)]
        public virtual async Task Test() {
            var definition = new MappingDefinition {
                Key_ = "maximo.securitygroup.mapping",
                Description = "Mapping between Maximo Security Groups and Softwrench Groups",
                SourceColumnAlias = "Maximo Groups",
                DestinationColumnAlias = "Softwrench Groups"
            };

            var mapping = await _dao.FindSingleByQueryAsync<MappingDefinition>(MappingDefinition.ByKey, "maximo.securitygroup.mapping");
            if (mapping == null) {
                await _dao.SaveAsync(definition);
            }
        }
    }
}
