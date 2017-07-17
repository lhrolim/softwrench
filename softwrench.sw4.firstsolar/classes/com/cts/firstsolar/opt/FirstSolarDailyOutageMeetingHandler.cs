using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
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
    public class FirstSolarDailyOutageMeetingHandler : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarDailyOutageMeetingHandler));

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public FirstSolarDailyOutageMeetingEmailService EmailService { get; set; }

        [Import]
        public FirstSolarWorkPackageAttachmentsHandler AttachmentsHandler { get; set; }

        private const string FilterPrefix = "swwpkgdo:";
        private const string AttachmentsRelationship = "#domfileexplorer_";

        public void HandleAttachmentsOnCompositionLoad(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            AttachmentsHandler.HandleAttachmentsOnCompositionLoad(woResult, packageResult, AttachmentsRelationship, FSWPackageConstants.DailyOutageMeetingAttachsRelationship);
        }

        public bool HandleDailyOutageMeetings(CrudOperationData crudoperationData, WorkPackage package, CrudOperationData woData, ApplicationSchemaDefinition schema) {

            if (package.DailyOutageMeetings == null) {
                package.DailyOutageMeetings = new List<DailyOutageMeeting>();
            }

            if (!schema.Compositions().Any(c => EntityUtil.IsRelationshipNameEquals(c.AssociationKey, "dailyOutageMeetings"))) {
                //might be disabled due to security reasons
                return false;
            }

            var toKeepDom = new List<DailyOutageMeeting>();
            var anyNewDom = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("dailyOutageMeetings_")) {
                var domsData = crudoperationData.AssociationAttributes["dailyOutageMeetings_"] as List<CrudOperationData>;
                if (domsData == null) {
                    throw new Exception("Incorrect format of daily outage meeting list.");
                }
                domsData.ForEach((data) => {
                    var dom = GetOurCreateDailyOutageMeeting(data, package.DailyOutageMeetings, toKeepDom);
                    anyNewDom = anyNewDom || dom.Id == null;
                    EntityBuilder.PopulateTypedEntity(data, dom);
                    dom.Cc = EmailService.HandleEmailRecipient(data, "cc");
                    if (dom.Id != null) {
                        return;
                    }
                    package.DailyOutageMeetings.Add(dom);

                    dom = Dao.Save(dom);

                    AttachmentsHandler.HandleAttachments(data, dom.Id ?? 0, AttachmentsRelationship, FilterPrefix, woData);

                    toKeepDom.Add(dom);
                });
            }

            var deleted = new List<DailyOutageMeeting>();
            package.DailyOutageMeetings.ForEach(dom => {
                if (toKeepDom.Contains(dom)) {
                    return;
                }
                Dao.Delete(dom);
                deleted.Add(dom);
            });
            deleted.ForEach(dom => package.DailyOutageMeetings.Remove(dom));
            return anyNewDom;
        }

        public async Task HandleEmails(WorkPackage package, string siteId, IEnumerable<DailyOutageMeeting> domsToSend) {
            await AttachmentsHandler.HandleEmails(package, siteId, FSWPackageConstants.DailyOutageMeetingAttachsRelationship, FilterPrefix, domsToSend, EmailService);
        }

        private static DailyOutageMeeting GetOurCreateDailyOutageMeeting(AttributeHolder crudoperationData, ICollection<DailyOutageMeeting> existingDom, ICollection<DailyOutageMeeting> toKeepDom) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingDom == null) {
                return new DailyOutageMeeting();
            }
            var found = existingDom.FirstOrDefault(dom => dom.Id == id);
            if (found == null) {
                return new DailyOutageMeeting() { Id = id };
            }
            toKeepDom.Add(found);
            return found;
        }

        private async Task InnerHandleEmail(DailyOutageMeeting dom, WorkPackage package, string siteId) {
            try {
                await EmailService.SendEmail(dom, package, siteId);
            } catch (Exception ex) {
                dom.Status = RequestStatus.Error;
                Log.ErrorFormat("Failed to send email for {0} {1} from workorder with wonum {2} from site {3}: {4}", EmailService.RequestI18N(), dom.Id, package.Wonum, siteId, ex.Message);
                await Dao.SaveAsync(dom);
            }
        }
    }
}
