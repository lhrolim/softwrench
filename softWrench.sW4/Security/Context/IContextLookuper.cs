﻿using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Security.Context {
    public interface IContextLookuper : ISingletonComponent {
        /// <summary>
        /// Retrieves a context for the current thread invocation
        /// </summary>
        /// <returns></returns>
        ContextHolder LookupContext();

        /// <summary>
        /// Fills current thread invocation context
        /// </summary>
        /// <param name="key"></param>
        void FillContext(ApplicationMetadataSchemaKey key);

        /// <summary>
        /// Sets an object in the memory context, that would be accessible by any thread
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ob"></param>
        /// <param name="userSpecific">if true, this object will be visible only for the current user</param>
        void SetMemoryContext(string key, object ob,bool userSpecific=false);

        /// <summary>
        /// Sets an object in the memory context, that would be accessible by any thread
        /// </summary>
        /// <param name="key"></param>
        /// <param name="userSpecific">if true, this object will be visible only for the current user</param>
        void RemoveFromMemoryContext(string key, bool userSpecific = false);

        /// <summary>
        /// Retrieves an object from the memory context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="userSpecific"></param>
        /// <returns></returns>
        T GetFromMemoryContext<T>(string key,bool userSpecific = false);

    }
}