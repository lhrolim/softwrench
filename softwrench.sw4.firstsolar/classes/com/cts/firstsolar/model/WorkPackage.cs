using System;
using System.Collections.Generic;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Data.Persistence.SWDB.Entities;
using softWrench.sW4.Metadata.Validator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_WORKPACKAGE", Lazy = false)]
    public class WorkPackage : IBaseEntity {

        public static string ByToken = "from WorkPackage where AccessToken = ?";


        private string _wonum;

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public long WorkorderId { get; set; }

        public string Wonum {
            get {
                if (_wonum != null) {
                    return _wonum;
                }
                _wonum = Wpnum != null && Wpnum.StartsWith("WP") ? "NA" + Wpnum.Substring(2) : Wpnum;
                return _wonum;
            }
            set { _wonum = value; }
        }

        [Property]
        [UserIdProperty]
        public string Wpnum { get; set; }

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

        [Property]
        public DateTime? EstimatedCompDate { get; set; }

        [Property]
        public DateTime? ActualCompDate { get; set; }

        public string MwhLostTotal { get; set; }

        [Property]
        public string ExpectedMwhLost { get; set; }

        [Property]
        public string MwhLostPerDay { get; set; }

        [Property]
        public string ProblemStatement { get; set; }

        [Property]
        public bool? BuildComplete { get; set; }


        #region resultsForReview

        [Property]
        public string ResultsForReview { get; set; }

        [Property]
        public int DaysUponClosure { get; set; }

        [Property]
        public string RequestExplanation { get; set; }

        #endregion


        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False,
            Where = "ParentColumn = 'outages' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> OutagesList { get; set; } = new List<GenericListRelationship>();

        [Property]
        public string OutageType { get; set; }

        [Property]
        public string InterConnectDocs { get; set; }

        [Bag(0, Table = "OPT_CALLOUT", Cascade = "all", Lazy = CollectionLazy.False, Inverse = true)]
        [Key(1, Column = "workpackageid", NotNull = true)]
        [OneToMany(2, ClassType = typeof(CallOut))]
        public virtual IList<CallOut> CallOuts { get; set; }

        [Bag(0, Table = "OPT_MAINTENANCE_ENG", Cascade = "all", Lazy = CollectionLazy.False, Inverse = true)]
        [Key(1, Column = "workpackageid", NotNull = true)]
        [OneToMany(2, ClassType = typeof(MaintenanceEngineering))]
        public virtual IList<MaintenanceEngineering> MaintenanceEngineerings { get; set; }

        [Bag(0, Table = "OPT_DAILY_OUTAGE_MEETING", Cascade = "all", Lazy = CollectionLazy.False, OrderBy = "MeetingTime asc")]
        [Key(1, Column = "workpackageid", NotNull = true)]
        [OneToMany(2, ClassType = typeof(DailyOutageMeeting))]
        public virtual IList<DailyOutageMeeting> DailyOutageMeetings { get; set; }



        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'engcomponents' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> EngComponentsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'gsuimmediatetests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> GsuImmediateTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'gsutests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> GsuTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'sf6tests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> Sf6TestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'vacuumtests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> VacuumTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'airswitchertests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> AirSwitcherTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'capbanktests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> CapBankTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'batterytests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> BatteryTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'relaytests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> RelayTestsList { get; set; } = new List<GenericListRelationship>();

        [Bag(0, Table = "GEN_LISTRELATIONSHIP", Cascade = "all", Lazy = CollectionLazy.False, Where = "ParentColumn = 'feedertests' and ParentEntity = 'WorkPackage' ", Inverse = true)]
        [Key(1, Column = "parentid")]
        [OneToMany(2, ClassType = typeof(GenericListRelationship))]
        public IList<GenericListRelationship> FeederTestsList { get; set; } = new List<GenericListRelationship>();

        /// <summary>
        /// Token used to access this workpackage without requiring the user to be authenticated
        /// </summary>
        [Property]
        public string AccessToken { get; set; }

        [Bag(0, Table = "OPT_WPEMAILSTATUS", Cascade = "all", Lazy = CollectionLazy.False, Inverse = true)]
        [Key(1, Column = "workpackageid")]
        [OneToMany(2, ClassType = typeof(WorkPackageEmailStatus))]
        public virtual IList<WorkPackageEmailStatus> EmailStatuses { get; set; } = new List<WorkPackageEmailStatus>();



        public override string ToString() {
            return $"{nameof(Id)}: {Id}, {nameof(Wonum)}: {Wonum}";
        }
    }
}
