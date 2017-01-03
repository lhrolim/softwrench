using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace cts.commons.persistence.Transaction {
    public class TransactionalHelper {
        public static bool IsTransactionable(Type type) {
            var allMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).ToList();
            var transactionableMethods = allMethods.Where(method => method.GetCustomAttributes(typeof(Transactional), true).Length > 0).ToList();
            if (!transactionableMethods.Any()) {
                return false;
            }

            transactionableMethods.ForEach(method => {
                if (!method.IsPublic || !method.IsVirtual) {
                    throw new Exception(string.Format("{0} needs to be public and virtual to work with [Transactional].", MethodExGelper(method, type)));
                }

                if (method.ReturnType != typeof(void)) {
                    return;
                }
                var isAsync = method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
                if (!isAsync) {
                    return;
                }
                var sb = new StringBuilder();
                sb.Append(MethodExGelper(method, type));
                sb.Append(" cannot be async void and have the [Transactional] attribute.");
                sb.Append(" Consider the creation of internal method and add the [Transactional] to it.");
                throw new Exception(sb.ToString());
            });

            return true;
        }

        private static string MethodExGelper(MethodInfo method, Type type) {
            return string.Format("The method {0} of type {1}", method.Name, type.Name);
        }
    }
}
