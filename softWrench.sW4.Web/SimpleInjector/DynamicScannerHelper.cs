using System;
using System.Collections.Generic;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using log4net;
using NHibernate.Linq;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.SimpleInjector {
    public class DynamicScannerHelper {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DynamicScannerHelper));
        public static readonly IDictionary<string, DynamicComponentRecord> DynTypes = new Dictionary<string, DynamicComponentRecord>();
        private const string DynFoundMsg = "Dynamic component found to deploy: {0} to replace {1}";

        public static void LoadDynamicTypes(ScriptEntry singleDynComponent) {
            if (singleDynComponent != null) {
                LoadDynamicType(singleDynComponent);
                return;
            }

            DynTypes.Clear();

            // manually initialized because can be called before the container creation
            var dao = CreateDao();
            var scritService = new ScriptsService(dao, null, null);

            var version = scritService.GetSystemVersion();
            var entries = dao.FindByQuery<ScriptEntry>(ScriptEntry.ScriptByDeployVersion, true, version);
            if (entries == null || entries.Count == 0) {
                return;
            }

            entries.ForEach(entry => {
                MakeDynRecord(entry, scritService);
            });
        }

        public static IDictionary<string, DynamicComponentRecord> CloneDynTypes() {
            var dynTypes = new Dictionary<string, DynamicComponentRecord>();
            DynTypes.ForEach(pair => dynTypes.Add(pair.Key, pair.Value));
            return dynTypes;
        }

        private static void LoadDynamicType(ScriptEntry singleDynComponent) {
            // manually initialized because can be called before the container creation
            var scritService = new ScriptsService(null, null, null);
            var shouldBeOnContaier = scritService.ShouldBeOnContainer(singleDynComponent);

            if (!shouldBeOnContaier) {
                DynTypes.Remove(singleDynComponent.Target);
                return;
            }

            MakeDynRecord(singleDynComponent, scritService);
        }

        private static void MakeDynRecord(ScriptEntry entry, ScriptsService scritService) {
            Log.Debug(string.Format(DynFoundMsg, entry.Name, entry.Target));
            var record = new DynamicComponentRecord {
                Name = entry.Name,
                Type = (Type)scritService.EvaluateScript(entry.Script)
            };
            DynTypes[entry.Target] = record;
        }

        private static ISWDBHibernateDAO CreateDao() {
            var appConfig = new ApplicationConfigurationAdapter();
            return new SWDBHibernateDAO(appConfig, new HibernateUtil(appConfig));
        }

        public class DynamicComponentRecord {
            public string Name { get; set; }
            public Type Type { get; set; }
        }
    }
}