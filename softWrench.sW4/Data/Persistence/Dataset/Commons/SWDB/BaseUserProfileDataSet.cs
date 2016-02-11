using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.Util;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Init;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB {
    class BaseUserProfileDataSet : SWDBApplicationDataset {

        private UserProfileManager _userProfileManager;

        public BaseUserProfileDataSet(UserProfileManager userProfileManager) {
            _userProfileManager = userProfileManager;
        }


        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var profileDatamap = result.ResultObject;

            var profileOb = new UserProfile() {
                Roles = new HashedSet<Role>()
            };

            if (profileDatamap.GetAttribute("id") != null) {
                //not a creation
                profileOb = _userProfileManager.FindByName(profileDatamap.GetAttribute("name") as string);
            }


            HandleBasicRoles(profileOb, profileDatamap);



            return result;
        }

        public IEnumerable<IAssociationOption> GetTopLevelApplications(OptionFieldProviderParameters parameters) {
            return MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, null).Select(a => new AssociationOption(a.ApplicationName, a.ApplicationName));
        }


        public IEnumerable<IAssociationOption> GetSchemas(OptionFieldProviderParameters parameters) {
            var entity = parameters.OriginalEntity;
            var mode = entity.GetStringAttribute("#selectedmode");
            var application = entity.GetStringAttribute("application");
            SchemaPermissionMode schemaMode;
            Enum.TryParse(mode, out schemaMode);
            var completeApplicationMetadataDefinition = MetadataProvider.Application(application);
            if (SchemaPermissionMode.View.Equals(schemaMode)) {
                var outputSchemas = completeApplicationMetadataDefinition.Schemas().Values.Where(s => s.Mode == SchemaMode.output);
                var applicationSchemaDefinitions = outputSchemas as ApplicationSchemaDefinition[] ?? outputSchemas.ToArray();
                if (applicationSchemaDefinitions.Any() && applicationSchemaDefinitions.Count() > 1) {
                    //if there are output schemas present, use them
                    return applicationSchemaDefinitions.Select(o => new AssociationOption(o.SchemaId, o.Title));
                }

                


            }
            return null;
        }

        public IEnumerable<IAssociationOption> GetSelectableModes(OptionFieldProviderParameters parameters) {
            var entity = parameters.OriginalEntity;
            var fullControl = entity.GetAttribute("#appfullcontrol");
            var allowCreation = entity.GetAttribute("#allowCreation");
            var allowUpdate = entity.GetAttribute("#appfullcontrol");
            var allowRemoval = entity.GetAttribute("#appfullcontrol");
            //TODO: include logic based on permissions
            return Enum.GetValues(typeof(SchemaPermissionMode)).Cast<SchemaPermissionMode>().Select(i => new PriorityBasedAssociationOption(i.GetName(), i.Label(), i.Priority())).OrderBy(a => a.Priority);
        }


        public IEnumerable<IAssociationOption> GetSelectableTabs(OptionFieldProviderParameters parameters) {
            var entity = parameters.OriginalEntity;

            return Enum.GetValues(typeof(SchemaPermissionMode)).Cast<SchemaPermissionMode>().Select(i => new PriorityBasedAssociationOption(i.GetName(), i.Label(), i.Priority())).OrderBy(a => a.Priority);
        }


        private void HandleBasicRoles(UserProfile profileOb, DataMap profileDatamap) {
            var roles = SWDAO.FindByQuery<Role>("from Role order by name");

            var basicRoles = new List<IDictionary<string, object>>();
            foreach (var role in roles) {
                IDictionary<string, object> dict = new Dictionary<string, object>();
                dict["name"] = role.Name;
                dict["description"] = role.Description;
                dict["id"] = role.Id;
                dict["_#selected"] = profileOb.Roles.Any(r => r.Id == role.Id);
                basicRoles.Add(dict);
            }
            profileDatamap.SetAttribute("#basicroles_", basicRoles);
        }


        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch,
            Tuple<string, string> userIdSite) {

            var profile = UserProfile.FromJson(json);
            SecurityFacade.GetInstance().SaveUserProfile(profile);



            return new TargetResult("" + profile.Id, "" + profile.Name, profile);
        }

        public override string ApplicationName() {
            return "_UserProfile";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}