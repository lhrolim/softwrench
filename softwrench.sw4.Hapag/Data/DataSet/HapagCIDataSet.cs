using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.DataSet {
    public class HapagCIDataSet : BaseApplicationDataSet {


        private SWDBHibernateDAO _dao;

        private SWDBHibernateDAO GetDAO() {
            if (_dao == null) {
                _dao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
            return _dao;
        }


        public override CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {
            var compData = base.GetCompositionData(application, request, currentData);
            HandleCiSpecMapping(compData.ResultObject["cispec_"].ResultList);
            return compData;
        }

        private void HandleCiSpecMapping(IList<Dictionary<string, object>> ciSpecs) {
            var mappings = GetDAO().FindAll<CiSpecMapping>(typeof(CiSpecMapping));
            foreach (var spec in ciSpecs) {
                var originalValue = spec["assetattrid"].ToString();
                var m = mappings.FirstOrDefault(f => f.MaximoValue.EqualsIc(originalValue));
                if (m != null) {
                    spec.Add("#normalizedattribute", m.ServiceITValue);
                } else {
                    spec.Add("#normalizedattribute", originalValue);
                }
            }
        }


        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var baseDetail = base.GetApplicationDetail(application, user, request);
            FetchRemarks(baseDetail.ResultObject);
            return baseDetail;
        }

        private void FetchRemarks(DataMap resultObject) {
            var ciId = resultObject.GetAttribute("ciid");
            var extraAttributte = GetDAO().FindSingleByQuery<ExtraAttributes>(ExtraAttributes.ByMaximoTABLEIdAndAttribute, "ci", ciId.ToString(), "remarks");
            if (extraAttributte != null) {
                resultObject.SetAttribute(extraAttributte.AttributeName, extraAttributte.AttributeValue);
            }
        }

        public override string ApplicationName() {
            return "ci";
        }

        public override string ClientFilter() {
            return "hapag";
        }


    }
}
