using System;

namespace softWrench.sW4.Metadata.Validator {

    /// <summary>
    /// Used to identify a entity property as the user id property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UserIdProperty : Attribute {
    }
}
