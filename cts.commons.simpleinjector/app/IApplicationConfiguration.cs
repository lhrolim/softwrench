using cts.commons.persistence;
using JetBrains.Annotations;

//using softWrench.sW4.Util;

namespace cts.commons.simpleinjector.app
{
    public interface IApplicationConfiguration :ISingletonComponent
    {
        bool IsDB2(DBType maximo);
        DBMS? LookupDBMS(DBType dbtype);
        bool IsOracle(DBType maximo);
        string GetClientKey();

        bool IsLocal();

        bool IsUnitTest { get; }
    }
}