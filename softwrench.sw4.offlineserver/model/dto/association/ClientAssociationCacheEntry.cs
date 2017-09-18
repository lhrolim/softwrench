namespace softwrench.sw4.offlineserver.model.dto.association {
    public class ClientAssociationCacheEntry {

        public string MaxRowstamp { get; set; }
        public string MaxUid { get; set; }
        public string WhereClauseHash { get; set; }
        public string SyncSchemaHash { get; set; }

    }
}
