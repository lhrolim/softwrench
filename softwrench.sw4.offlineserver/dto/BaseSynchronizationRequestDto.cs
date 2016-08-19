using Newtonsoft.Json.Linq;

namespace softwrench.sw4.offlineserver.dto {
    public abstract class BaseSynchronizationRequestDto {
        public UserSyncData UserData { get; set; }
        public JObject RowstampMap { get; set; }
    }
}