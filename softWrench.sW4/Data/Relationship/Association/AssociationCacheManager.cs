using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using JetBrains.Annotations;
using Quartz;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Scheduler.Interfaces;

namespace softWrench.sW4.Data.Relationship.Association {
    public class AssociationCacheManager : ASwJob {

        private readonly ConcurrentDictionary<AssociationCacheKey, AssociationCacheValue> cache = new ConcurrentDictionary<AssociationCacheKey, AssociationCacheValue>();

        private EntityRepository _entityRepository;

        public AssociationCacheManager(EntityRepository entityRepository) {
            _entityRepository = entityRepository;
        }



        public override void ExecuteJob() {
            var entities = MetadataProvider.Entities();
            foreach (var entity in entities) {

            }

        }



        class AssociationCacheValue {
            [NotNull]
            private long maxRowstamp;

            [NotNull]
            private IEnumerable<IAssociationOption> cachedOptions;

        }

        public override string Name() {
            return "AssociationCacheJOB";
        }

        public override string Description() {
            return "JOB for storing the cacheable associations";
        }

        public override string Cron() {
            return "0 0/15/30/45 * * * ?";
        }

        public override bool IsEnabled {
            get {
                return false;
            }
        }


        public override bool RunAtStartup() {
            return true;
        }
    }
}
