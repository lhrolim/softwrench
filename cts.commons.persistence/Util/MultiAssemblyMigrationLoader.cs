using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;

//took from http://stackoverflow.com/questions/25239992/using-multiple-fluentmigrator-assemblies-on-same-database
namespace cts.commons.persistence.Util {
    public class MultiAssemblyMigrationLoader : IMigrationInformationLoader {
        public MultiAssemblyMigrationLoader(IMigrationConventions conventions, IEnumerable<Assembly> assemblies, string @namespace, IEnumerable<string> tagsToMatch)
            : this(conventions, assemblies, @namespace, false, tagsToMatch) {
        }

        public MultiAssemblyMigrationLoader(IMigrationConventions conventions, IEnumerable<Assembly> assemblies, string @namespace, bool loadNestedNamespaces, IEnumerable<string> tagsToMatch) {
            this.Conventions = conventions;
            this.Assemblies = assemblies;
            this.Namespace = @namespace;
            this.LoadNestedNamespaces = loadNestedNamespaces;
            this.TagsToMatch = tagsToMatch ?? new string[0];
        }

        public IMigrationConventions Conventions { get; private set; }

        public IEnumerable<Assembly> Assemblies { get; private set; }

        public string Namespace { get; private set; }

        public bool LoadNestedNamespaces { get; private set; }

        public IEnumerable<string> TagsToMatch { get; private set; }

        public SortedList<long, IMigrationInfo> LoadMigrations() {
            var sortedList = new SortedList<long, IMigrationInfo>();

            IEnumerable<Type> migrations = this.FindMigrationTypes();
            if (migrations == null) return sortedList;

            foreach (Type migration in migrations) {
                IMigrationInfo migrationInfo = this.Conventions.GetMigrationInfo(migration);
                if (sortedList.ContainsKey(migrationInfo.Version))
                    throw new DuplicateMigrationException(string.Format("Duplicate migration version {0}.", migrationInfo.Version));
                sortedList.Add(migrationInfo.Version, migrationInfo);
            }
            return sortedList;
        }

        private IEnumerable<Type> FindMigrationTypes() {
            IEnumerable<Type> types = new Type[] { };
            foreach (var assembly in Assemblies) {
                types = types.Concat(assembly.GetExportedTypes());
            }
            IEnumerable<Type> matchedTypes = types.Where(t => Conventions.TypeIsMigration(t)
                                                                 &&
                                                                 (Conventions.TypeHasMatchingTags(t, TagsToMatch) ||
                                                                  !Conventions.TypeHasTags(t)));

            if (!string.IsNullOrEmpty(Namespace)) {
                Func<Type, bool> shouldInclude = t => t.Namespace == Namespace;
                if (LoadNestedNamespaces) {
                    string matchNested = Namespace + ".";
                    shouldInclude = t => t.Namespace == Namespace || t.Namespace.StartsWith(matchNested);
                }

                matchedTypes = matchedTypes.Where(shouldInclude);
            }

            return matchedTypes;
        }
    }
}