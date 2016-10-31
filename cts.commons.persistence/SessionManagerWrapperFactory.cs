using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.app;
using JetBrains.Annotations;

namespace cts.commons.persistence {
    public class SessionManagerWrapperFactory {

        private static IDictionary<string, SessionManagerWrapper> _cache = new ConcurrentDictionary<string, SessionManagerWrapper>();

        public static SessionManagerWrapper GetInstance(string connectionString, string driverName, string dialect, [CanBeNull] IEnumerable<Assembly> assembliesToLookupForMappings, IApplicationConfiguration applicationConfiguration) {
            if (_cache.ContainsKey(connectionString)) {
                return _cache[connectionString];
            }
            var result =new SessionManagerWrapper(connectionString,driverName,dialect,assembliesToLookupForMappings,applicationConfiguration);
            _cache.Add(connectionString,result);
            return result;

        }

    }
}
