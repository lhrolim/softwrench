using softwrench.sw4.api.classes.application;
using softwrench.sw4.batch.api.entities;

namespace softwrench.sw4.batch.api.services {

    public interface IBatchOptionProvider : IApplicationFiltereable {

        BatchOptions GenerateOptions(IBatch batch);


    }
}
