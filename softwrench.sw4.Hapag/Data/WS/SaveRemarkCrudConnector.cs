using cts.commons.simpleinjector;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.Hapag.Data.WS {
    public class SaveRemarkCrudConnector : BaseMaximoCustomConnector {

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

        public override string ApplicationName() {
            return "asset";
        }

        public override string ActionId() {
            return "saveremarks";
        }
    }
}
