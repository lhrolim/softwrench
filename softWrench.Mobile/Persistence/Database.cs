using System;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Metadata;
using SQLite;

namespace softWrench.Mobile.Persistence
{
    /// <summary>
    /// A helper class for working with SQLite
    /// </summary>
    public static class Database
    {
        private static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Database.db");

        private static bool _initialized;
        //private static bool _hasNeverDownloadedData = true;

        private static readonly Type[] TableTypes =
        {
            typeof (Settings),
            typeof (PersistableCookieCollection),
            typeof (User),
            typeof (PersistableApplicationMetadata),
            typeof (Sequence),
            typeof (PersistableDataMap),
            typeof (StashedDataMap),
            typeof (PersistableDataOperation),
            typeof (PersistableMenu)
        };

        /// <summary>
        /// For use within the app on startup, this will create the database
        /// </summary>
        /// <returns></returns>
        public static Task Initialize (CancellationToken cancellationToken = default(CancellationToken))
        {
            return CreateDatabase(new SQLiteAsyncConnection(Path, true), cancellationToken);
        }

        /// <summary>
        /// Global way to grab a connection to the database, make sure to wrap in a using
        /// </summary>
        public static SQLiteAsyncConnection GetConnection (CancellationToken cancellationToken)
        {
            var connection = new SQLiteAsyncConnection(Path, true);
            if (!_initialized)
            {
                CreateDatabase(connection, cancellationToken).Wait(cancellationToken);
            }
            return connection;
        }

        public static SQLiteConnection GetConnection ()
        {
            return new SQLiteConnection(Path, true);
        }

        private static Task CreateDatabase (SQLiteAsyncConnection connection, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                //Create the tables
                var createTask = connection.CreateTablesAsync (TableTypes);
                createTask.Wait(cancellationToken);

                //var preLoadedDataTask = connection.Table<Country>().CountAsync();
                //preLoadedDataTask.Wait(cancellationToken);

                //if (preLoadedDataTask.Result == 0)
                //{
                //    var insertTask = connection.InsertAllAsync(TestData.All);

                //    //Wait for inserts
                //    insertTask.Wait(cancellationToken);
                //}

                //var downloadedDataTask = connection.Table<Article>().CountAsync();
                //downloadedDataTask.Wait(cancellationToken);

                //_hasNeverDownloadedData = (downloadedDataTask.Result == 0);
                
                //Mark database created
                _initialized = true;

            }, cancellationToken);
        }
    }
}