using System;
using cts.commons.simpleinjector;
using softwrench.sw4.problem.classes;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.AUTH;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.services {

    [OverridingComponent(ClientFilters = "tgcs")]
    public class ToshibaUserSyncManager : UserSyncManager {

        private readonly RestEntityRepository _restEntityRepository;

        private readonly EntityMetadata _entityMetadata;
        private readonly ApplicationMetadata _applicationMetadata;

        private readonly DataSetProvider _dsProvider;


        public ToshibaUserSyncManager(SWDBHibernateDAO dao, IConfigurationFacade facade, EntityRepository repository, IProblemManager problemManager, LdapManager ldapManager, RestEntityRepository restEntityRepository, DataSetProvider dsProvider)
            : base(dao, facade, repository, problemManager, ldapManager) {
            _restEntityRepository = restEntityRepository;
            _dsProvider = dsProvider;
            _entityMetadata = MetadataProvider.Entity("person");
            var application = MetadataProvider.Application("person", false);
            if (!ApplicationConfiguration.IsClient("tgcs")) {
                return;
            }
            _applicationMetadata = application.StaticFromSchema("newPersonDetail");
        }

        public override User GetUserFromMaximoBySwUser(User swUser, bool forceUserShouldExist = false) {
            var personId = TranslatePersonId(swUser);


            //first checking at softlayer side
            var user = base.GetUserFromMaximoBySwUser(swUser, forceUserShouldExist);

            if (user == null) {
                Log.DebugFormat("Fetching user {0} from ISM", personId);
                //if not found, let´s force a ISM sync/creation
                var ismPerson = _restEntityRepository.Get(_entityMetadata, personId);
                if (ismPerson == null) {
                    return null;
                }
                NormalizeISMPerson(ismPerson);
                var ds = _dsProvider.LookupDataSet("person", "newPersonDetail");
                Log.InfoFormat("creating person and user on the fly on Maximo");
                var result = ds.Execute(_applicationMetadata, new JObjectDatamapAdapter(ismPerson), null, OperationConstants.CRUD_CREATE, false, new Tuple<string, string>(ismPerson.GetStringAttribute("personid"), ismPerson.GetStringAttribute("siteid")));
                return (User)result.ResultObject;
            }

            return user;
        }

        /// <summary>
        /// Normalizing attributes for BasePersonDataSet
        /// </summary>
        /// <param name="ismPerson"></param>
        private static void NormalizeISMPerson(DataMap ismPerson) {
            ismPerson.SetAttribute("#apicall", true);
            //inactivating user
            ismPerson.SetAttribute("#isactive", ismPerson.GetStringAttribute("STATUS").Equals("ACTIVE"));
            ismPerson.SetAttribute("#primaryemail", ismPerson.GetStringAttribute("primaryemail"));
        }


        //TODO: findout what to with the person
        protected virtual string TranslatePersonId(User swUser) {
            return swUser.MaximoPersonId;
        }
    }
}
