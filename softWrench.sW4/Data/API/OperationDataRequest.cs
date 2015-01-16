using softwrench.sW4.Shared2.Metadata.Applications;

namespace softWrench.sW4.Data.API {
    public class OperationDataRequest {

        private RouterParametersDTO _routerParametersDTO;

        public string Application { get; set; }
        public string Id { get; set; }

        public ClientPlatform Platform { get; set; }

        public string CurrentSchemaKey { get; set; }

        public bool MockMaximo { get; set; }

        public RouterParametersDTO RouteParametersDTO {
            get {
                return _routerParametersDTO ?? new RouterParametersDTO();
            }
            set {
                _routerParametersDTO = value;
            }
        }
        public string Operation { get; set; }
    }
}
