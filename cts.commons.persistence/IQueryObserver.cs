using System;

namespace cts.commons.persistence {
    public interface IQueryObserver {
        void OnQueryExecution(string query, string queryAlias, int? ellapsedTimeMillis = null);

        void MarkQueryResolution(string queryAlias, long ellapsedTimeMillis, int? countResult =0);

        bool IsTurnedOn();
    }
}