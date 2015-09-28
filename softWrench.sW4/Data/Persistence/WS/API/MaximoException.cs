using System;

namespace softWrench.sW4.Data.Persistence.WS.API {
    /// <summary>
    /// Exception to indicate an error when invoking Maximo's Web Service
    /// </summary>
    public class MaximoException : Exception {
        public MaximoException(Exception cause) : base(cause.Message, cause) { }
        public MaximoException(string message, Exception cause) : base(message, cause) { }
    }
}
