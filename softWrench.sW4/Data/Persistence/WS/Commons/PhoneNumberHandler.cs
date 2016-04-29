using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Util;
using WsUtil = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using softWrench.sW4.wsWorkorder;
using cts.commons.simpleinjector;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class PhoneNumberHandler : ISingletonComponent {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rootObject"></param>
        /// <returns>An object to be deleted on Maximo--> this is needed in case of an update since maximo creates a new instance everytime (cause he considers the triple number type and personid to be the key, instead of the id). Therefore we´ll update by doing the insert plus a delete</returns>
        public CrudOperationData HandlePhoneNumbers(CrudOperationData entity, object rootObject) {

            if (entity.GetAttribute("#primaryphone") != null) {
                //new users, let´s generate adapt to a composition email to store it in maximo
                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(rootObject, "PHONE", 1);
                var email = arr.GetValue(0);
                ReflectionUtil.SetProperty(email, "action", ProcessingActionType.AddChange.ToString());
                WsUtil.SetValue(email, "isprimary", true, true);
                WsUtil.SetValue(email, "phonenum", entity.GetAttribute("#primaryphone"), true);
                WsUtil.SetValue(email, "type", "WORK", true);
                return null;
            }
            CrudOperationData result = null;
            var phones = ((IEnumerable<CrudOperationData>)entity.GetRelationship("phone")).Where(w => w.UnmappedAttributes.ContainsKey("#isDirty")).ToArray();
            WsUtil.CloneArray(phones, rootObject, "PHONE", delegate (object integrationObject, CrudOperationData crudData) {

                ReflectionUtil.SetProperty(integrationObject, "action", ProcessingActionType.AddChange.ToString());
                ReflectionUtil.InstantiateAndSetIfNull(integrationObject, "type");

                if (crudData.ContainsAttribute("#originalphonenum") && isDifferent(crudData)) {
                    IDictionary<string, object> attributes = new Dictionary<string, object>()
                    {
                        {"phonenum",crudData.GetAttribute("#originalphonenum") },
                        { "type",crudData.GetAttribute("#originaltype") },
                        { "personid",crudData.GetAttribute("personid") },
                    };
                    result = new CrudOperationData(crudData.Id, attributes, new Dictionary<string, object>(), MetadataProvider.Entity("phone"), MetadataProvider.Application("phone").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail")));
                }
            });
            return result;
        }

        private bool isDifferent(CrudOperationData crudData) {
            return !crudData.GetAttribute("#originalphonenum").Equals(crudData.GetAttribute("phonenum")) ||
                   !crudData.GetAttribute("type").Equals(crudData.GetAttribute("#originaltype"));
        }
    }
}

