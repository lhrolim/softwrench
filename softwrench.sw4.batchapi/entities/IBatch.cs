using cts.commons.persistence;

namespace softwrench.sw4.batch.api.entities {
    public interface IBatch : IBaseAuditEntity {
        int NumberOfItems { get; }
    }
}