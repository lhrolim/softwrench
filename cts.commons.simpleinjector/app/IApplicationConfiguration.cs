using cts.commons.persistence;

namespace cts.commons.simpleinjector.app
{
    public interface IApplicationConfiguration :ISingletonComponent
    {
        bool IsDB2(DBType maximo);
        DBMS? LookupDBMS(DBType dbtype);
    }
}