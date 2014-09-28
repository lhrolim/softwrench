using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW4_Import
{
    public static class QueryBuilder
    {
        public static List<string> BuildQueryForImport(Import import)
        {
            List<string> resultingQueries = new List<string>();

            foreach (string rowKey in import.Data.Keys)
            {
                StringBuilder query = new StringBuilder();
                query.Append("insert into ");
                query.Append(ConfigurationManager.AppSettings["DatabaseSchema"]);
                query.Append(".");
                query.Append(ConfigReader.ImportTable);

                query.Append("(");

                List<string> fieldValues = new List<string>();
                foreach (string fieldKey in import.Data[rowKey].Keys)
                {
                    query.Append(fieldKey);
                    query.Append(",");
                    fieldValues.Add(import.Data[rowKey][fieldKey]);
                }
                query.Remove(query.Length - 1, 1);
                query.Append(") values (");

                foreach (string value in fieldValues)
                {
                    query.Append("'");
                    query.Append(value.Replace("'", "''"));
                    query.Append("',");
                } 
                query.Remove(query.Length - 1, 1);
                query.Append(");");

                resultingQueries.Add(query.ToString());
            }

            return resultingQueries;
        }
    }
}
