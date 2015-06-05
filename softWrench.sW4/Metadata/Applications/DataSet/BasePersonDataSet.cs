using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Iesi.Collections;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.AUTH;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BasePersonDataSet : MaximoApplicationDataSet {
        private IConnectorEngine _maxConnectorEngine;
        private IConnectorEngine _swConnectorEngine;
        private SecurityFacade _securityFacade;
        private ISWDBHibernateDAO _dao;

        protected override IConnectorEngine Engine() {
            if (_maxConnectorEngine == null) {
                _maxConnectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            }
            return _maxConnectorEngine;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            // get is active for each of the users
            var result = base.GetList(application, searchDto);
            var usernames = result.ResultObject.Select(str => str.GetAttribute("personid").ToString()).ToList();
            var swusers = UserManager.GetUsersByUsername(usernames);
            foreach (var record in result.ResultObject) {
                var swuser = swusers.Where(user => user.MaximoPersonId == record.GetAttribute("personid").ToString());
                if (!swuser.Any()) {
                    continue;
                }
                record.Attributes.Add("#isactive", swuser.First().IsActive);
            }
            return result;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            DataMap dataMap;
            if (id != null) {
                dataMap = (DataMap)Engine().FindById(application.Schema, entityMetadata, id, applicationCompositionSchemas);
                if (request.InitialValues != null) {
                    var initValDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application,
                        request.InitialValues, entityMetadata.Schema.MappingType);
                    dataMap = DefaultValuesBuilder.AddMissingInitialValues(dataMap, initValDataMap);
                }
            } else {
                dataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, entityMetadata.Schema.MappingType);
            }
            // get isactive for person from swdb
            var swUser = new User();
            if (dataMap.GetAttribute("personid") != null) {
                swUser = SecurityFacade.GetInstance().FetchUser(dataMap.Value("personid"));
            }
            var isActive = swUser.IsActive ? "1" : "0";
            dataMap.SetAttribute("#isactive", isActive);
            dataMap.SetAttribute("#profiles", swUser.Profiles);
            var availableprofiles = UserProfileManager.FetchAllProfiles(true).ToList();
            foreach (var profile in swUser.Profiles) {
                availableprofiles.Remove(profile);
            }
            dataMap.SetAttribute("#availableprofiles", availableprofiles);

            // Hide the password inputs if using LDAP
            var ldapEnabled = ApplicationConfiguration.LdapServer != null;
            
            dataMap.SetAttribute("#ldapEnabled", ldapEnabled);

            var associationResults = BuildAssociationOptions(dataMap, application, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);

            // Save the updated sw user record
            var username = json.GetValue("personid").ToString();
            var isactive = json.GetValue("#isactive").ToString() == "1";
            User user = UserManager.GetUserByUsername(username) ?? new User(null, username, isactive);

            JToken password;
            json.TryGetValue("#password", out password);
            if (password != null)
            {
                var passwordString = password.ToString();
                user.Password = AuthUtils.GetSha1HashData(passwordString);
            }
            user.IsActive = isactive;
            user.Profiles = LoadProfiles(json);
            UserManager.SaveUser(user);

            return Engine().Execute(operationWrapper);
        }


        public Iesi.Collections.Generic.ISet<UserProfile> LoadProfiles(JObject json) {
            var result = new HashedSet<UserProfile>();
            var profiles = json.GetValue("#profiles");
            if (profiles == null) {
                return result;
            }
            dynamic obj = profiles;
            //Loop over the array
            foreach (dynamic row in profiles) {
                result.Add(new UserProfile() {
                    Id = row.id
                });
            }
            return result;
        }

        public override string ApplicationName() {
            return "person";
        }
    }
}
