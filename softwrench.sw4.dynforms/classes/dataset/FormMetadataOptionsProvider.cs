using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softwrench.sw4.dynforms.classes.dataset {


    public class FormMetadataOptionsProvider : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO SWDAO { get; set; }

        public IEnumerable<IAssociationOption> GetAvailableOptions(OptionFieldProviderParameters parameters) {
            var optionProviderId = parameters.OptionField.Qualifier;
            return DoGetAssociationOptions(optionProviderId);
        }

        public IEnumerable<IAssociationOption> DoGetAssociationOptions(string optionProviderId) {
            var metadataOption = SWDAO.FindByPK<FormMetadataOptions>(typeof(FormMetadataOptions),Int32.Parse(optionProviderId));
            var itemsSt = metadataOption.ListDefinitionStringValue;
            var items = JArray.Parse(itemsSt);
            var list = new List<AssociationOption>();
            foreach (var jToken in items) {
                list.Add(new AssociationOption(jToken["value"].ToString(), jToken["label"].ToString()));
            }

            return list;
        }
    }
}
