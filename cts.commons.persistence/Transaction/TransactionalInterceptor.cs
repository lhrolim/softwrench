using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Castle.DynamicProxy;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace cts.commons.persistence.Transaction {

    /// <summary>
    /// Interceptor of methods of IComponents and Controllers with [[Transactional]] attribute.
    /// </summary>
    public class TransactionalInterceptor : IInterceptor, ISingletonComponent {

        private const string TransactionContextKey = "TransactionContext";
        private readonly SimpleInjectorGenericFactory _factory;

        // forces SimpleInjectorGenericFactory to be resolved first, do not remove
        public TransactionalInterceptor(SimpleInjectorGenericFactory factory) {
            _factory = factory;
        }

        public void Intercept(IInvocation invocation) {
            // is not transactional
            var attributes = invocation.Method.GetCustomAttributes(typeof(Transactional), true);
            if (attributes.Length == 0) {
                invocation.Proceed();
                return;
            }

            var transactional = (Transactional)attributes[0];
            var contexts = transactional.DbTypes.Select(GetContext).ToList();

            // Creates or reuses Session and Transation
            BeforeInvocation(contexts);

            // Invokes the method (if async wait it to end)
            // Rollbacks if needed
            AsyncHelper.RunSync(() => Invocation(invocation, contexts));

            // Commits the transaction and close the session when needed
            AfterInvocation(contexts);
        }

        public static TransactionContext GetContext(DBType type) {
            var contexts = CallContext.LogicalGetData(TransactionContextKey) as Dictionary<DBType, TransactionContext>;
            if (contexts == null) {
                contexts = new Dictionary<DBType, TransactionContext>();
                CallContext.LogicalSetData(TransactionContextKey, contexts);
            }

            if (contexts.ContainsKey(type)) {
                return contexts[type];
            }

            var context = new TransactionContext { DbType = type };
            contexts.Add(type, context);
            return context;
        }

        public static TransactionContext GetContext(IBaseHibernateDAO dao) {
            var swdb = dao as ISWDBHibernateDAO;
            return GetContext(swdb != null ? DBType.Swdb : DBType.Maximo);
        }

        protected virtual void BeforeInvocation(List<TransactionContext> contexts) {
            contexts.ForEach(context => AsyncHelper.RunSync(() => BeforeInvocation(context)));
        }

        protected virtual async Task BeforeInvocation(TransactionContext context) {
            // increments counter -> used to know when to close session and tx
            context.TransactionCounter++;
            // has tx already
            if (context.Transaction != null) {
                return;
            }

            // open session and transaction
            var dao = GetDao(context);
            context.Session = dao.GetSession();

            context.Transaction = await dao.BeginTransactionAsync(context.Session);
            context.TransactionManaged = true;
        }

        protected virtual async Task Invocation(IInvocation invocation, List<TransactionContext> contexts) {
            try {
                invocation.Proceed();
                var invocationResult = invocation.ReturnValue;
                var task = invocationResult as Task;
                if (task != null) {
                    await task;
                }
            } catch (Exception) {
                RollbackTx(contexts);
                contexts.ToList().ForEach(context => Clear(context, true));
                throw;
            }
        }

        protected virtual void AfterInvocation(List<TransactionContext> contexts) {
            contexts.ForEach(context => {
                try {
                    CommitTx(context);
                    Clear(context);
                    context.TransactionCounter--;
                } catch (Exception) {
                    InnerRollbackTx(context, true);
                    Clear(context, true);
                    throw;
                }
            });
        }

        protected virtual void CommitTx(TransactionContext context) {
            var tx = context.Transaction;
            if (tx != null && tx.IsActive && context.TransactionCounter == 1) {
                tx.Commit();
            }
        }

        protected virtual void InnerRollbackTx(TransactionContext context, bool force = false) {
            var tx = context.Transaction;
            if (tx != null && tx.IsActive && (context.TransactionCounter == 1 || force)) {
                tx.Rollback();
            }
        }

        protected virtual void RollbackTx(List<TransactionContext> contexts) {
            contexts.ForEach(context => InnerRollbackTx(context));
        }

        protected virtual void Clear(TransactionContext context, bool force = false) {
            if (context.TransactionCounter != 1 && !force) {
                return;
            }
            context.Session?.Dispose();
            context.Transaction?.Dispose();
            // clears the transaction context
            if (context.Session != null && context.Session.IsOpen) {
                context.Session.Close();
            }
            context.Session = null;
            context.Transaction = null;
            context.TransactionManaged = false;
        }

        protected virtual IBaseHibernateDAO GetDao(TransactionContext context) {
            return context.DbType == DBType.Swdb ? (IBaseHibernateDAO)_factory.GetObject<ISWDBHibernateDAO>() : _factory.GetObject<IMaximoHibernateDAO>();
        }
    }
}
