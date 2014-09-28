using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.SimpleInjector;

//TODO: move to hapag --> use simpleinjector
namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class SaveRemarkCrudConnector : BaseMaximoCustomConnector {
        private readonly SWDBHibernateDAO _dao;

        public SaveRemarkCrudConnector() {
            _dao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
        }

        public object SaveRemarks(AssetRemarkContainer assetRemark) {
            var extraField = _dao.FindSingleByQuery<ExtraAttributes>(ExtraAttributes.ByMaximoTABLEIdAndAttribute, "asset", assetRemark.Id,
                "remarks");
            if (extraField == null) {
                extraField = new ExtraAttributes {
                    AttributeName = "remarks",
                    MaximoId = assetRemark.Id,
                    MaximoTable = "asset"
                };
            }
            extraField.AttributeValue = assetRemark.Remark;
            return _dao.Save(extraField);
        }

        public class AssetRemarkContainer : OperationData {


            public string Remark;

        }

    }
}
