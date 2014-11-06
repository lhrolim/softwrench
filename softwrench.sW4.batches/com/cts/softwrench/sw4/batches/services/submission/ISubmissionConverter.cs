using System;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.SimpleInjector;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {
    public interface ISubmissionConverter :IComponent{
        Boolean ShouldSubmit(JObject row);

        CrudOperationData Convert(JObject row);


        String ApplicationName();
    }
}