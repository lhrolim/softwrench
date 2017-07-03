using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.Persistence.Operation;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarCallOutHandler : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public IMaximoHibernateDAO MaxDao { get; set; }

        [Import]
        public FirstSolarCallOutEmailService CallOutEmailService { get; set; }

        [Import]
        public FirstSolarWorkPackageAttachmentsHandler AttachmentsHandler { get; set; }

        private const string FilterPrefix = "swwpkgco:";
        private const string AttachmentsRelationship = "#calloutfileexplorer_";
        protected static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutHandler));

        public void HandleAttachmentsOnCompositionLoad(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            AttachmentsHandler.HandleAttachmentsOnCompositionLoad(woResult, packageResult, AttachmentsRelationship, FSWPackageConstants.CallOutAttachsRelationship);
        }

        public bool HandleCallOuts(CrudOperationData crudoperationData, WorkPackage package, CrudOperationData woData, ApplicationSchemaDefinition schema) {

            if (!schema.Compositions().Any(c => EntityUtil.IsRelationshipNameEquals(c.AssociationKey, "callouts"))) {
                //might be disabled due to security reasons
                return false;
            }

            var existingCallOuts = package.CallOuts;
            package.CallOuts = new List<CallOut>();

            var anyNewCallOut = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("callOuts_")) {
                var callOutsData = crudoperationData.AssociationAttributes["callOuts_"] as List<CrudOperationData>;
                if (callOutsData == null) {
                    throw new Exception("Incorrect format of subcontractors call out list.");
                }
                callOutsData.ForEach((data) => {
                    var callout = GetOrCreateCallOut(data, existingCallOuts);
                    anyNewCallOut = anyNewCallOut || callout.Id == null;
                    package.CallOuts.Add(HandleCallout(data, callout, package, woData));
                });
            }
            existingCallOuts?.ForEach(callout => {
                if (RequestStatus.Sent.Equals(callout.Status)) {
                    throw new Exception("Is not possible delete a sent subcontractor callout. Reload the page to get the updated version of this work package.");
                }
                Dao.Delete(callout);
            });
            return anyNewCallOut;
        }

        public async Task HandleEmails(WorkPackage package, string siteId, IEnumerable<CallOut> calloutsToSend) {
            await AttachmentsHandler.HandleEmails(package, siteId, FSWPackageConstants.CallOutAttachsRelationship, FilterPrefix, calloutsToSend, CallOutEmailService);
        }

        public void HandleEmail(CallOut callOut, WorkPackage package, string siteId) {
            AttachmentsHandler.HandleEmail(callOut, package, siteId, FilterPrefix, FSWPackageConstants.CallOutAttachsRelationship, CallOutEmailService);
        }

        private CallOut HandleCallout(CrudOperationData crudoperationData, CallOut callOut, WorkPackage workpackage, CrudOperationData woData) {
            var newStatusSt = crudoperationData.GetStringAttribute("status");
            var sendNow = crudoperationData.GetBooleanAttribute("sendnow");

            RequestStatus newStatus;
            Enum.TryParse(newStatusSt, true, out newStatus);

            if (callOut.Status.IsSubmitted()) {
                if (!newStatus.IsSubmitted()) {
                    throw new Exception("Is not possible edit a sent subcontractor callout. Reload the page to get the updated version of this work package.");
                }
                // submited callouts are not editable so just return the existing one
                return callOut;
            }



            var nullableSubcontractorId = crudoperationData.GetStringAttribute("subcontractorid");
            var subContractorName = crudoperationData.GetStringAttribute("subcontractorname");

            if (crudoperationData.AssociationAttributes.ContainsKey("subcontractor_")) {
                var subcontractor = (CrudOperationData)crudoperationData.AssociationAttributes["subcontractor_"];
                subContractorName = subcontractor.GetStringAttribute("name");
            } else if (subContractorName == null) {
                throw new Exception("missing subcontractor");
            }


            callOut = EntityBuilder.PopulateTypedEntity(crudoperationData, callOut);

            callOut.SubContractorId = nullableSubcontractorId;
            callOut.SubContractorName = subContractorName;

            callOut.Status = newStatus;
            callOut.Email = CallOutEmailService.HandleEmailRecipient(crudoperationData, "email");

            if (sendNow.HasValue && sendNow.Value) {
                callOut.SendTime = DateTime.Now;
                callOut.SendNow = true;
            } else {
                callOut.Status = RequestStatus.Scheduled;
            }


            callOut.WorkPackage = workpackage;
            callOut.GenerateToken();

            callOut = Dao.Save(callOut);

            AttachmentsHandler.HandleAttachments(crudoperationData, callOut.Id ?? 0, AttachmentsRelationship, FilterPrefix, woData);

            return callOut;
        }

        private static CallOut GetOrCreateCallOut(CrudOperationData crudoperationData, IList<CallOut> existingCallOuts) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingCallOuts == null) {
                return new CallOut();
            }
            var found = existingCallOuts.FirstOrDefault(callOut => callOut.Id == id);
            if (found == null) {
                return new CallOut() { Id = id };
            }
            existingCallOuts.Remove(found);
            return found;
        }
    }
}
