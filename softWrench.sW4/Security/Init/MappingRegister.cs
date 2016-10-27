using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Mapping;

namespace softWrench.sW4.Security.Init {
    public class MappingRegister : ISWEventListener<ApplicationStartedEvent> {

        private readonly ISWDBHibernateDAO _dao;

        public MappingRegister(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        public async void HandleEvent(ApplicationStartedEvent eventToDispatch) {

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
