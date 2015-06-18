using System;
using softwrench.sw4.api.classes.application;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;

namespace softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services {

    public interface IBatchOptionProvider : IApplicationFiltereable {

        BatchOptions GenerateOptions(IBatch batch);


    }
}
