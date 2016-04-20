using System;

namespace softWrench.sW4.Exceptions {

    /// <summary>
    /// Exception class defining an invalid where clause.
    /// </summary>
    public class InvalidWhereClauseException : InvalidOperationException {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidWhereClauseException"></see> class with a specified error message.
        /// </summary>
        /// <param name="message">the error message.</param>
        /// <param name="innerException"></param>
        public InvalidWhereClauseException(string message, Exception innerException=null) : base(message, innerException) { }
    }
}
