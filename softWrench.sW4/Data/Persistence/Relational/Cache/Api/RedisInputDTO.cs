using System.Collections.Generic;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {
    public class RedisInputDTO <T> where T: DataMap{

        /// <summary>
        /// List of items to insert into cache. Must be sorted by uid asc (preferrably) or desc (internally a reverse will get called)
        /// </summary>
        public IList<T> Datamaps { get; set; }

        public RedisInputDTO([NotNull]IList<T> datamaps) {
            Datamaps = datamaps;
        }

        //        public long? MaxRowstamp { get; set; }

        //        public int? ChunkNumber { get; set; }

        //        public void AdjustMaxRowstamp() {
        //            if (MaxRowstamp == null) {
        //
        //            }
        //        }
    }
}
