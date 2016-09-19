using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softwrench.sw4.Hapag.Security;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Data;

namespace softwrench.sw4.Hapag.Data.Sync {
    public class PersonGroupSyncManager : AMaximoRowstampManager, IUserSyncManager {

        private const string EntityName = "persongroup";
        private const string PersonGroupColumn = "persongroup";

        private readonly HlagLocationManager _hlagLocationManager;

        public PersonGroupSyncManager(SWDBHibernateDAO dao, IConfigurationFacade facade, HlagLocationManager hlagLocationManager, EntityRepository repository)
            : base(dao, facade, repository) {
            _hlagLocationManager = hlagLocationManager;
        }

        public async Task Sync() {
            var rowstamp = ConfigFacade.Lookup<long>(ConfigurationConstants.PersonGroupRowstampKey);
            var dto = new SearchRequestDto();
            //let´s search just for persongroups that begin with the prefix
            dto.AppendSearchEntry(PersonGroupColumn, HapagPersonGroupConstants.BaseHapagPrefix);
            var personGroup = await FetchNew(rowstamp, EntityName, dto);
            var attributeHolders = personGroup as AttributeHolder[] ?? personGroup.ToArray();
            if (!attributeHolders.Any()) {
                //nothing to update
                return;
            }
            var personGroupToSave = ConvertMaximoPersonGroupToPersonGroupEntity(attributeHolders);
            var resultList = SaveOrUpdatePersonGroup(personGroupToSave);
            _hlagLocationManager.UpdateCache(resultList);
            await SetRowstampIfBigger(ConfigurationConstants.PersonGroupRowstampKey, GetLastRowstamp(attributeHolders,new[]{"rowstamp"}), rowstamp);
        }



        private IEnumerable<PersonGroup> ConvertMaximoPersonGroupToPersonGroupEntity(IEnumerable<AttributeHolder> personGroups) {
            var personGroupsToIntegrate = new List<PersonGroup>();
            personGroupsToIntegrate.AddRange(
                personGroups.Select(
                GeneratePersonGroup
            ));
            return personGroupsToIntegrate;
        }

        private static PersonGroup GeneratePersonGroup(AttributeHolder personGroup) {
            var description = (string)personGroup.GetAttribute("description");
            var pg = new PersonGroup {
                Name = (string)personGroup.GetAttribute(PersonGroupColumn),
                Description = description,
            };
            pg.SuperGroup = HlagLocationUtil.IsSuperGroup(pg);
            return pg;
        }

        private IEnumerable<PersonGroup> SaveOrUpdatePersonGroup(IEnumerable<PersonGroup> personGroupsToIntegrate) {
            var resultList = new List<PersonGroup>();
            try {
                foreach (var personGroupToIntegrate in personGroupsToIntegrate) {
                    var personGroup = DAO.FindSingleByQuery<PersonGroup>(PersonGroup.PersonGroupByName, personGroupToIntegrate.Name);
                    if (personGroup != null) {
                        personGroup.Description = personGroupToIntegrate.Description;
                        resultList.Add(DAO.Save(personGroup));
                    } else {
                        resultList.Add(DAO.Save(personGroupToIntegrate));
                    }
                }
                return resultList;
            } catch (Exception e) {
                Log.Error("error integrating maximo person group", e);
                throw;
            }
        }

        public int Order { get { return 2; } }
    }
}
