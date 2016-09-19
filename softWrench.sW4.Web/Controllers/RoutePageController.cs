using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers {
    public class RoutePageController : ApiController {

        private readonly EntityRepository _entityRepository;
        private readonly IMemoryContextLookuper _lookuper;

        public RoutePageController(EntityRepository entityRepository, IMemoryContextLookuper lookuper) {
            _entityRepository = entityRepository;
            _lookuper = lookuper;
        }

        [HttpGet]
        public IGenericResponseResult PageNotFound() {
            return new GenericResponseResult<RoutePageDTO>(new RoutePageDTO { PageNotFound = true });
        }

        [HttpGet]
        public async Task<IGenericResponseResult> ManyUserIds(string application, string schemaid, string userid) {
            var appMetadata = MetadataProvider.Application(application);
            var entityName = appMetadata.Entity;
            var entityMetadata = MetadataProvider.Entity(entityName);
            var siteIdAttr = entityMetadata.Schema.SiteIdAttribute;

            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");

            var dto = new RoutePageDTO {
                Datamaps = await _entityRepository.GetIdAndSiteIdByUserId(entityMetadata, userid),
                IdFieldName = appMetadata.IdFieldName,
                SiteIdFieldName = siteIdAttr == null ? null : siteIdAttr.Name,
                ApplicationName = application,
                ApplicationTitle = appMetadata.Title,
                ContextPath = fullContext.Context
            };
            return new GenericResponseResult<RoutePageDTO>(dto);
        }

        public class RoutePageDTO {
            public IReadOnlyList<DataMap> Datamaps { get; set; }
            public bool PageNotFound { get; set; }
            public string IdFieldName { get; set; }
            public string SiteIdFieldName { get; set; }
            public string ApplicationName { get; set; }
            public string ApplicationTitle { get; set; }
            public string ContextPath { get; set; }
        }
    }
}