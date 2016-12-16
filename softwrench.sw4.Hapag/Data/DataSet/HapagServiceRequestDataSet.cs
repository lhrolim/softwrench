using cts.commons.portable.Util;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cts.commons.persistence;
using softwrench.sw4.Hapag.Data.WS.Ism.Base;
using softWrench.sW4.Metadata.Applications.DataSet.Faq;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagServiceRequestDataSet : HapagBaseApplicationDataSet {


        public HapagServiceRequestDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, IMaximoHibernateDAO maxDao)
            : base(locationManager, entityRepository, maxDao) {
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            //this means that we are creating the service request from an asset
            var isCreationFromAsset = request.InitialValues != null && request.InitialValues.ContainsAttribute("assetnum");
            if (isCreationFromAsset) {
                //if the asset is pre selected, we need to determine the right schema out of it (either, printer,phone or general)
                application = DetermineSchemaFromAsset(request.InitialValues);
                AdjustInitialValues(request.InitialValues, application.Schema.SchemaId);
                request.AssociationsToFetch = "#all";
            }
            var dbDetail = await base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;
            if (resultObject == null) {
                //it happens only if we´re pointint to a database different then the ws impl
                return dbDetail;
            }

            if (application.Schema.SchemaId == "creationSummary") {
                HandleCreationSummary(resultObject);
            }
            var usefulFaqLinksUtils = GetUsefulFaqLinksUtils(application.Schema);
            if (usefulFaqLinksUtils.Count > 0) {
                HandleUsefulFaqLinks(resultObject, usefulFaqLinksUtils);
            }

            if (application.Schema.Mode == SchemaMode.input) {
                HandleAffectDateAndTime(resultObject, application);
            } else if (application.Schema.Mode == SchemaMode.output) {
                HandleClosedDate(resultObject, application);
            }
            dbDetail.AllAssociationsFetched = isCreationFromAsset;
            return dbDetail;
        }

        private void AdjustInitialValues(Entity initialValues, string schemaId) {
            var isCustodian = (string)initialValues.GetAttribute("#iscustodian");
            var originalLocation = (string)initialValues.GetAttribute(ISMConstants.PluspCustomerColumn, true);
            //workaround as the dropdown values of the  locations contains currently, just MT4,MT4 instead of HLC-DE-MT4
            //TODO: shouldn´t that be the entire value?
            var prefixedOnlyLocation = originalLocation.Replace(HapagPersonGroupConstants.PersonGroupPrefix, "");
            if (schemaId.Equals("phone")) {
                initialValues.SetAttribute("affectedDevice", "Cisco-IP-Phone");
                initialValues.SetAttribute("phonepluspcustomer", originalLocation);
            } else if (schemaId.Equals("printer")) {
                initialValues.SetAttribute("printerpluspcustomer", originalLocation);
            } else if (isCustodian.EqualsIc("false")) {
                var assetnum = initialValues.GetAttribute("assetnum", true);
                initialValues.SetAttribute("itcassetnum", assetnum);
                initialValues.SetAttribute("itcassetlocation", prefixedOnlyLocation);
            }
        }

        private ApplicationMetadata DetermineSchemaFromAsset(Entity initialValues) {
            var assetclassstructure = (string)initialValues.GetAttribute("classstructureid");
            var completeApplication = MetadataProvider.Application("servicerequest");
            if (AssetConstants.PhoneClassStructure.Equals(assetclassstructure)) {
                return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("phone"));
            }
            if (IsPrinterAsset(assetclassstructure)) {
                return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("printer"));
            }
            return completeApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("general"));
        }

        private static readonly IDictionary<string, List<string>> FaqUsefulLinks = new Dictionary<string, List<string>>();

        private static List<UsefulFaqLinksUtils> GetUsefulFaqLinksUtils(ApplicationSchemaDefinition applicationSchemaDefinition) {
            var list = new List<UsefulFaqLinksUtils>();
            try {
                var schemaId = applicationSchemaDefinition.SchemaId;
                if (!FaqUsefulLinks.ContainsKey(schemaId)) {
                    FaqUsefulLinks.Add(schemaId, GetFaqUsefulLinksFromConfigFile(schemaId));
                }
                if (FaqUsefulLinks[schemaId].Count <= 0) {
                    return list;
                }
                var language = FaqUtils.GetLanguageToFilter(SecurityFacade.CurrentUser().Language);
                list.AddRange(FaqUsefulLinks[schemaId].Select(id => new UsefulFaqLinksUtils(id, language, schemaId)));
                Log.DebugFormat("useful faq list size {0}", list.Count());
                return list;

            } catch {
                return list;
            }
        }

        private static List<string> GetFaqUsefulLinksFromConfigFile(string schemaId) {
            var faqUsefulLinks = new List<string>();
            var path = ApplicationConfiguration.SertiveItFaqUsefulLinksPath;
            if (string.IsNullOrEmpty(path)) {
                return faqUsefulLinks;
            }
            if (System.IO.File.Exists(@path)) {
                foreach (var row in System.IO.File.ReadAllLines(@path)) {
                    var solutionFromSchema = Regex.Split(row, "<" + schemaId + ">");
                    if (solutionFromSchema.Length > 1) {
                        var solutionAux = Regex.Split(solutionFromSchema[1], "</" + schemaId + ">");
                        int id;
                        if (solutionAux.Length > 1 && int.TryParse(solutionAux[0], out id)) {
                            faqUsefulLinks.Add(solutionAux[0]);
                        }
                    }
                }
            }
            return faqUsefulLinks;
        }

        private void HandleAffectDateAndTime(DataMap resultObject, ApplicationMetadata application) {
            var originalDateTime = resultObject.GetAttribute("affecteddate", true);
            resultObject["#affecteddateonly"] = originalDateTime;
            resultObject["#affectedtime"] = originalDateTime;

            var user = SecurityFacade.CurrentUser();
            resultObject["isitc"] = ShouldShowITC(user);
        }

        private bool ShouldShowITC(InMemoryUser user) {
            var ctx = ContextLookuper.LookupContext();
            //to avoid null references...
            var module = ctx.Module ?? "";

            var isFrThatHasNoAccess = module.EqualsAny(FunctionalRole.Tom.GetName(), FunctionalRole.Itom.GetName(),
                FunctionalRole.Sso.GetName());
            if (isFrThatHasNoAccess) {
                return false;
            }
            return user.HasProfile(ProfileType.Itc) || module.Equals(FunctionalRole.XItc.GetName());
        }

        private void HandleClosedDate(DataMap resultObject, ApplicationMetadata application) {
            var status = resultObject.GetAttribute("status");
            var statusDate = resultObject.GetAttribute("statusdate");
            resultObject.SetAttribute("#closeddate", "CLOSED".Equals(status) ? statusDate : "");
        }

        private static async void HandleUsefulFaqLinks(DataMap resultObject, List<UsefulFaqLinksUtils> usefulFaqLinksUtils) {
            var usefulFaqLinks = await FaqUtils.GetUsefulFaqLinks(usefulFaqLinksUtils);

            if (CollectionExtensions.IsNullOrEmpty(usefulFaqLinks)) {
                return;
            }
            resultObject.Add("usefulFaqLinks", usefulFaqLinks);
        }

        private static void HandleCreationSummary(DataMap resultObject) {
            var list = resultObject.GetAttribute("attachment_");
            var sb = new StringBuilder();
            foreach (var dictionary in (IEnumerable<Dictionary<string, object>>)list) {
                sb.Append("<p>");
                sb.Append(dictionary["docinfo_.description"]);
                sb.Append("</p>");
            }
            resultObject.Add("#attachmentsummary", sb.ToString());
        }


        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {

            var filter = parameters.BASEDto;
            // Default filter to be applied to all Schemas
            filter.AppendSearchEntry("status", false, AssetConstants.Active, AssetConstants.Operating);
            filter.IgnoreWhereClause = true;

            // Add another filters depending on Schema
            var attributeHolder = parameters.OriginalEntity;
            var schemaId = parameters.Metadata.Schema.SchemaId;

            if ("printer".Equals(schemaId)) {
                filter.AppendSearchEntry(ISMConstants.PluspCustomerColumn, (string)attributeHolder.GetAttribute("printerpluspcustomer"));
                filter.AppendWhereClause(AssetConstants.BuildPrinterWhereClause());
            } else if ("phone".Equals(schemaId)) {
                filter.AppendSearchEntry(ISMConstants.PluspCustomerColumn, (string)attributeHolder.GetAttribute("phonepluspcustomer"));
                filter.AppendSearchEntry(AssetConstants.ClassStructureIdColumn, AssetConstants.PhoneClassStructure);
            } else {
                var module = ContextLookuper.LookupContext().Module;
                if (module == null || !module.EqualsAny(FunctionalRole.AssetControl.GetName(), FunctionalRole.AssetRamControl.GetName())) {
                    //if asset control, or asset ram control could be creating sr out of asset and this would make no sense
                    filter.AppendSearchEntry(AssetConstants.CustodianColumn, SecurityFacade.CurrentUser(false).MaximoPersonId);
                }
            }
            return filter;
        }


        public SearchRequestDto FilterAssetsByItcLocation(AssociationPreFilterFunctionParameters parameters) {
            var fromLocation = parameters.OriginalEntity.GetAttribute("itcassetlocation") as string;
            return AssetByLocationCondition(parameters.BASEDto, fromLocation);
        }

        public SearchRequestDto FilterAffectedPerson(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            //var w = "STATUS = 'ACTIVE' AND PLUSPCUSTVENDOR ='DE017430' AND LocationOrg = 'HLAG-00'";
            filter.WhereClause = "STATUS = 'ACTIVE'";
            return filter;
        }


        public SearchRequestDto LocationFilterByStatusFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.WhereClause = "Status NOT IN ('DECOMMISSIONED') AND TYPE='OPERATING'";
            return filter;
        }

        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            if (parameters.Metadata.Schema.SchemaId.Equals("printer") || parameters.Metadata.Schema.SchemaId.Equals("phone")) {
                filter.WhereClause = string.Format("LOCATION = '{0}'", parameters.OriginalEntity["location"]);
            }
            return filter;
        }
        public SearchRequestDto PreFilterFunction(AssociationPreFilterFunctionParameters parameters) {
            return parameters.BASEDto;
        }


        public override string ApplicationName() {
            return "servicerequest";
        }
        public override string ClientFilter() {
            return "hapag";
        }
    }
}
