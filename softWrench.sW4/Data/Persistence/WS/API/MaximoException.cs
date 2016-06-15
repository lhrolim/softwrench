using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;
using JetBrains.Annotations;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.API {
    /// <summary>
    /// Exception to indicate an error when invoking Maximo's Web Services
    /// </summary>
    public class MaximoException : Exception {

        private readonly Exception _immediate;
        private readonly Exception _root;

        private readonly string _soapStack;

        public Exception ImmediateCause {
            get {
                return _immediate;
            }
        }
        public Exception RootCause {
            get {
                return _root;
            }
        }

        public virtual string FullStackTrace {
            get {
                var sb = new StringBuilder();
                sb.Append(RootCause != null ? (RootCause.StackTrace + "\n") : "");
                sb.Append(ImmediateCause != null ? (ImmediateCause.StackTrace + "\n") : "");
                if (_soapStack != null) {
                    sb.Append(_soapStack);
                } else {
                    sb.Append(StackTrace);
                }
                return sb.ToString();

            }
        }

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
        /// <param name="additionalMessage"></param>
        public MaximoException([NotNull]Exception immediateCause, [NotNull]Exception rootCause, string additionalMessage = null) : base(rootCause.Message + " " + (additionalMessage ?? "")) {
            if (rootCause is SoapException) {
                var ex = (SoapException)rootCause;
                if (ex.Detail != null) {
                    _soapStack = ex.Detail.InnerText;
                }
            }

            _immediate = immediateCause;
            _root = rootCause;
        }


        public static Exception ParseWebExceptionResponse(WebException webException) {
            var responseStream = webException.Response.GetResponseStream();
            if (responseStream == null) {
                var rootException = ExceptionUtil.DigRootException(webException);
                return new MaximoException(webException, rootException);
            }

            using (var responseReader = new StreamReader(responseStream)) {
                // parse xml response
                var text = responseReader.ReadToEnd();
                var rootException = ExceptionUtil.DigRootException(webException);
                return new MaximoException(webException, rootException, text);
            }
        }

    }
}
