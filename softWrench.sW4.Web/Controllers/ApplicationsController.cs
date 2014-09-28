using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Web.Http;
using System.Web.Security;
using JetBrains.Annotations;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Menu.Containers;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Web.Controllers
{
    /// <summary>
    /// Deprecated for MenuController, kept here for compatibility issues with Ipad Client
    /// </summary>
    [Authorize]
    public class ApplicationsController : ApiController
    {
        [NotNull]
        public IEnumerable<object> Get(ClientPlatform platform)
        {
            try
            {
                InMemoryUser user = SecurityFacade.CurrentUser();
                return from application in user.Applications(platform)
                       select new { application.Id, application.Name, application.Title };
            }
            catch (InvalidOperationException)
            {
                FormsAuthentication.SignOut();
                return Enumerable.Empty<object>();
            }
         
        }

        [NotNull]
        public ApplicationMetadata Get(string application, ClientPlatform platform) {
            var user = SecurityFacade.CurrentUser();


            if (null == user) {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            ApplicationMetadata metadata;

            try {
                metadata = MetadataProvider
                    .Application(application)
                    .ApplyPolicies(new ApplicationMetadataSchemaKey(null,null,platform), user);
            }
            catch (SecurityException) {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            return metadata;
        }

    
    }
}

