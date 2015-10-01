using System;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.API {
    /// <summary>
    /// Exception to indicate an error when invoking Maximo's Web Service
    /// </summary>
    public class MaximoException : Exception {

        private readonly Exception _immediate;
        private readonly Exception _root;
        
        public Exception ImmediateCause { get { return _immediate; } }
        public Exception RootCause { get { return _root; } }
        public string Type { get { return GetType().Name; } }
        public string FullStackTrace { get { return RootCause.StackTrace + "\n" + ImmediateCause.StackTrace + "\n" + StackTrace; } }
        public string OutlineInformation {
            get {
                var immediateLine = ExceptionUtil.LastStackTraceLine(ImmediateCause);
                var rootLine = ExceptionUtil.LastStackTraceLine(RootCause);
                return string.Format("[immediate]: {0}\n [root]: {1}", immediateLine, rootLine);
            }
        }

        /// <summary>
        /// Creates a MaximoException instance setting the rootCause.Message as this.Message, 
        /// rootCause as this.RootCause and immediateCause as this.ImmediateCause
        /// </summary>
        /// <param name="immediateCause"></param>
        /// <param name="rootCause"></param>
        public MaximoException(Exception immediateCause, Exception rootCause) : base(rootCause.Message) {
            _immediate = immediateCause;
            _root = rootCause;
        }
        
    }
}
