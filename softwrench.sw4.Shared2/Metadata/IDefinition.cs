using System;
using System.Collections;
using System.Collections.Generic;
using softwrench.sw4.Shared2.Metadata;

namespace softwrench.sW4.Shared2.Metadata {
    public interface IDefinition{
        /// <summary>
        /// Used to store generic data on a definition class. 
        /// This can be handy for custom data that needs to be stored on a ClientPlataform. 
        /// It should never be populated from the metadata parsing itself, only custom adapter scenarios.
        /// </summary>
        object ExtensionParameter(string key);
        void ExtensionParameter(string key, object value);
    }
}