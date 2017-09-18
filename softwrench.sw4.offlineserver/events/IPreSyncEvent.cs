using cts.commons.simpleinjector.Events;
using softwrench.sw4.offlineserver.model.dto;

namespace softwrench.sw4.offlineserver.events {
    public class PreSyncEvent : ISWEvent {
        private readonly BaseSynchronizationRequestDto _request;

        public PreSyncEvent(BaseSynchronizationRequestDto request) {
            _request = request;
        }

        public BaseSynchronizationRequestDto Request {
            get { return _request; }
        }

        public bool AllowMultiThreading {
            get { return false; }
        }
    }
}
