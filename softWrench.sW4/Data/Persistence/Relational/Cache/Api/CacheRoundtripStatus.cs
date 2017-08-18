namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {

    public class CacheRoundtripStatus {

        public bool Complete { get; set; }

        /// <summary>
        /// Client side position, a persistent postion, meaning it was already inserted
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// not yet inserted, subject of SyncChunkHandler
        /// </summary>
        public int TransientPosition { get; set; }

        public string Application { get; set; }

    }
}
