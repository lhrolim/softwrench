using System;
using System.Collections;
using System.Collections.Generic;

namespace softwrench.sW4.Shared.Metadata
{
    public interface IDefinition
    {
        /// <summary>
        /// Used to store generic data on a definition class. 
        /// This can be handy for custom data that needs to be stored on a ClientPlataform. 
        /// It should never be populated from the metadata parsing itself, only custom adapter scenarios.
        /// </summary>
        IDictionary<String, object> ExtensionParameters { get; set; } 
    }
}