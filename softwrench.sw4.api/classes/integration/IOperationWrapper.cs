using System;

namespace softwrench.sw4.api.classes.integration {
    public interface IOperationWrapper {

        string OperationName { get; set; }

        //ICommonOperationData OperationData(Type type = null);

        string UserId {
            get; set;
        }
        string SiteId {
            get; set;
        }

        ICommonOperationData GetOperationData { get; }

        string GetStringAttribute(string attribute);

    }
}