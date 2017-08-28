using Newtonsoft.Json.Linq;
using softwrench.sw4.offlineserver.model;

namespace softwrench.sw4.offlineserver.dto {
    public abstract class BaseSynchronizationRequestDto {

        public DeviceData DeviceData { get; set; }

        public UserSyncData UserData { get; set; }
        public JObject RowstampMap { get; set; }


        /// <summary>
        /// Id of the operation coming from the client side, to allow grouping all the threads under a single entity
        /// </summary>
        public string ClientOperationId { get; set; }
    }
}