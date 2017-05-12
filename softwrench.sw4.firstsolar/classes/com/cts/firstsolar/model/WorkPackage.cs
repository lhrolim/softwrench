using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Data.Persistence.SWDB.Entities;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_WORKPACKAGE", Lazy = false)]
    public class WorkPackage : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public long WorkorderId { get; set; }

        [Property]
        public long Wonum { get; set; }

        [Property]
        public DateTime? CreatedDate { get; set; }

        //        [Property]
        //        public string MaitenanceProcedure { get; set; }


        //        [ComponentProperty]
        //        public WorkOrderDetailComponent WorkorderComponent { get; set; } = new WorkOrderDetailComponent();


        [Property]
        public string Tier { get; set; }

        [Property]
        public bool? TestResultReviewEnabled { get; set; }
        [Property]
        public bool? SubContractorEnabled { get; set; }
        [Property]
        public bool? MaintenanceEnabled { get; set; }

        


        #region resultsForReview

        [Property]
        public string ResultsForReview { get; set; }

        [Property]
        public int DaysUponClosure { get; set; }

        [Property]
        public string RequestExplanation { get; set; }

        #endregion


        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False,
            Where = "ParentColumn = 'outages' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> OutagesList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False,
            Where = "ParentColumn = 'outagetypes' and ParentEntity = 'WorkPackage'")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> OutagesTypeList { get; set; } = new List<GenericListRelationship>();

        [Property]
        public string InterConnectDocs { get; set; }

        [Bag(0, Table = "OPT_CALLOUT", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False)]
        [Key(1, Column = "workpackageid")]
        [OneToMany(2, ClassType = typeof(CallOut))]
        public virtual IList<CallOut> CallOuts { get; set; }

        [Bag(0, Table = "OPT_MAINTENANCE_ENG", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False)]
        [Key(1, Column = "workpackageid")]
        [OneToMany(2, ClassType = typeof(MaintenanceEngineering))]
        public virtual IList<MaintenanceEngineering> MaintenanceEngineerings { get; set; }



        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'engcomponents' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> EngComponentsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'gsuimmediatetests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> GsuImmediateTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'gsutests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> GsuTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'sf6tests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> Sf6TestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'vacuumtests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> VacuumTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'airswitchertests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> AirSwitcherTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'capbanktests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> CapBankTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'batterytests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> BatteryTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'relaytests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> RelayTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all-delete-orphan", Lazy = CollectionLazy.False, Where = "ParentColumn = 'feedertests' and ParentEntity = 'WorkPackage' ")]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> FeederTestsList { get; set; } = new List<GenericListRelationship>();
    }
}
