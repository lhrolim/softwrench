using System;
using System.Collections.Generic;

namespace cts.commons.persistence.Transaction {

    /// <summary>
    /// Attribute to be added to a method to make it transactional. If any exception is raised during the method invocation the transaction is rolledback.
    /// This Attribute should only be added on classes that will be added on SimpleInjector Container (classes that extends IComponent or Controllers).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Transactional : Attribute {

        /// <summary>
        /// Multiple transactions can be creted each for a different db.
        ///  </summary>
        /// <param name="type">The type of the db that a transaction will be created</param>
        /// <param name="types">The extra type of db's that a transaction will be created.</param>
        public Transactional(DBType type, params DBType[] types) {
            DbTypes = new List<DBType> { type };
            DbTypes.AddRange(types);
        }

        public List<DBType> DbTypes { get; private set; }
    }
}
