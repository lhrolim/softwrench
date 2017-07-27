using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API
{
    public interface IDataRequest
    {
        ApplicationMetadataSchemaKey Key { get; set; }
        String Title { get; set; }
        IDictionary<string,object> CustomParameters { get; set; }
        String CommandId { get; set; }
    }
}