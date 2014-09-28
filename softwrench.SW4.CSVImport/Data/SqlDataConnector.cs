using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace SW4_HistoricalImport.Data
{
    public class SqlDataConnector : IDataConnector
    {
        private string connString = ConfigurationManager.ConnectionStrings["Database"].ToString();
        private SqlConnection conn = null;
        private int batchInsertSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchInsertSize"].ToString());

        public bool SubmitQueriesToDatabase(List<string> queries)
        {
            bool result = true;
            int rowcount = 0;
            string queryTest = "";
            try
            {
                conn = new SqlConnection(connString);
                conn.Open();


                for (int i = 0; i < Math.Ceiling((double)(queries.Count / batchInsertSize)); i++)
                {
                    var dbTransaction = conn.BeginTransaction();

                    for (int j = 0; j < batchInsertSize; j++)
                    {
                        rowcount++;
                        var recordNum = (i * batchInsertSize) + j;
                        SqlCommand command = conn.CreateCommand();
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = queries[recordNum];
                        command.Connection = conn;
                        command.Transaction = dbTransaction;
                        command.ExecuteNonQuery();
                        queryTest = queries[recordNum];
                    }

                    dbTransaction.Commit();
                }

                var rTransaction = conn.BeginTransaction();

                for (int i = 0; i < (queries.Count % batchInsertSize); i++)
                {
                    rowcount++;
                    var recordNum = ((queries.Count/batchInsertSize)*batchInsertSize) + i;
                    SqlCommand command = conn.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = queries[recordNum];
                    command.Connection = conn;
                    command.Transaction = rTransaction;
                    command.ExecuteNonQuery();
                    queryTest = queries[recordNum];
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
