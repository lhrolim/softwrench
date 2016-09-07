using softWrench.sW4.Audit;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Base {
    abstract class BaseISMDecorator : CrudConnectorDecorator {
        protected SWDBHibernateDAO DAO {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var trail = DAO.Save(ISMAuditTrail.GetInstance(ISMAuditTrail.TrailType.Creation, maximoTemplateData.ApplicationMetadata));
            var ob = maximoTemplateData.IntegrationObject;
            var transaction = ReflectionUtil.GetProperty(ob, "Transaction") as Transaction;
            if (transaction == null) {
                transaction = new Transaction();
                ReflectionUtil.SetProperty(ob, "Transaction", transaction);
            }
            PopulateTransaction(transaction, trail);
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var trail = DAO.Save(ISMAuditTrail.GetInstance(ISMAuditTrail.TrailType.Update, maximoTemplateData.ApplicationMetadata));
            var ob = maximoTemplateData.IntegrationObject;
            var transaction = ReflectionUtil.GetProperty(ob, "Transaction") as Transaction;
            if (transaction == null) {
                transaction = new Transaction();
                ReflectionUtil.SetProperty(ob, "Transaction", transaction);
            }
            PopulateTransaction(transaction, trail);
        }

        protected void PopulateTransaction(Transaction transaction, ISMAuditTrail trail) {
            transaction.TransactionNumber = trail.Id.ToString();
            transaction.TransactionRouting = trail.Routing;
            transaction.TransactionType = trail.Type;
            transaction.TransactionName = trail.Name;
            transaction.TransactionDateTime = trail.BeginTime;
            transaction.DataSource = "ServiceIT";
        }


    }
}
