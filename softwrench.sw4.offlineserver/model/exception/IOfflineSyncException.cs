using softwrench.sw4.api.classes.exception;

namespace softwrench.sw4.offlineserver.model.exception {
    public interface IOfflineSyncException : IBaseSwException, IStatusCodeException {

        bool RequestSupportReport { get; }



    }
}