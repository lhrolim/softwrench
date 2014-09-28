using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SW4_HistoricalImport.Data
{
    public class MysqlDataConnector : IDataConnector
    {
        private string connString = ConfigurationManager.ConnectionStrings["Database"].ToString();
        private MySqlConnection conn = null;
        private int batchInsertSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchInsertSize"].ToString());

        public bool SubmitQueriesToDatabase(List<string> queries)
        {
            bool result = true;
            try
            {
                conn = new MySqlConnection(connString);
                conn.Open();

                for (int i = 0; i < Math.Ceiling((double)(queries.Count / batchInsertSize)); i++)
                {
                    var dbTransaction = conn.BeginTransaction();

                    for (int j = 0; j < batchInsertSize; j++)
                    {
                        var recordNum = (i * batchInsertSize) + j;
                        MySqlCommand command = conn.CreateCommand();
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = queries[recordNum];
                        command.Connection = conn;
                        command.Transaction = dbTransaction;
                        command.ExecuteNonQuery();
                    }

                    dbTransaction.Commit();
                }

                var rTransaction = conn.BeginTransaction();

                for (int i = 0; i < (queries.Count % batchInsertSize); i++)
                {
                    var recordNum = ((queries.Count/batchInsertSize)*batchInsertSize) + i;
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = queries[recordNum];
                    command.Connection = conn;
                    command.Transaction = rTransaction;
                    command.ExecuteNonQuery();
                }
                rTransaction.Commit();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
    }
}
