using System.Collections.Generic;

namespace softWrench.sW4.Data.Persistence.WS.Rest {
    public interface IRestObjectWrapper {
        void AddEntry(string key, object value);

        IDictionary<string, object> Entries { get; }

    }
}