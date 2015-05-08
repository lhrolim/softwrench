using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata {
    public class OffLineMetadataProvider {


        //key = ordered list of comma separated list of roles
        public static IDictionary<string, CompleteApplicationMetadataDefinition> AssociationsCache = new Dictionary<string, CompleteApplicationMetadataDefinition>();

        //key = ordered list of comma separated list of roles
        public static IDictionary<string, CompleteApplicationMetadataDefinition> CompositionsCache = new Dictionary<string, CompleteApplicationMetadataDefinition>();


        private static readonly ILog Log = LogManager.GetLogger(typeof(OffLineMetadataProvider));

        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchTopLevelApps() {
            var watch = Stopwatch.StartNew();
            var result = new HashSet<CompleteApplicationMetadataDefinition>();
            var menu = MetadataProvider.Menu(ClientPlatform.Mobile);
            var leafs = menu.ExplodedLeafs;
            foreach (var menuBaseDefinition in leafs) {
                if (menuBaseDefinition is ApplicationMenuItemDefinition) {
                    result.Add(MetadataProvider.Application((menuBaseDefinition as ApplicationMenuItemDefinition).Application));
                }
            }

            Log.DebugFormat("fetching top level apps took: {0} ", LoggingUtil.MsDelta(watch));
            //TODO: add hidden menu items
            return result;
        }


        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchCompositionApps(InMemoryUser user) {
            //TODO: cache
            var watch = Stopwatch.StartNew();
            var names = new List<string>();
            foreach (var app in FetchTopLevelApps()) {
                var mobileSchemas = app.Schemas().Where(a => a.Value.IsMobilePlatform());
                foreach (var schema in mobileSchemas) {
                    if (schema.Value.IsMobilePlatform()) {
                        names.AddRange(schema.Value.Compositions.Select(association => association.Relationship));
                    }
                }
            }

            var result = new HashSet<CompleteApplicationMetadataDefinition>();
            foreach (var name in names) {
                var app = MetadataProvider.Application(EntityUtil.GetApplicationName(name));
                result.Add(app.CloneSecuring(user));
            }

            Log.DebugFormat("fetching available compositions took: {0} ", LoggingUtil.MsDelta(watch));
            return result;
        }

        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchAssociationApps(InMemoryUser user) {
            //TODO: cache
            var watch = Stopwatch.StartNew();
            var names = new List<string>();
            foreach (var app in FetchTopLevelApps()) {
                var mobileSchemas = app.Schemas().Where(a => a.Value.IsMobilePlatform());
                foreach (var schema in mobileSchemas) {
                    if (schema.Value.IsMobilePlatform()) {
                        names.AddRange(schema.Value.Associations.Select(association => association.ApplicationTo));
                    }
                }
            }

            var result = new HashSet<CompleteApplicationMetadataDefinition>();
            foreach (var name in names) {
                //TODO: online mode doesn´t require applications for the associations, but currently offline do --> make some sort of inmemory automation
                var app = MetadataProvider.Application(EntityUtil.GetApplicationName(name));
                result.Add(app.CloneSecuring(user));
            }

            Log.DebugFormat("fetching available associations took: {0} ", LoggingUtil.MsDelta(watch));

            return result;
        }

    }
}
