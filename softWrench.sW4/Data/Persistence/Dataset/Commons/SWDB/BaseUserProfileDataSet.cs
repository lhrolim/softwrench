using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.Util;
using Iesi.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
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


        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
            var profileDatamap = result.ResultObject;

            var profileOb = new UserProfile() {
                Roles = new LinkedHashSet<Role>()
            };

            if (profileDatamap.GetAttribute("id") != null) {
                //not a creation
                profileOb = _userProfileManager.FindByName(profileDatamap.GetAttribute("name") as string);
            }
            HandleBasicRoles(profileOb, profileDatamap);
            //HandleUsers(profileOb, profileDatamap);
            return result;
        }

        [UsedImplicitly]
        public IEnumerable<IAssociationOption> GetTopLevelApplications(OptionFieldProviderParameters parameters) {
            return MetadataProvider.FetchTopLevelApps(null, null).Select(a => new AssociationOption(a.ApplicationName, a.Title));
        }

        //        [UsedImplicitly]
        //        public IEnumerable<IAssociationOption> GetSelectableModes(OptionFieldProviderParameters parameters) {
        //            var entity = parameters.OriginalEntity;
        //            var allowCreation = "true".EqualsIc(entity.GetStringAttribute("#appallowcreation"));
        //            var allowUpdate = "true".EqualsIc(entity.GetStringAttribute("#appallowupdate"));
        //
        //            //TODO: include logic based on permissions
        //            var enumOptions = new List<SchemaPermissionMode>(Enum.GetValues(typeof(SchemaPermissionMode)).Cast<SchemaPermissionMode>());
        //           
        //            var options = enumOptions.Select(i => new PriorityBasedAssociationOption(i.GetName(), i.Label(), i.Priority()));
        //            return options.OrderBy(a => a.Priority);
        //        }

        /// <summary>
        /// This method just need to return a non null value if more than one schema is available for selection, otherwise, just by selecting the mode, it should be enough for the user
        /// 
        /// 
        ///  
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public IEnumerable<IAssociationOption> GetSchemas(OptionFieldProviderParameters parameters) {
            var entity = parameters.OriginalEntity;
            var mode = entity.GetStringAttribute("selectedmode");
            var application = entity.GetStringAttribute("#application");
            SchemaPermissionMode schemaMode;
            Enum.TryParse(mode, true, out schemaMode);
            var completeApplicationMetadataDefinition = MetadataProvider.Application(application);
            var resultSchemas = DoGetSchemas(completeApplicationMetadataDefinition, schemaMode);
            //TODO: allow for mobile
            return resultSchemas.Where(s => ClientPlatform.Mobile != s.Platform).Select(s => new AssociationOption(s.SchemaId, s.Title));
        }

        [NotNull]
        private IEnumerable<ApplicationSchemaDefinition> DoGetSchemas(CompleteApplicationMetadataDefinition completeApplicationMetadataDefinition, SchemaPermissionMode schemaMode) {
            if (SchemaPermissionMode.View.Equals(schemaMode)) {
                var outputSchemas = completeApplicationMetadataDefinition.Schemas().Values.Where(s => s.Mode == SchemaMode.output);
                var applicationSchemaDefinitions = outputSchemas as ApplicationSchemaDefinition[] ?? outputSchemas.ToArray();
                if (applicationSchemaDefinitions.Any()) {
                    //if there are output schemas present, use them
                    return applicationSchemaDefinitions;
                }
                //otherwise fallback to detail implementation
                return completeApplicationMetadataDefinition.NonInternalSchemasByStereotype("detail");
            }
            if (SchemaPermissionMode.Creation.Equals(schemaMode)) {
                return completeApplicationMetadataDefinition.NonInternalSchemasByStereotype("detailnew");
            }
            if (SchemaPermissionMode.Grid.Equals(schemaMode)) {
                return completeApplicationMetadataDefinition.NonInternalSchemasByStereotype("list");
            }
            return completeApplicationMetadataDefinition.NonInternalSchemasByStereotype("detail");
        }

        [UsedImplicitly]
        public IEnumerable<IAssociationOption> GetSelectableTabs(OptionFieldProviderParameters parameters) {
            var entity = parameters.OriginalEntity;
            var mode = entity.GetStringAttribute("selectedmode");
            var application = entity.GetStringAttribute("#application");
            var schemaId = entity.GetStringAttribute("schema");
            SchemaPermissionMode schemaMode;
            Enum.TryParse(mode, true, out schemaMode);
            var completeApplicationMetadataDefinition = MetadataProvider.Application(application);
            ApplicationSchemaDefinition schema =
                completeApplicationMetadataDefinition.Schema(new ApplicationMetadataSchemaKey(schemaId, SchemaMode.None, ClientPlatform.Web));
            var results = new List<PriorityBasedAssociationOption>();
            results.Add(new PriorityBasedAssociationOption("main", "Main", 0, new Dictionary<string, object> { { "type", "main" } }));
            results.AddRange(schema.Tabs().Select(c => new PriorityBasedAssociationOption(c.Attribute, c.Label, 1, new Dictionary<string, object> { { "type", c.Type } })));
            return results;
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

        //private void HandleUsers(UserProfile profileOb, DataMap profileDatamap) {
        //    var users = SWDAO.FindByNativeQuery("SELECT * FROM SW_USER2 WHERE ID IN (SELECT USER_ID FROM SW_USER_USERPROFILE WHERE PROFILE_ID = {0})".FormatInvariant(profileOb.Id));
        //    // Get additinal information about each user from maximo database (firstname, lastname)
        //    profileDatamap.SetAttribute("#users_", users);
        //}

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var result = await base.GetCompositionData(application, request, currentData);
            if (request.CompositionList != null && request.CompositionList.Contains("#fieldPermissions_") && request.CompositionList.Count == 1) {
                var app = MetadataProvider.Application(result.OriginalCruddata.GetStringAttribute("#application"));
                var schema = app.Schema(new ApplicationMetadataSchemaKey(result.OriginalCruddata.GetStringAttribute("schema"), SchemaMode.None, ClientPlatform.Web));
                return _userProfileManager.LoadAvailableFieldsAsCompositionData(schema, result.OriginalCruddata.GetStringAttribute("#selectedtab"), request.PaginatedSearch.PageNumber);
            }

            if (request.CompositionList != null && request.CompositionList.Contains("#actionPermissions_") && request.CompositionList.Count == 1) {
                var app = MetadataProvider.Application(result.OriginalCruddata.GetStringAttribute("#application"));
                var schema = app.Schema(new ApplicationMetadataSchemaKey(result.OriginalCruddata.GetStringAttribute("schema"), SchemaMode.None, ClientPlatform.Web));
                return _userProfileManager.LoadAvailableActionsAsComposition(schema, request.PaginatedSearch.PageNumber);
            }

            return result;
        }

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {
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
