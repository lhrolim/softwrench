using softWrench.Mobile.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Data
{
    public static class DataMapExtensions
    {
        public static string Id(this DataMap dataMap, IApplicationIdentifier application)
        {
            return dataMap.Value(application.IdFieldName);
        }

        public static T Id<T>(this DataMap dataMap, IApplicationIdentifier application)
        {
            return dataMap.Value<T>(application.IdFieldName);
        }
    }
}
