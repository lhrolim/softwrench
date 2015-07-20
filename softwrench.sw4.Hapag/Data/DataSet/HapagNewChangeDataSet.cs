using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Security;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagNewChangeDataSet : HapagBaseApplicationDataSet {

        public HapagNewChangeDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, MaximoHibernateDAO maxDao)
            : base(locationManager, entityRepository, maxDao) {
        }




        #region PrefilterFunctions



        public IEnumerable<IAssociationOption> GetInfrastructureAssetToChange(OptionFieldProviderParameters parameters) {
            var entityMetadata = MetadataProvider.Entity("ci");
            var result = EntityRepository.Get(entityMetadata, new SearchRequestDto());
            var attributeHolders = result as AttributeHolder[] ?? result.ToArray();
            var options = new HashSet<IAssociationOption>();
            foreach (var attributeHolder in attributeHolders) {
                var ciname = attributeHolder.GetAttribute("ciname");
                if (!DataSetUtil.IsValid(ciname, typeof(String))) {
                    continue;
                }
                options.Add(new AssociationOption((string)attributeHolder.GetAttribute("cinum"), (string)ciname));
            }

            return options;
        }

        #endregion

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {            
            var dbDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;
            if (resultObject == null) {
                //it happens only if we´re pointint to a database different then the ws impl
                return dbDetail;
            }

            if (application.Schema.SchemaId == "creationSummary") {
                HandleCreationSummary(resultObject);
            }
            
            return dbDetail;
        }

        private static void HandleCreationSummary(DataMap resultObject) {
            var list = resultObject.GetAttribute("attachment_");
            var sb = new StringBuilder();
            foreach (var dictionary in (IEnumerable<Dictionary<string, object>>)list) {
                sb.Append("<p>");
                var urlDescription = AttachmentHandler.BuildFileName(dictionary["docinfo_.urlname"] as string);
                if (urlDescription == null) {
                    //keep description
                    dictionary["urldescription"] = dictionary["description"];
                } else {
                    dictionary["urldescription"] = urlDescription;
                }
                sb.Append(dictionary["urldescription"]);
                sb.Append("</p>");
            }
            resultObject.Attributes.Add("#attachmentsummary", sb.ToString());
        }

        public override string ApplicationName() {
            return "newchange";
        }
    }
}
