using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.persistence;
using cts.commons.web.Attributes;
using softwrench.sw4.dynforms.classes.dataset;
using softwrench.sw4.dynforms.classes.model.metadata;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dynforms.classes.controller {

    [Authorize]
    [SWControllerConfiguration]
    public class FormMetadataController : ApiController {

        [Import]
        public DynFormSchemaHandler DynSchemaHandler { get; set; }

        [Import]
        public FormMetadataOptionsProvider OptionsProvider { get; set; }

        [HttpPost]
        public async Task<IGenericResponseResult> StoreDetailForm(string formName, [FromBody]JSonWrapper json) {

            await DynSchemaHandler.ReplaceDetailDisplayables(formName, json.NewFields);

            return new BlankApplicationResponse { SuccessMessage = $"Form {formName} saved successfully" };

        }

        [HttpGet]
        public IEnumerable<IAssociationOption> EagerLoadOptions(string fieldQualifier) {
            //TODO: make it async, but GenericSWMethodInvoker isn´t designed to accept it
            return OptionsProvider.DoGetAssociationOptions(fieldQualifier);
        }

        public class JSonWrapper {
            public string NewFields { get; set; }
        }

    }
}
