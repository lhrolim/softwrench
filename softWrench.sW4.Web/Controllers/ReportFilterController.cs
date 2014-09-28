using JetBrains.Annotations;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace softWrench.sW4.Web.Controllers {

    [System.Web.Mvc.Authorize]
    public class ReportFilterController : ApiController {

        private readonly DataController _dataController;
        private readonly IConfigurationFacade _configFacade;
        private IWhereClauseFacade _whereClauseFacade;
        private readonly I18NResolver _i18Nresolver;


        public ReportFilterController(DataController dataController, IConfigurationFacade configFacade, IWhereClauseFacade whereClauseFacade, I18NResolver i18Nresolver) {
            _dataController = dataController;
            _configFacade = configFacade;
            _whereClauseFacade = whereClauseFacade;
            _i18Nresolver = i18Nresolver;
        }

        public IApplicationResponse Get(string application, [FromUri] DataRequestAdapter request) {
            var user = SecurityFacade.CurrentUser();

            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var response = _dataController.Get(application, request);

            return response;
        }
    }
}
