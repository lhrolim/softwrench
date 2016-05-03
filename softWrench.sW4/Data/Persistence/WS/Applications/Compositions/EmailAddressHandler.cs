using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;
using softWrench.sW4.wsWorkorder;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    class EmailAddressHandler {

        public EmailAddressHandler() {



        }

        public static CrudOperationData HandleEmailAddress(CrudOperationData entity, object rootObject) {

            if (entity.GetAttribute("#primaryemail") != null) {
                //new users, let´s generate adapt to a composition email to store it in maximo
                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(rootObject, "EMAIL", 1);
                var email = arr.GetValue(0);
                ReflectionUtil.SetProperty(email, "action", ProcessingActionType.AddChange.ToString());
                WsUtil.SetValue(email, "isprimary", true, true);
                WsUtil.SetValue(email, "EMAILADDRESS", entity.GetAttribute("#primaryemail"), true);
                WsUtil.SetValue(email, "type", "WORK", true);
                return null;
            }

            CrudOperationData result = null;

            var emailAddress =
                ((IEnumerable<CrudOperationData>)entity.GetRelationship("email")).Where(
                    w => w.UnmappedAttributes.ContainsKey("#isDirty")).ToArray();
            WsUtil.CloneArray(emailAddress, rootObject, "EMAIL", delegate (object integrationObject, CrudOperationData crudData) {
                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                ReflectionUtil.InstantiateAndSetIfNull(integrationObject, "type");
                if (crudData.ContainsAttribute("#originalemailaddress") && isDifferent(crudData)) {

                    IDictionary<string, object> attributes = new Dictionary<string, object>()
                    {
                        {"emailaddress",crudData.GetAttribute("#originalemailaddress") },
                        { "type",crudData.GetAttribute("#originaltype") },
                        { "personid",crudData.GetAttribute("personid") },
                    };
                    result = new CrudOperationData(crudData.Id, attributes, new Dictionary<string, object>(), MetadataProvider.Entity("email"), MetadataProvider.Application("email").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail")));
                }
            });
            return result;
        }


        private static bool isDifferent(CrudOperationData crudData) {
            return !crudData.GetAttribute("#originalemailaddress").Equals(crudData.GetAttribute("emailaddress")) ||
                   !crudData.GetAttribute("type").Equals(crudData.GetAttribute("#originaltype"));
        }
    }
}

