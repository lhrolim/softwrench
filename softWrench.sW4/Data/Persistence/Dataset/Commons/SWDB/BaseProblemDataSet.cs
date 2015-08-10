using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB
{
    class BaseProblemDataSet : SWDBApplicationDataset
    {

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request)
        {
            var result = base.GetApplicationDetail(application, user, request);
            var dataAsString = StringExtensions.GetString(CompressionUtil.Decompress((byte[])result.ResultObject.GetAttribute("data")));
            var deserialized = JsonConvert.DeserializeObject(dataAsString);
            var formatted = JsonConvert.SerializeObject(deserialized, Formatting.Indented);
            result.ResultObject.SetAttribute("#dataasstring", formatted);
            return result;
        }

        public override string ApplicationName()
        {
            return "_SoftwrenchError";
        }

        public override string ClientFilter()
        {
            return null;
        }
    }
}