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

        public IGenericResponseResult CreateNewFilter([NotNull]String application, string schema, string fields, string operators, string values,string template, [NotNull]string alias) {
            Validate.NotNull(application, "application");
            Validate.NotNull(fields, "fields");
            Validate.NotNull(operators, "operators");
            Validate.NotNull(values, "values");
            Validate.NotNull(alias, "alias");

            var association = _gridFilterManager.CreateNewFilter(SecurityFacade.CurrentUser(), application, fields, operators, values,template, alias, schema);
            return new GenericResponseResult<GridFilterAssociation>(association) {
                SuccessMessage = "Filter {0} created successfully".Fmt(alias)
            };
        }

        public IGenericResponseResult UpdateFilter(int? id, string alias, string fields, string operators, string values,string template) {
            Validate.NotNull(fields, "fields");
            Validate.NotNull(operators, "operators");
            Validate.NotNull(values, "values");
            Validate.NotNull(id, "id");

            var filter = _gridFilterManager.UpdateFilter(SecurityFacade.CurrentUser(), fields, alias, operators, values,template, id);
            return new GenericResponseResult<GridFilter>(filter) {
                SuccessMessage = "Filter {0} updated successfully".Fmt(filter.Alias)
            };
        }

        [HttpPost]
        public IGenericResponseResult DeleteFilter(int? filterId, int? creatorId) {
            Validate.NotNull(filterId, "filterId");
            var association = _gridFilterManager.DeleteFilter(SecurityFacade.CurrentUser(), filterId, creatorId);
            return new GenericResponseResult<GridFilterAssociation>(association) {
                SuccessMessage = "Filter {0} removed successfully".Fmt(association.Filter.Alias)
            };
        }

    }
}
