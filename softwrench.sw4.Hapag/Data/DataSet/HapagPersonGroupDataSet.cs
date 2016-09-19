using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;

namespace softwrench.sw4.Hapag.Data.DataSet {

    class HapagPersonGroupDataSet : HapagBaseApplicationDataSet {
        public HapagPersonGroupDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, IMaximoHibernateDAO maxDao) 
            : base(locationManager, entityRepository, maxDao)
        {
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var dbList = await base.GetList(application, searchDto);
            var resultObject = dbList.ResultObject;
            if ((application.Schema.SchemaId == "itcreport")) {
                dbList.ResultObject = await HandleITCReport(resultObject, application, searchDto);
            }
            return dbList;
        }

        private async Task<List<AttributeHolder>> HandleITCReport(IEnumerable<AttributeHolder> resultObject, ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {

            var user = SecurityFacade.CurrentUser();
            var applicationSchemaKey = new ApplicationMetadataSchemaKey("itcreportregionandarea");
            var complementApplicationMetadata = MetadataProvider.Application(application.Name).ApplyPolicies(applicationSchemaKey, user, ClientPlatform.Web);
            var complementSearchDTO = new PaginatedSearchRequestDto();

            complementSearchDTO.SearchParams = searchDto.SearchParams.Replace("persongroup_.", "locationpersongroup_.");
            complementSearchDTO.SearchValues = searchDto.SearchValues;
            complementSearchDTO.SearchSort = searchDto.SearchSort;            
            complementSearchDTO.ShouldPaginate = false;
            
            ContextLookuper.FillContext(applicationSchemaKey); 
            var complementaryList = await base.GetList(complementApplicationMetadata, complementSearchDTO);
            resultObject = resultObject.Union(complementaryList.ResultObject);

            foreach (var attributeHolder in resultObject) {
                try {
                    var persongroup = attributeHolder["persongroup"];
                    if (persongroup != null) {
                        
                        var groupDefault = attributeHolder["groupdefault"];                        

                        if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagLocationPrefix)) {

                            attributeHolder["itcrole"] = groupDefault.Equals(1) ? "Location ITC" : "Location ITC Delegate";
                            attributeHolder["hlagsite"] = persongroup.ToString().Substring(HapagPersonGroupConstants.BaseHapagLocationPrefix.Length, 3);

                            var persongroupdescription = attributeHolder["persongroup_.description"];
                            if (persongroupdescription != null) {
                                attributeHolder["hlagcostcenter"] = persongroupdescription.ToString().Substring(persongroupdescription.ToString().LastIndexOf("-") + 1);

                                var costcenterDescription = attributeHolder["costcenterdescription"];
                                if (costcenterDescription != null) {
                                    attributeHolder["hlagcostcenter"] += " // " + costcenterDescription;
                                }
                            }

                            // Add blank fields just to match the second union iterator (Region and Area)
                            attributeHolder["locationcostcenterdescription"] = String.Empty;
                            attributeHolder["locationpersongroup_.persongroup"] = String.Empty;
                            attributeHolder["locationpersongroup_.description"] = String.Empty;                            

                        } else if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagAreaPrefix) || 
                            persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagRegionPrefix)) {

                            var locationpersongroup = attributeHolder["locationpersongroup_.persongroup"];
                            attributeHolder["hlagsite"] = locationpersongroup.ToString().Substring(HapagPersonGroupConstants.BaseHapagLocationPrefix.Length, 3);

                            var locationpersongroupdescription = attributeHolder["locationpersongroup_.description"];
                            if (locationpersongroupdescription != null) {
                                attributeHolder["hlagcostcenter"] = locationpersongroupdescription.ToString().Substring(locationpersongroupdescription.ToString().LastIndexOf("-") + 1);

                                var locationcostcenterdescription = attributeHolder["locationcostcenterdescription"];
                                if (locationcostcenterdescription != null) {
                                    attributeHolder["hlagcostcenter"] += " // " + locationcostcenterdescription;
                                }
                            }
                            
                            if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagAreaPrefix)) {
                                attributeHolder["itcrole"] = groupDefault.Equals(1) ? "Area ITC" : "Area ITC Delegate";
                            } else if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagRegionPrefix)) {
                                attributeHolder["itcrole"] = groupDefault.Equals(1) ? "Region ITC" : "Region ITC Delegate";
                            }

                            // Add blank fields just to match the first union iterator (Location)
                            attributeHolder["costcenterdescription"] = String.Empty;
                        }
                    }                    
                }
                catch (ArgumentOutOfRangeException) {
                }
            }

            return resultObject.ToList();
        }

        public override string ApplicationName() {
            return "persongroupview";
        }
    }
}
