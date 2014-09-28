using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Data;

namespace softWrench.Mobile.Persistence
{
    internal class SynchronizationRepository
    {
        /// <summary>
        ///     Returns the data maps that are dirty, i.e. with pending
        ///     synchronization to the server. Only composition roots
        ///     are returned (components are ignored).
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task<List<DataMap>> LoadPendingCompositeDataMapsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Database
                .GetConnection(cancellationToken)
                .Table<PersistableDataMap>()
                .Where(map => map.IsDirty && map.ParentId == null)
                .ToListAsync()
                .ContinueWith(t => (from map in t.Result
                                    select map.ToDataMap()).ToList(), cancellationToken);

        }

        /// <summary>
        ///     Returns the data maps that are dirty, i.e. with pending
        ///     synchronization to the server and are component (children)
        ///     of the specified composition root data map.
        /// </summary>
        /// <param name="composite">The composition root data map.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<DataMap>> LoadPendingComponentDataMapsAsync(DataMap composite, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parentId = composite.LocalState.LocalId;

            // Note how maps that already exist on the upstream
            // server are discarded. This is a known limitation:
            // for the time being we're allowing only "inserts".
            var result = await Database
                .GetConnection(cancellationToken)
                .Table<PersistableDataMap>()
                .Where(m => m.ParentId == parentId && m.IsLocal && m.IsDirty)
                .ToListAsync();

            return result
                .Select(m => m.ToDataMap())
                .ToList();
        }

        public Task<List<DataOperation>> LoadPendingDataOperationsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Database
                .GetConnection(cancellationToken)
                .Table<PersistableDataOperation>()
                .OrderBy(o => o.LocalRowStamp)
                .ToListAsync()
                .ContinueWith(t => (from operation in t.Result
                                    select operation.ToDataOperation()).ToList(), cancellationToken);

        }

        /// <summary>
        ///     Marks the specified composite root ("parent")
        ///     data map, and all of its component data maps
        ///     ("children") as not dirty.
        /// </summary>
        /// <param name="dataMap">The composite root data map.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task<int> MarkCompositeDataMapAsSynchronizedAsync(DataMap dataMap, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = dataMap.LocalState.LocalId;

            return Database
                .GetConnection(cancellationToken)
                .ExecuteAsync("update DataMap set IsDirty = 0 where LocalId = ? or ParentId = ?", id, id);
        }

        /// <summary>
        ///     Marks the specified composite root ("parent") data map, and all
        ///     of its component data maps ("children") as "bouncing" with the 
        ///     given reason.
        /// </summary>
        /// <param name="dataMap">The composite root data map.</param>
        /// <param name="bounceReason">The bounce reason.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task<int> MarkCompositeDataMapAsBouncingAsync(DataMap dataMap, string bounceReason, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = dataMap.LocalState.LocalId;

            return Database
                .GetConnection(cancellationToken)
                .ExecuteAsync("update DataMap set IsBouncing = 1, BounceReason = ? where LocalId = ? or ParentId = ?", bounceReason,  id, id);
        }

        public Task<int> MarkDataOperationAsSynchronizedAsync(DataOperation dataOperation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Database
                .GetConnection(cancellationToken)
                .ExecuteAsync("delete from DataOperation where LocalId = ?", dataOperation.LocalId);
        }

        /// <summary>
        ///     Stashes data that need to be held
        ///     after the synchronization process
        ///     finishes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StashAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = Database.GetConnection(cancellationToken);

            // Let's clear the previous stashed data first.
            await connection.ExecuteAsync("delete from StashedDataMap");

            // Let's stash everything which 
            //  - is not synchronized by now with the server
            //    (because this means it was rejected) OR
            //  - have custom fields stored locally (as we don't
            //    want to lose them).
            // This is done directly through SQL as we
            // really don't need to process this info by code
            // right now.
            await connection.ExecuteAsync("insert into StashedDataMap select * from DataMap where IsDirty = 1 or CustomFields is not null");

            // If data is dirty it means it was bounced from
            // the server (i.e., could not be synchronized
            // successfully), so let's ack this.
            await connection.ExecuteAsync("update StashedDataMap set IsBouncing = 1 where IsDirty = 1");
        }

        /// <summary>
        ///     Restores previously stashed data back
        ///     to its original place.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task UnstashAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = Database.GetConnection(cancellationToken);

            // For stashed items that also exist on the
            // server and bounced, we will effectively
            // discard the copy fetched from the server
            // and replace it with our stashed version.
            await connection.ExecuteAsync("delete from DataMap where exists (select 1 from StashedDataMap where StashedDataMap.Application = DataMap.Application and StashedDataMap.Id = DataMap.Id and StashedDataMap.IsBouncing = 1 and StashedDataMap.IsLocal = 0)");

            // Move back all bounced data.
            await connection.ExecuteAsync("insert into DataMap select * from StashedDataMap where IsBouncing = 1");

            // Merges all stashed custom fields back to
            // the main table. Notice how we can safely
            // ignore bounced data as they were already
            // restored by the previous operation.
            await connection.ExecuteAsync("update DataMap set CustomFields = (select CustomFields from StashedDataMap where StashedDataMap.Id = DataMap.Id and StashedDataMap.IsBouncing = 0 and StashedDataMap.CustomFields is not null)");

            // Empties the stash table.
            await connection.ExecuteAsync("delete from StashedDataMap");
        }
    }
}