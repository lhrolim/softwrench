
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchResult : GenericResponseResult<AttributeHolder> {

        public CompositionFetchResult(AttributeHolder originalEntity)
            : base(originalEntity) {

        }

    }
}
