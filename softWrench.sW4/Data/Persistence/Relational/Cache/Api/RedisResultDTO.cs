using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {

    public class RedisResultDTO<T> where T : DataMap {


        public RedisResultDTO(string realkey, IList<T> results) {
            RealKey = realkey;
            Results = results;
        }

        public IList<T> Results { get; set; }

        public string RealKey { get; set; }


    }
}
