using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API.Composition;

namespace softWrench.sW4.Data.API {
    public class OperationDataRequest {


        //For some reason MVC api is not working unless it´s a pure getter/setter
        public RouterParametersDTO RouteParametersDTOHandled {
            get {
                return RouteParametersDTO ?? new RouterParametersDTO();
            }
        }

        //this comes from client side, not supposed to be used on the server side
        public RouterParametersDTO RouteParametersDTO {
            private get; set;
        }


        public string ApplicationName {
            get; set;
        }
        public string Id {
            get; set;
        }

        public string UserId {
            get; set;
        }

        public string SiteId {
            get; set;
        }

        public ClientPlatform Platform {
            get; set;
        }

        public string CurrentSchemaKey {
            get; set;
        }

        public bool MockMaximo {
            get; set;
        }

        public string Operation {
            get; set;
        }

        public string Username {
            get; set;
        }

        public string Comment {
            get; set;
        }

        public CompositionOperationDTO CompositionData { get; set; }

        public bool Batch {
            get; set;
        }
    }
}
