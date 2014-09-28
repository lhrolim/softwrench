using System;

namespace softWrench.Mobile.Exceptions
{
    [Serializable]
    public class InvalidSettingsException : Exception
    {
        public InvalidSettingsException()
            : base(null, null)
        {
        }

        public InvalidSettingsException(string message)
            : base(message, null)
        {
        }

        public InvalidSettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
