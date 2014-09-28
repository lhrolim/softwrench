using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;

namespace softWrench.Mobile.Persistence {
    internal class MetadataRepository {

        private static MetadataRepository _instance;

        private MetadataRepository() {

        }

        public static MetadataRepository GetInstance() {
            return _instance ?? (_instance = new MetadataRepository());
        }

        private MenuDefinition _cachedMenu;
        private List<ApplicationSchemaDefinition> _cachedApplications;



        /// <summary>
        /// cleans all cached data. Must be called before syncrhonization with the server begins
        /// </summary>
        public void ResetCaches() {
            _cachedMenu = null;
            _cachedApplications = null;
        }


        public async Task<List<ApplicationSchemaDefinition>> LoadAllApplicationsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cachedApplications != null) {
                return _cachedApplications;
            }
            var applications = await Database
                .GetConnection(cancellationToken)
                .Table<PersistableApplicationMetadata>()
                .ToListAsync()
                .ContinueWith(t => (from a in t.Result
                                    select DoLoadApplicationAsync(a, cancellationToken).Result).ToList(), cancellationToken);
            _cachedApplications = applications;
            return applications;
        }

        public async Task<ApplicationSchemaDefinition> LoadApplicationAsync(string application, CancellationToken cancellationToken = default(CancellationToken)) {
            var result = await Database
                .GetConnection(cancellationToken)
                .Table<PersistableApplicationMetadata>()
                .Where(a => a.Name == application)
                .ToListAsync();

            return result.Any()
                ? await DoLoadApplicationAsync(result.First(), cancellationToken)
                : null;
        }

        private async Task<ApplicationSchemaDefinition> DoLoadApplicationAsync(PersistableApplicationMetadata application, CancellationToken cancellationToken) {
            var parent = JsonParser.ApplicationSchemaDefinition(application.Data);

            foreach (var composition in parent.Compositions) {
                // Retrieves the metadata of the
                // composition's source application.
                var child = await LoadApplicationAsync(composition.Relationship(), cancellationToken);
                composition.To(child);
                child.Title = composition.Label;
            }

            return parent;
        }

        public async Task SaveAsync(ApplicationSchemaDefinition application, CancellationToken cancellationToken = default(CancellationToken)) {
            //TODO: from somewhere, purge revoked/deleted metadata.

            var persistableApplicationMetadata = new PersistableApplicationMetadata(application);

            var connection = Database.GetConnection(cancellationToken);

            // Replaces all application metadata.
            await connection.DeleteAsync(persistableApplicationMetadata);
            await connection.InsertAsync(persistableApplicationMetadata);
        }

        public async Task<MenuDefinition> LoadMenuAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_cachedMenu != null) {
                return _cachedMenu;
            }

            var menuJSON = await Database
                .GetConnection(cancellationToken)
                .Table<PersistableMenu>()
                .FirstOrDefaultAsync();
            if (menuJSON == null) {
                return null;
            }
            var menu = JsonParser.ParseMenu(menuJSON.Data);
            _cachedMenu = menu;
            return menu;
        }

        public async Task SaveMenuAsync(MenuDefinition menu, CancellationToken cancellationToken = default(CancellationToken)) {

            var persistableMenu = new PersistableMenu(menu);

            var connection = Database.GetConnection(cancellationToken);

            // Replaces all application metadata.
            await connection.DeleteAsync(persistableMenu);
            await connection.InsertAsync(persistableMenu);
        }

        public async Task<Sequence> LoadAndIncrementSequenceAsync(ApplicationSchemaDefinition application, string field, CancellationToken cancellationToken = default(CancellationToken)) {
            var connection = Database.GetConnection(cancellationToken);

            var sequence = await connection
                .Table<Sequence>()
                .Where(s => s.Application == application.Name && s.Field == field)
                .FirstOrDefaultAsync();

            if (null != sequence) {
                await connection.ExecuteAsync("update Sequence set next = next + 1 where LocalId = ?", sequence.LocalId);
            }

            return sequence;
        }

        public async Task RecreateSequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var connection = Database.GetConnection(cancellationToken);
            await connection.ExecuteAsync("delete from Sequence");

            //TODO: this decision should come from metadata
            var workOrderApplication = await LoadApplicationAsync("workorder", cancellationToken);

            if (null != workOrderApplication) {
                var sequence = new Sequence {
                    Application = workOrderApplication.ApplicationName,
                    Field = "wonum",
                    Mask = "new {0}",
                    LocalId = Guid.NewGuid(),
                    Next = 0
                };

                await connection.InsertAsync(sequence);
            }
        }

        public List<ApplicationSchemaDefinition> CachedApplications {
            get { return _cachedApplications; }
        }
    }
}