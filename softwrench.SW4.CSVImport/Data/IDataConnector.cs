using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SW4_HistoricalImport.Data
{
    internal interface IDataConnector
    {
        bool SubmitQueriesToDatabase(List<string> queries);
    }
}
