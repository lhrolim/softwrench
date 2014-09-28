using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Persistence.Expressions;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using SQLite;

namespace softWrench.Mobile.Persistence {
    internal class DataRepository {
        private static void SaveAsyncImpl(CompositeDataMap dataMap, SQLiteConnection connection) {
            connection.Execute("delete from DataMap where LocalId = ?", dataMap.Composite.LocalState.LocalId);
            connection.Insert(new PersistableDataMap(dataMap.Application, dataMap.Composite, true, DateTime.Now));

            foreach (var component in dataMap.Components()) {
                foreach (var componentDataMap in component.DataMaps) {
                    // Links the component to its composite.
                    componentDataMap.LocalState.ParentId = dataMap.Composite.LocalState.LocalId;

                    connection.Execute("delete from DataMap where LocalId = ?", componentDataMap.LocalState.LocalId);
                    connection.Insert(new PersistableDataMap(component.Application, componentDataMap, true, DateTime.Now));
                }
            }
        }

        private static void SaveAsyncImpl(IApplicationIdentifier application, IEnumerable<DataMap> dataMaps) {
            var connection = Database.GetConnection();

            connection.RunInTransaction(() => {
                // Bounced datamaps are (hopefully) stashed            
                // elsewhere, so we can safely obliterate all.
                connection.Execute("delete from DataMap where application = ?", new object[] { application.ApplicationName });

                foreach (var dataMap in dataMaps) {
                    // This method is intended to be used by data
                    // sync routines, so we set the dirty flag as
                    // false, as they are fresh off the server.
                    var persistableDataMap = new PersistableDataMap(application, dataMap, false, DateTime.Now);
                    connection.Insert(persistableDataMap);
                }
            });

            connection.Close();
        }

        public async Task<DataMap> NewAsync(ApplicationSchemaDefinition application, CompositeDataMap dataMap = null) {
            var newDataMap = new DataMap(application);

            // If we have no composite data map it means
            // a "parent" data map is being created. For
            // the sake of coherence, let's simply wrap
            // it inside a CompositeDataMap structure.
            if (null == dataMap) {
                dataMap = CompositeDataMap.Wrap(application, newDataMap);
            } else {
                // A component data map is being created.
                // Let's link it to the composite root.
                newDataMap.LocalState.ParentId = dataMap.Composite.LocalState.LocalId;
            }

            await Task
                .Factory
                .StartNew(() => Dispatcher.OnNew(newDataMap, application, dataMap));

            return newDataMap;
        }

        public async Task<List<DataMap>> LoadAsync(ApplicationSchemaDefinition application, CancellationToken cancellationToken = default(CancellationToken)) {
            var result = (await Database
                .GetConnection(cancellationToken)
                .Table<PersistableDataMap>()
                .Where(map => map.Application == application.Name)
                .OrderByDescending(map => map.IsBouncing)
                .OrderByDescending(map => map.LocalRowStamp)
                .ToListAsync())
                .Select(map => map.ToDataMap())
                .ToList();

            foreach (var dataMap in result) {
                Dispatcher.OnLoad(dataMap, application);
            }

            return result;
        }

        /// <summary>
        ///     Returns all data maps that satisfy the specified filter.
        /// </summary>
        /// <param name="application">The application metadata.</param>
        /// <param name="filters">The search criteria.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<List<DataMap>> LoadAsync(ApplicationSchemaDefinition application, IEnumerable<FilterExpression> filters, CancellationToken cancellationToken = default(CancellationToken)) {
            var query = QueryBuilder.Build(application, filters);

            var result = (await Database
                .GetConnection(cancellationToken)
                .QueryAsync<PersistableDataMap>(query.Sql, query.Parameters))
                .Select(map => map.ToDataMap())
                .ToList();

            //Next and previous are counter-intuitive because the list is ordered on the inverse way
            for (int i = 0; i < result.Count; i++) {
                var currentMap = result[i];
                if (i != 0) {
                    currentMap.Next = result[i - 1];
                }
                if (i != result.Count - 1) {
                    currentMap.Previous = result[i + 1];
                }
                Dispatcher.OnLoad(currentMap, application);
            }
            return result;
        }

        /// <summary>
        ///     Returns the data maps specified by its local id,
        ///     or <see langword="null" /> if it doesn't exist.
        /// </summary>
        /// <remarks>
        ///     This method is intended for low-level data handling (opposing
        ///     to main tasks like loading data in a detail page) and, as such,
        ///     does not trigger the <see cref="OnLoadContext"/> pipeline.
        /// </remarks>
        /// <param name="localId">The unique identifier of the datamap.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<DataMap> PeekAsync(Guid localId, CancellationToken cancellationToken = default(CancellationToken)) {
            var result = await Database
                .GetConnection(cancellationToken)
                .FindAsync<PersistableDataMap>(localId);

            return null == result
                ? null
                : result.ToDataMap();
        }

        public async Task SaveAsync(CompositeDataMap dataMap, CancellationToken cancellationToken = default(CancellationToken)) {
            Dispatcher.OnBeforeSave(dataMap);

            await Database
                .GetConnection(cancellationToken)
                .RunInTransactionAsync(c => SaveAsyncImpl(dataMap, c));
        }

        public Task SaveAsync(ApplicationSchemaDefinition application, DataMap dataMap, CancellationToken cancellationToken = default(CancellationToken)) {
            return SaveAsync(CompositeDataMap.Wrap(application, dataMap), cancellationToken);
        }

        public Task SaveAsync(IApplicationIdentifier application, IEnumerable<DataMap> dataMaps, CancellationToken cancellationToken = default(CancellationToken)) {
            // This method is intended to insert data maps
            // downloaded during synchronization, so we'll
            // not run the pipeline here.
            return Task
                .Factory
                .StartNew(() => SaveAsyncImpl(application, dataMaps), cancellationToken);
        }

        public async Task<int> SaveAsync(DataOperation dataOperation, CancellationToken cancellationToken = default(CancellationToken)) {
            return await Database
                .GetConnection(cancellationToken)
                .InsertAsync(new PersistableDataOperation(dataOperation, DateTime.Now));
        }

        private static class Dispatcher {
            public static void OnNew(DataMap newDataMap, ApplicationSchemaDefinition application, CompositeDataMap composite) {
                ApplicationBehaviorDispatcher
                    .OnNew(newDataMap, application, composite);
            }

            public static void OnLoad(DataMap dataMap, ApplicationSchemaDefinition application) {
                ApplicationBehaviorDispatcher
                    .OnLoad(dataMap, application);
            }

            public static void OnBeforeSave(CompositeDataMap dataMap) {
                ApplicationBehaviorDispatcher
                    .OnBeforeSave(dataMap.Composite, dataMap.Application);

                foreach (var component in dataMap.Components()) {
                    foreach (var componentDataMap in component.DataMaps) {
                        ApplicationBehaviorDispatcher
                            .OnBeforeSave(componentDataMap, component.Application);
                    }
                }
            }
        }
    }
}