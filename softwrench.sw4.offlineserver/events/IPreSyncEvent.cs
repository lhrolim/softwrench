using softwrench.sw4.offlineserver.dto;

namespace softwrench.sw4.offlineserver.events {
    public class PreSyncEvent {
        private readonly SynchronizationRequestDto _request;

        public PreSyncEvent(SynchronizationRequestDto request) {
            _request = request;
        }

        public SynchronizationRequestDto Request {
            get { return _request; }
        }
    }
}
