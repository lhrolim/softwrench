using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.persistence;
using softwrench.sw4.dynforms.classes.model.metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.SPF;

namespace softwrench.sw4.dynforms.classes.controller {

    public class FormMetadataController : ApiController {

        [Import]
        public DynFormSchemaHandler DynSchemaHandler { get; set; }


        [HttpPost]
        public async Task<IGenericResponseResult> StoreDetailForm(string formName, [FromBody]JSonWrapper json) {

            await DynSchemaHandler.ReplaceDetailDisplayables(formName, json.NewFields);

            return new BlankApplicationResponse() { SuccessMessage = $"Form {formName} saved successfully" };

        }

        public class JSonWrapper
        {
            public string NewFields { get; set; }
        }

    }
}
