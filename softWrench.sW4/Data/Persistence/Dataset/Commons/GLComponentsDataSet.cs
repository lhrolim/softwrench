using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    class GLComponentDataSet : MaximoApplicationDataSet {
        public IEnumerable<IAssociationOption> GetGLComponents71(OptionFieldProviderParameters parameters) {
            return GlComponents(parameters, null);
        }

        public IEnumerable<IAssociationOption> GetGLComponents75(OptionFieldProviderParameters parameters) {
            var user = SecurityFacade.CurrentUser();
            var orgid = parameters.OriginalEntity.Attributes.Any() ? parameters.OriginalEntity.Attributes["orgid"] : null;

            if (orgid == null) orgid = user.OrgId;

            return GlComponents(parameters, String.Format("and orgid = '{0}'", orgid));
        }

        private IEnumerable<IAssociationOption> GlComponents(OptionFieldProviderParameters parameters, string additionalFilters) {
            if (parameters.OptionField.ExtraParameter == null) {
                return new List<IAssociationOption>();
            }

            var user = SecurityFacade.CurrentUser();

            var orgid = parameters.OriginalEntity.Attributes.Any() ? parameters.OriginalEntity.Attributes["orgid"] : null;

            if (orgid == null) {
                orgid = user.OrgId;
            }

            //in oracle, || should be used instead of +
            var concatenator = ApplicationConfiguration.IsOracle(DBType.Maximo) ? "||" : "+";

            // default orgid if none was found. this is usually the case if it is a new entry.
            var query = String.Format(@"SELECT compvalue, compvalue {0} ' - ' {0} comptext AS comptext FROM glcomponents
                                        where glorder = {1} and active = 1 and orgid = '{2}'", concatenator, parameters.OptionField.ExtraParameter, orgid);

            var results = MaxDAO.FindByNativeQuery(query, null);

            if (results.Any()) {
                var gllengthquery = String.Format(@"SELECT gllength FROM glconfigure
                                                        where glorder = {0} {1}", parameters.OptionField.ExtraParameter, additionalFilters);

                var gllengthresult = MaxDAO.FindSingleByNativeQuery<object>(gllengthquery, null);

                var items = new List<IAssociationOption>();

                // ???,???? etc
                var questionMarks = " " + new String('?', Convert.ToInt32(gllengthresult));

                items.Add(new AssociationOption(questionMarks, " " + questionMarks));

                items.AddRange(results.Select(result => new AssociationOption(result["compvalue"], result["comptext"])));

                return items;
            }

            return new List<IAssociationOption>();
        }

        public override string ApplicationName() {
            return "glcomponents";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}