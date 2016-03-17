﻿using System;
using JetBrains.Annotations;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.API {
    /// <summary>
    /// Exception to indicate an error when invoking Maximo's Web Services
    /// </summary>
    public class MaximoException : Exception {

        private readonly Exception _immediate;
        private readonly Exception _root;
        
        public Exception ImmediateCause { get { return _immediate; } }
        public Exception RootCause { get { return _root; } }
        public virtual string FullStackTrace { get { return RootCause.StackTrace + "\n" + ImmediateCause.StackTrace + "\n" + StackTrace; } }
        public virtual string OutlineInformation {
            get {
                var outline = "";
                if (ImmediateCause != null) {
                    outline += ("[immediate]:\n" + ExceptionUtil.LastStackTraceLine(ImmediateCause) + "\n");
                }
                if (RootCause != null) {
                    outline += ("[root]:\n" + ExceptionUtil.LastStackTraceLine(RootCause) + "\n");
                }
                return outline;
            }
        }

        public MaximoException([NotNull]string message) : base(message) { }

        /// <summary>
        /// Creates a MaximoException instance setting rootCause as this.RootCause 
        /// and immediateCause as this.ImmediateCause
        /// </summary>
        /// <param name="message"></param>
        /// <param name="immediateCause"></param>
        /// <param name="rootCause"></param>
        public MaximoException([NotNull]string message, [NotNull]Exception immediateCause, [NotNull]Exception rootCause) : base(message) {
            _immediate = immediateCause;
            _root = rootCause;
        }

        /// <summary>
        /// Creates a MaximoException instance setting the rootCause.Message as this.Message, 
        /// rootCause as this.RootCause and immediateCause as this.ImmediateCause
        /// </summary>
        /// <param name="immediateCause"></param>
        /// <param name="rootCause"></param>
        public MaximoException([NotNull]Exception immediateCause, [NotNull]Exception rootCause) : base(rootCause.Message) {
            _immediate = immediateCause;
            _root = rootCause;
        }
        
    }
}
