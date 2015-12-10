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

namespace softwrench.sw4.Hapag.Data.DataSet {

    class HapagPersonGroupDataSet : HapagBaseApplicationDataSet {
        public HapagPersonGroupDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, MaximoHibernateDAO maxDao) 
            : base(locationManager, entityRepository, maxDao)
        {
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var dbList = base.GetList(application, searchDto);
            var resultObject = dbList.ResultObject;
            if ((application.Schema.SchemaId == "itcreport")) {
                dbList.ResultObject = HandleITCReport(resultObject, application, searchDto);
            }
            return dbList;
        }

        private IEnumerable<AttributeHolder> HandleITCReport(IEnumerable<AttributeHolder> resultObject, ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {

            var user = SecurityFacade.CurrentUser();
            var applicationSchemaKey = new ApplicationMetadataSchemaKey("itcreportregionandarea");
            var complementApplicationMetadata = MetadataProvider.Application(application.Name).ApplyPolicies(applicationSchemaKey, user, ClientPlatform.Web);
            var complementSearchDTO = new PaginatedSearchRequestDto();

            complementSearchDTO.SearchParams = searchDto.SearchParams.Replace("persongroup_.", "locationpersongroup_.");
            complementSearchDTO.SearchValues = searchDto.SearchValues;
            complementSearchDTO.SearchSort = searchDto.SearchSort;            
            complementSearchDTO.ShouldPaginate = false;
            
            ContextLookuper.FillContext(applicationSchemaKey); 
            var complementaryList = base.GetList(complementApplicationMetadata, complementSearchDTO);
            resultObject = resultObject.Union(complementaryList.ResultObject);

            foreach (var attributeHolder in resultObject) {
                try {
                    var persongroup = attributeHolder.Attributes["persongroup"];
                    if (persongroup != null) {
                        
                        var groupDefault = attributeHolder.Attributes["groupdefault"];                        

                        if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagLocationPrefix)) {

                            attributeHolder.Attributes["itcrole"] = groupDefault.Equals(1) ? "Location ITC" : "Location ITC Delegate";
                            attributeHolder.Attributes["hlagsite"] = persongroup.ToString().Substring(HapagPersonGroupConstants.BaseHapagLocationPrefix.Length, 3);

                            var persongroupdescription = attributeHolder.Attributes["persongroup_.description"];
                            if (persongroupdescription != null) {
                                attributeHolder.Attributes["hlagcostcenter"] = persongroupdescription.ToString().Substring(persongroupdescription.ToString().LastIndexOf("-") + 1);

                                var costcenterDescription = attributeHolder.Attributes["costcenterdescription"];
                                if (costcenterDescription != null) {
                                    attributeHolder.Attributes["hlagcostcenter"] += " // " + costcenterDescription;
                                }
                            }

                            // Add blank fields just to match the second union iterator (Region and Area)
                            attributeHolder.Attributes["locationcostcenterdescription"] = String.Empty;
                            attributeHolder.Attributes["locationpersongroup_.persongroup"] = String.Empty;
                            attributeHolder.Attributes["locationpersongroup_.description"] = String.Empty;                            

                        } else if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagAreaPrefix) || 
                            persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagRegionPrefix)) {

                            var locationpersongroup = attributeHolder.Attributes["locationpersongroup_.persongroup"];
                            attributeHolder.Attributes["hlagsite"] = locationpersongroup.ToString().Substring(HapagPersonGroupConstants.BaseHapagLocationPrefix.Length, 3);

                            var locationpersongroupdescription = attributeHolder.Attributes["locationpersongroup_.description"];
                            if (locationpersongroupdescription != null) {
                                attributeHolder.Attributes["hlagcostcenter"] = locationpersongroupdescription.ToString().Substring(locationpersongroupdescription.ToString().LastIndexOf("-") + 1);

                                var locationcostcenterdescription = attributeHolder.Attributes["locationcostcenterdescription"];
                                if (locationcostcenterdescription != null) {
                                    attributeHolder.Attributes["hlagcostcenter"] += " // " + locationcostcenterdescription;
                                }
                            }
                            
                            if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagAreaPrefix)) {
                                attributeHolder.Attributes["itcrole"] = groupDefault.Equals(1) ? "Area ITC" : "Area ITC Delegate";
                            } else if (persongroup.ToString().StartsWith(HapagPersonGroupConstants.BaseHapagRegionPrefix)) {
                                attributeHolder.Attributes["itcrole"] = groupDefault.Equals(1) ? "Region ITC" : "Region ITC Delegate";
                            }

                            // Add blank fields just to match the first union iterator (Location)
                            attributeHolder.Attributes["costcenterdescription"] = String.Empty;
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
