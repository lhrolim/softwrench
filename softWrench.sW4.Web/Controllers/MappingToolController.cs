using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.web.Attributes;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Mapping;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SWControllerConfiguration]
    public class MappingToolController : ApiController {

        private readonly SWDBHibernateDAO _swdbDao;
        private readonly IMappingResolver _mappingResolver;

        public MappingToolController(SWDBHibernateDAO dao, IMappingResolver mappingResolver)
        {
            _swdbDao = dao;
            _mappingResolver = mappingResolver;
        }


        [HttpGet]
        public async Task<IEnumerable<Mapping.Mapping>> LoadMappings(int mappingDefinitionId) {
            return await _swdbDao.FindByQueryAsync<Mapping.Mapping>(Mapping.Mapping.ByMappingDefinition, mappingDefinitionId);
        }


        [HttpPost]
        [Transactional(DBType.Swdb)]
        public virtual async Task<IEnumerable<Mapping.Mapping>> Save(int mappingDefinitionId, string mappingDefinitionKey, [FromBody]IEnumerable<Mapping.Mapping> mappings) {
            var items = new List<Mapping.Mapping>(mappings.Where(m => m.IsValid()));

            foreach (var mapping in items) {
                mapping.MappingDefinition = new MappingDefinition { Id = mappingDefinitionId };
            }

            var result =await _swdbDao.BulkSaveAsync(items);
            _mappingResolver.UpdateCache(mappingDefinitionKey);
            return result;
        }
    }
}
