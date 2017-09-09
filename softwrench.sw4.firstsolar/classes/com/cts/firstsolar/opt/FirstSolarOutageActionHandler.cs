using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Common.Logging;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {

    public class FirstSolarOutageActionHandler : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarOutageActionHandler));

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        public bool HandleOutageActions(CrudOperationData crudoperationData, WorkPackage package, ApplicationSchemaDefinition schema) {

            //TODO: extract composition generic code
            if (package.OutageActions == null) {
                package.OutageActions = new List<OutageAction>();
            }

            if (!schema.Compositions().Any(c => EntityUtil.IsRelationshipNameEquals(c.AssociationKey, "outageActions"))) {
                //might be disabled due to security reasons
                return false;
            }

            var toKeepDoa = new List<OutageAction>();
            var anyNewDoa = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("outageActions_")) {
                var doasData = crudoperationData.AssociationAttributes["outageActions_"] as List<CrudOperationData>;
                if (doasData == null) {
                    throw new Exception("Incorrect format of daily outage meeting list.");
                }

                doasData.ForEach((data) => {
                    var doa = GetOurCreateDailyOutageAction(data, package.OutageActions, toKeepDoa);
                    EntityBuilder.PopulateTypedEntity(data, doa);
                    anyNewDoa = anyNewDoa || doa.Id == null;
                    if ("true".EqualsIc(data.GetStringAttribute("#isDirty"))) {
                        doa.ActionTime = DateTime.Now;
                        anyNewDoa = true;
                    }
                    if (doa.AssigneeLabel == null) {
                        doa.AssigneeLabel = data.GetStringAttribute("assignee_.displayname");
                    }


                    package.OutageActions.Add(doa);

                    doa = Dao.Save(doa);

                    toKeepDoa.Add(doa);
                });



            }
            toKeepDoa.AddRange(HandleDoasOutOfEngineeringTests(package, crudoperationData));

            var deleted = new List<OutageAction>();
            package.OutageActions.ForEach(doa => {
                if (toKeepDoa.Contains(doa)) {
                    return;
                }
                Dao.Delete(doa);
                deleted.Add(doa);
            });
            deleted.ForEach(doa => package.OutageActions.Remove(doa));
            return anyNewDoa;
        }

        private List<OutageAction> HandleDoasOutOfEngineeringTests(WorkPackage package, CrudOperationData crudoperationData) {
            var createdTests = crudoperationData.GetStringAttribute("newlycreatedtests");
            var outageActionsToAdd = new List<OutageAction>();
            if (string.IsNullOrEmpty(createdTests)) {
                return outageActionsToAdd;
            }

            var tests = createdTests.Split(',');
            const string techKey = "#" + FirstSolarCustomGlobalFedService.TechColumn;
            const string techIdKey = "#" + FirstSolarCustomGlobalFedService.TechIdColumn;
            var unmaped = crudoperationData.UnmappedAttributes;

            foreach (var test in tests) {
                var action = new OutageAction {
                    Completed = false,
                    ActionTime = DateTime.Now,
                    Action = test
                };

                if (unmaped.ContainsKey(techKey) && unmaped.ContainsKey(techIdKey)) {
                    action.Assignee = unmaped[techIdKey];
                    action.AssigneeLabel = unmaped[techKey];
                }

                outageActionsToAdd.Add(action);
                package.OutageActions.Add(action);
            }

            return outageActionsToAdd;
        }

        private OutageAction GetOurCreateDailyOutageAction(AttributeHolder crudoperationData, ICollection<OutageAction> existingDom, ICollection<OutageAction> toKeepDom) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingDom == null) {
                return new OutageAction();
            }
            var found = existingDom.FirstOrDefault(dom => dom.Id == id);
            if (found == null) {
                return new OutageAction() { Id = id };
            }
            toKeepDom.Add(found);
            return found;
        }



    }
}
