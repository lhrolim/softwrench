using System;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using JetBrains.Annotations;

namespace softWrench.sW4.Mapping {
    public interface IMappingResolver :ISingletonComponent{

        /// <summary>
        /// Resolves the original values passed for the given key type to a list of values to be applied
        /// </summary>
        /// <param name="key">the catalog key</param>
        /// <param name="originalValues">The catalog value</param>
        /// <returns></returns>
        [NotNull]
        IReadOnlyList<string> Resolve([NotNull]string key, [CanBeNull]IEnumerable<string> originalValues);


    }
}