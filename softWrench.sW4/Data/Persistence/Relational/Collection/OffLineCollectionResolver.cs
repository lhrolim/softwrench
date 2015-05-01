using cts.commons.portable.Util;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Persistence.Relational.Collection {
    public class OffLineCollectionResolver : CollectionResolver {

        protected override SearchRequestDto BuildSearchRequestDto(InternalCollectionResolverParameter parameter,
            CollectionMatchingResultWrapper matchingResultWrapper) {
            var dto = base.BuildSearchRequestDto(parameter, matchingResultWrapper);
            if (parameter.Rowstamp != null) {
                dto.AppendSearchEntry(RowStampUtil.RowstampColumnName, ">{0}".Fmt(parameter.Rowstamp));
            }
            return dto;
        }

        protected override CollectionMatchingResultWrapper GetResultWrapper() {
            return new OffLineMatchResultWrapper();
        }


        protected class OffLineMatchResultWrapper : CollectionMatchingResultWrapper {
            internal override CollectionMatchingResultKey FetchKey(AttributeHolder entity) {
                //doesnt matter for offline
                return new CollectionMatchingResultKey();
            }
        }
    }
}
