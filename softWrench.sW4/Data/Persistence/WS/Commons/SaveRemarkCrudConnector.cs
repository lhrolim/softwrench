using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.Internal;
using cts.commons.simpleinjector;

//TODO: move to hapag --> use simpleinjector
namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class SaveRemarkCrudConnector : BaseMaximoCustomConnector {
        private static SWDBHibernateDAO Dao() {
            return SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
        }

        public object SaveRemarks(AssetRemarkContainer assetRemark) {
            var extraField = Dao().FindSingleByQuery<ExtraAttributes>(ExtraAttributes.ByMaximoTABLEIdAndAttribute, "asset", assetRemark.Id,
                "remarks");
            if (extraField == null) {
                extraField = new ExtraAttributes {
                    AttributeName = "remarks",
                    MaximoId = assetRemark.Id,
                    MaximoTable = "asset"
                };
            }
            extraField.AttributeValue = assetRemark.Remark;
            return Dao().Save(extraField);
        }

        public class AssetRemarkContainer : OperationData {


            public string Remark;

        }

    }
}
