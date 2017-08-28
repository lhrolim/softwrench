using System;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {

    public class BaseRedisInsertKey {

        public string Key { get; set; }
        public bool ReplaceExisting { get; set; } = true;
        public TimeSpan? ExpiresIn { get; set; }

        public BaseRedisInsertKey(string key) {
            Key = key;
        }

        public BaseRedisInsertKey(string key, TimeSpan? expiresIn) {
            Key = key;
            ExpiresIn = expiresIn;
        }

    }
}