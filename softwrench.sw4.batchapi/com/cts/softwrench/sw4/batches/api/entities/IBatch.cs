using cts.commons.persistence;

namespace softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities {
    public interface IBatch : IBaseAuditEntity {
        int NumberOfItems { get; }
    }
}