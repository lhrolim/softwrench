using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata {
    public class OffLineMetadataProvider {


        //key = ordered list of comma separated list of roles
        public static IDictionary<string, CompleteApplicationMetadataDefinition> AssociationsCache = new Dictionary<string, CompleteApplicationMetadataDefinition>();

        //key = ordered list of comma separated list of roles
        public static IDictionary<string, CompleteApplicationMetadataDefinition> CompositionsCache = new Dictionary<string, CompleteApplicationMetadataDefinition>();


        private static readonly ILog Log = LogManager.GetLogger(typeof(OffLineMetadataProvider));




        public static IEnumerable<CompleteApplicationMetadataDefinition> FetchCompositionApps(InMemoryUser user) {
            //TODO: cache
            var watch = Stopwatch.StartNew();
            var names = new List<string>();
            foreach (var app in MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user)) {
                var mobileSchemas = app.Schemas().Where(a => a.Value.IsMobilePlatform());
                foreach (var schema in mobileSchemas) {
                    if (schema.Value.IsMobilePlatform()) {
                        names.AddRange(schema.Value.Compositions().Select(association => association.Relationship));
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
            var names = new List<Tuple<string, string>>();
            foreach (var app in MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user)) {
                var mobileSchemas = app.Schemas().Where(a => a.Value.IsMobilePlatform());
                foreach (var schema in mobileSchemas) {
                    if (schema.Value.IsMobilePlatform()) {
                        names.AddRange(schema.Value.Associations().Select(association => new Tuple<string, string>(association.EntityAssociation.To, association.ApplicationTo)));
                    }
                }
            }

            var result = new HashSet<CompleteApplicationMetadataDefinition>();
            foreach (var name in names) {
                var appNamedAsEntity = name.Item1;
                var appNamedAsQualifier = name.Item2;

                //TODO: online mode doesn´t require applications for the associations, but currently offline do --> make some sort of inmemory automation
                var app = MetadataProvider.Application(EntityUtil.GetApplicationName(appNamedAsQualifier), false);
                if (app == null) {
                    app = MetadataProvider.Application(EntityUtil.GetApplicationName(appNamedAsEntity), false);
                    if (app == null) {
                        Log.WarnFormat(
                            "Application {0} | {1} is not available, things might go wrong on the offline application",
                            appNamedAsQualifier, appNamedAsEntity);
                    } else {
                        result.Add(app.CloneSecuring(user));
                    }

                } else {
                    result.Add(app.CloneSecuring(user));
                }
            }

            Log.DebugFormat("fetching available associations took: {0} ", LoggingUtil.MsDelta(watch));

            return result;
        }

    }
}
