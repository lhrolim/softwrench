using System;

namespace softWrench.sW4.Data.Persistence.WS.API {
    /// <summary>
    /// Exception to indicate an error happened while trying to resolve
    /// a Maximo's WebService's contract (i.e. download it's WSDL description)
    /// either because of misconfiguration on the client part (wrong uri, credentials, etc)
    /// or because the WebService was not correctly deployed at the expected server location.
    /// Extends <see cref="MaximoException"/>.
    /// </summary>
    public class MaximoWebServiceNotResolvedException : MaximoException {
        private const string OUTLINE_PREFIX = "An error happened while trying to resolve a Maximo's WebService's contract " +
                                              "either because of misconfiguration on the client part (wrong uri, credentials, etc) " +
                                              "or because the WebService was not correctly deployed at the expected location.\n";

        public MaximoWebServiceNotResolvedException(Exception immediateCause, Exception rootCause) : base(immediateCause, rootCause) { }
        public MaximoWebServiceNotResolvedException(string message, Exception immediateCause, Exception rootCause) : base(message, immediateCause, rootCause) { }

        public override string OutlineInformation {
            get {
                return OUTLINE_PREFIX + base.OutlineInformation;
            }
        }
    }
}
