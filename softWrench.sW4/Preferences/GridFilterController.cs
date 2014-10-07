using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using softWrench.sW4.Data.API;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Preferences {

    public class GridFilterController : ApiController {

        private readonly GridFilterManager _gridFilterManager;

        public GridFilterController(GridFilterManager gridFilterManager) {
            _gridFilterManager = gridFilterManager;
        }

        public IGenericResponseResult CreateNewFilter([NotNull]String application, string schema, [NotNull]string queryString, [NotNull]string alias) {
            Validate.NotNull(application,"application");
            Validate.NotNull(queryString,"queryString");
            Validate.NotNull(alias,"alias");

            var association = _gridFilterManager.CreateNewFilter(SecurityFacade.CurrentUser(), application, queryString, alias, schema);
            return new GenericResponseResult<GridFilterAssociation>(association) {
                SuccessMessage = "Filter {0} created successfully".Fmt(alias)
            };
        }

    }
}
