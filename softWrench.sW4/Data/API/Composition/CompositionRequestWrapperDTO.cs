using Newtonsoft.Json.Linq;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionRequestWrapperDTO {

        public string Application { get; set; }

        public CompositionFetchRequest Request { get; set; }

        public JObject Data { get; set; }

    }
}