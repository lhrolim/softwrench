using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace softWrench.sW4.Util {
    public static class TaskFactoryExtensions {

        private static readonly ILog Log = LogManager.GetLogger(typeof(TaskFactoryExtensions));


        /// <summary>
        ///  Utility decorator method that allows proper logging of runtime exceptions
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static Task NewThread(this TaskFactory factory, Action<object> action, object state) {


            return factory.StartNew(t => {
                try {
                    action(state);
                } catch (Exception e) {
                    var rootException = ExceptionUtil.DigRootException(e);
                    Log.Error(rootException, e);
                    throw;
                }
            }
            , state);
        }

        public static Task NewThread(this TaskFactory factory, Action action) {


            return factory.StartNew(() => {
                try {
                    action();
                } catch (Exception e) {
                    var rootException = ExceptionUtil.DigRootException(e);
                    Log.Error(rootException, e);
                    throw;
                }
            });
        }
    }
}
