using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;
using System;
using System.Linq;
using dc = softwrench.sw4.Hapag.Data.HapagDashBoardsConstants;
using fr = softwrench.sw4.Hapag.Data.FunctionalRole;
using qc = softwrench.sw4.Hapag.Data.Configuration.HapagQueryConstants;
namespace softwrench.sw4.Hapag.Data.Configuration {

    public class HapagConfigurationRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {
        private readonly IWhereClauseFacade _wcFacade;
        private readonly IConfigurationFacade _facade;
        private readonly SWDBHibernateDAO _dao;


        public HapagConfigurationRegistry(IConfigurationFacade facade, IWhereClauseFacade wcFacade, SWDBHibernateDAO dao) {
            _wcFacade = wcFacade;
            _facade = facade;
            _dao = dao;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            //register as event so it runs after ApplicationWhereClauseRegistry
            if (ApplicationConfiguration.ClientName != "hapag") {
                //TODO: make build modularized
                return;
            }
            _dao.ExecuteSql("delete from conf_propertyvalue where definition_id = '/_whereclauses/imac/whereclause' and module = 'tom,itom'");
            CreateBaseWhereClauses();
            CreateEndUserWhereClause();
            CreateOfferingWhereClause();
            CreateLocalITCWhereClauses();
            CreateXITCWhereClauses();
            CreateTomAndITomWhereClauses();
            CreateADWhereClauses();
            CreateTuiWhereClauses();
            CreateChangeWhereClauses();
            CreateSsoWhereClauses();
            CreatePurchaseWhereClauses();

            CreateAssetCtrlRamWhereClauses();
            CreateAssetCtrlWhereClauses();
            CreateReportWhereClauses();

        }

        

        private void CreateOfferingWhereClause() {
            _wcFacade.Register("offering", qc.OfferingITCDashboard(), MetadataId(dc.ActionRequiredForOpenRequestsOffering));
        }


        private void CreateBaseWhereClauses() {

            //used for imac install template
            _wcFacade.Register("asset", qc.MovedAssets, MetadataId("movedassets"));
            _wcFacade.Register("incident", qc.DefaultIncidentGridQuery);
            // applied for XITC,TOM and ITOM
            _wcFacade.Register("problem", qc.DefaultProblemGridQuery);
        }

        private void CreateEndUserWhereClause() {
            _wcFacade.Register("servicerequest", qc.EndUserSR);

            //dashboards
            _wcFacade.Register("servicerequest", qc.EndUserOpenRequests, MetadataId(dc.EUOpenRequests));
            _wcFacade.Register("servicerequest", qc.EndUserActionRequired, MetadataId(dc.ActionRequiredForOpenRequests));
        }

        private void CreateLocalITCWhereClauses() {

            //R0017 + EU
            _wcFacade.Register("servicerequest", R0017Method("LocalITCSRWhereClause"), ForProfile(ProfileType.Itc));
            //R0017
            _wcFacade.Register("asset", R0017Method("AssetWhereClause"), ForProfile(ProfileType.Itc));
            _wcFacade.Register("imac", R0017Method("ImacWhereClause"), ForProfile(ProfileType.Itc));
            _wcFacade.Register("incident", R0017Method("IncidentWhereClause"), ForProfile(ProfileType.Itc));


            // Dashboard Where Clauses
            _wcFacade.Register("servicerequest", DR0017Method("LocalITCDashboardSRWhereClause"), MetadataIdForITC(dc.ActionRequiredForOpenRequests));
            _wcFacade.Register("imac", DR0017Method("DashboardIMACWhereClause"), MetadataIdForITC(dc.OpenImacs));
            _wcFacade.Register("incident", DR0017Method("ITCDashboardIncidentWhereClause"), MetadataIdForITC(dc.ActionRequiredForOpenIncidents));

        }

        private void CreateXITCWhereClauses() {
            //basically samething as LocalITC, but without appending EU queries for SR, have to register them back here...
            _wcFacade.Register("servicerequest", R0017Method("SRWhereClause"), ForModule(fr.XItc));

            _wcFacade.Register("asset", "@assetControlWhereClauseProvider.AssetWhereClauseIfRegionSelected", ForModule(fr.XItc));
            _wcFacade.Register("imac", R0017Method("ImacWhereClause"), ForModule(fr.XItc));
            _wcFacade.Register("incident", R0017Method("IncidentWhereClause"), ForModule(fr.XItc));
            //problem whereclause registered as basic one

            // Dashboard Where Clauses
            _wcFacade.Register("servicerequest", DR0017Method("XITCDashboardSRWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenRequests, fr.XItc));
            _wcFacade.Register("imac", DR0017Method("DashboardIMACWhereClause"), MetadataIdForModule(dc.OpenImacs, fr.XItc));
            _wcFacade.Register("incident", DR0017Method("ITCDashboardIncidentWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenIncidents, fr.XItc));

        }

        private void CreateChangeWhereClauses() {
            _wcFacade.Register("change", ChangeWhereClause("ChangeGridQuery"), ForAllModules());
            _wcFacade.Register("srforchange", ChangeWhereClause("ChangeSRUnionGridQuery"), ForAllModules().AppendSchema("changeunionschema"));
            _wcFacade.Register("woactivity", ChangeWhereClause("DashboardChangeTasksWhereClause"), MetadataIdForAll(dc.OpenChangeTasks));
            _wcFacade.Register("change", ChangeWhereClause("DashboardChangeTasksWhereClauseViewAll"), MetadataIdForAll(dc.OpenChangeTasks));
            _wcFacade.Register("change", ChangeWhereClause("DashboardChangeApprovalsWhereClause"), MetadataIdForAll(dc.OpenApprovals));
        }



        private void CreateTomAndITomWhereClauses() {
            //chang queries already registered in CreateChangeWhereClauses
            //Business logic: Allowed to see every Asset (except asset list reports) and Ticket assigned to Hapag-Lloyd, independent of the security concept.
            //dashsboards
            _wcFacade.Register("servicerequest", qc.SrITCDashboard(), MetadataIdForModules(dc.ActionRequiredForOpenRequests, fr.Tom, fr.Itom));
            _wcFacade.Register("incident", qc.IncidentITCDashboard(), MetadataIdForModules(dc.ActionRequiredForOpenIncidents, fr.Tom, fr.Itom));
            _wcFacade.Register("imac", qc.ImacsForTomITOM(), ForModules(fr.Tom));
            _wcFacade.Register("asset", "@assetControlWhereClauseProvider.AssetWhereClauseIfRegionSelected", ForModules(fr.Tom,fr.Itom));
        }



        private void CreateADWhereClauses() {
            _wcFacade.Register("incident", DR0017Method("AdIncidentsWhereClause"), ForModule(fr.Ad));
            _wcFacade.Register("incident", DR0017Method("DashboardAdIncidentsWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenIncidents, fr.Ad));
        }

        private void CreateTuiWhereClauses() {
            _wcFacade.Register("servicerequest", TuiWhereClauseProvider("DashboardServiceRequestWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenRequests, fr.Tui));
            _wcFacade.Register("servicerequest", TuiWhereClauseProvider("EuOpenRequests"), MetadataIdForModule(dc.EUOpenRequests, fr.Tui));
            _wcFacade.Register("incident", TuiWhereClauseProvider("DashboardIncidentWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenIncidents, fr.Tui));

            _wcFacade.Register("servicerequest", TuiWhereClauseProvider("ServiceRequestWhereClause"), ForModule(fr.Tui));
            _wcFacade.Register("incident", TuiWhereClauseProvider("IncidentWhereClause"), ForModule(fr.Tui));
            _wcFacade.Register("problem", TuiWhereClauseProvider("ProblemWhereClause"), ForModule(fr.Tui));
        }

        private void CreateSsoWhereClauses() {
            //change queries already registered in CreateChangeWhereClauses
            _wcFacade.Register("servicerequest", SsoWhereClauseProvider("DashboardServiceRequestWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenRequests, fr.Sso));
            _wcFacade.Register("servicerequest", SsoWhereClauseProvider("EuOpenRequests"), MetadataIdForModule(dc.EUOpenRequests, fr.Sso));
            _wcFacade.Register("incident", SsoWhereClauseProvider("DashboardIncidentWhereClause"), MetadataIdForModule(dc.ActionRequiredForOpenIncidents, fr.Sso));

            _wcFacade.Register("servicerequest", SsoWhereClauseProvider("ServiceRequestWhereClause"), ForModule(fr.Sso));
            _wcFacade.Register("incident", SsoWhereClauseProvider("IncidentWhereClause"), ForModule(fr.Sso));
            _wcFacade.Register("problem", SsoWhereClauseProvider("ProblemWhereClause"), ForModule(fr.Sso));
        }

        private void CreatePurchaseWhereClauses() {
            _wcFacade.Register("servicerequest", qc.PurchaseSR, ForModule(fr.Purchase));
            _wcFacade.Register("incident", qc.PurchaseIncident, ForModule(fr.Purchase));
            _wcFacade.Register("asset", "@assetControlWhereClauseProvider.AssetWhereClauseIfRegionSelected", ForModules(fr.Purchase));
        }

        private void CreateAssetCtrlRamWhereClauses() {
            _wcFacade.Register("asset", "@assetRamControlWhereClauseProvider.AssetWhereClause", ForModule(fr.AssetRamControl));
        }

        private void CreateAssetCtrlWhereClauses() {
            _wcFacade.Register("asset", "@assetControlWhereClauseProvider.AssetWhereClauseIfRegionSelected", ForModule(fr.AssetControl));
        }


        private string R0017Method(String methodName) {
            return "@r0017WhereClauseProvider." + methodName;
        }

        private string SsoWhereClauseProvider(String methodName) {
            return "@ssoWhereClauseProvider." + methodName;
        }

        private string TuiWhereClauseProvider(String methodName) {
            return "@tuiWhereClauseProvider." + methodName;
        }

        private string DR0017Method(String methodName) {
            return "@dashboardWhereClauseProvider." + methodName;
        }

        private string ChangeWhereClause(String methodName) {
            return "@changeWhereClauseProvider." + methodName;
        }



        private void CreateReportWhereClauses() {

            //Adding Hardware Repair Report default where clause
            var HardwareReportCondition = new WhereClauseRegisterCondition {
                Alias = "hardwarerepair",
                UserProfile = ProfileType.Itc.GetName(),
                AppContext = new ApplicationLookupContext { Schema = "hardwarerepair" }
            };
            _wcFacade.Register("incident", "@reportWhereClauseProvider.HardwareRepairReportWhereClauseWithR17", HardwareReportCondition);

            HardwareReportCondition = new WhereClauseRegisterCondition {
                Alias = "hardwarerepair",
                AppContext = new ApplicationLookupContext { Schema = "hardwarerepair" },
                Module = "xitc"
            };
            _wcFacade.Register("incident", "@reportWhereClauseProvider.HardwareRepairReportWhereClauseWithR17", HardwareReportCondition);

            HardwareReportCondition = new WhereClauseRegisterCondition {
                Alias = "hardwarerepair",
                AppContext = new ApplicationLookupContext { Schema = "hardwarerepair" },
                Module = Conditions.AnyCondition
            };
            _wcFacade.Register("incident", "@reportWhereClauseProvider.HardwareRepairReportWhereClause", HardwareReportCondition);

            //Adding Tape BackUp Report default where clause
            var TapeBackReportCondition = new WhereClauseRegisterCondition {
                Alias = "tapebackupreport",
                UserProfile = ProfileType.Itc.GetName(),
                AppContext = new ApplicationLookupContext { Schema = "tapebackupreport" }
            };
            _wcFacade.Register("incident", "@reportWhereClauseProvider.TapeBackupReportWhereClauseWithR17", TapeBackReportCondition);

            TapeBackReportCondition = new WhereClauseRegisterCondition {
                Alias = "tapebackupreport",
                AppContext = new ApplicationLookupContext { Schema = "tapebackupreport" },
                Module = "xitc"
            };
            _wcFacade.Register("incident", "@reportWhereClauseProvider.TapeBackupReportWhereClauseWithR17", TapeBackReportCondition);

            TapeBackReportCondition = new WhereClauseRegisterCondition {
                Alias = "tapebackupreport",
                AppContext = new ApplicationLookupContext { Schema = "tapebackupreport" },
                Module = Conditions.AnyCondition
            };
            _wcFacade.Register("incident", "@reportWhereClauseProvider.TapeBackupReportWhereClause", TapeBackReportCondition);

            //Adding Group Report default where clause
            var GroupReportCondition = new WhereClauseRegisterCondition {
                Alias = "groupreport",
                AppContext = new ApplicationLookupContext { Schema = "groupreport" },
                Module = Conditions.AnyCondition
            };
            _wcFacade.Register("persongroupview", qc.DefaultGroupReportQuery, GroupReportCondition);

            //Adding Escalation Incident Report default where clause
            var EscalationIncidentReport = new WhereClauseRegisterCondition {
                Alias = "escalationincident",
                AppContext = new ApplicationLookupContext { Schema = "escalationincident" }
            };
            _wcFacade.Register("incident", qc.DefaultEscalationIncidentQuery, EscalationIncidentReport);

            //Adding ITC Report default where clause
            var ITCReportCondition = new WhereClauseRegisterCondition {
                Alias = "itcreport",
                AppContext = new ApplicationLookupContext { Schema = "itcreport" },
                Module = Conditions.AnyCondition
            };
            _wcFacade.Register("persongroupview", qc.DefaultITCReportQuery, ITCReportCondition);

            var ITCReportComplementCondition = new WhereClauseRegisterCondition {
                Alias = "itcreportregionandarea",
                AppContext = new ApplicationLookupContext { Schema = "itcreportregionandarea" },
                Module = Conditions.AnyCondition
            };
            _wcFacade.Register("persongroupview", qc.DefaultITCReportRegionAndAreaQuery, ITCReportComplementCondition);

            var RI101Condition = new WhereClauseRegisterCondition {
                Alias = "RI101",
                AppContext = new ApplicationLookupContext { Schema = "RI101Export" },
                Module = Conditions.AnyCondition
            };

            _wcFacade.Register("asset", "ASSET.STATUS = '120 ACTIVE'", RI101Condition);
        }





        #region AuxMethods

        private WhereClauseRegisterCondition MetadataIdForITC(String metadataId) {
            return new WhereClauseRegisterCondition {
                Alias = metadataId,
                UserProfile = ProfileType.Itc.GetName(),
                AppContext = new ApplicationLookupContext { MetadataId = metadataId }
            };
        }

        private WhereClauseRegisterCondition MetadataIdForModule(String metadataId, FunctionalRole fr) {
            return new WhereClauseRegisterCondition {
                Alias = metadataId,
                AppContext = new ApplicationLookupContext { MetadataId = metadataId },
                Module = fr.GetName()
            };
        }


        private WhereClauseRegisterCondition MetadataId(String metadataId) {
            return new WhereClauseRegisterCondition {
                Alias = metadataId,
                AppContext = new ApplicationLookupContext { MetadataId = metadataId }
            };
        }

        private WhereClauseRegisterCondition MetadataIdForAll(String metadataId) {
            return new WhereClauseRegisterCondition {
                Alias = metadataId,
                AppContext = new ApplicationLookupContext { MetadataId = metadataId },
                Module = Conditions.AnyCondition
            };
        }

        private WhereClauseRegisterCondition ForProfile(ProfileType profile) {
            return new WhereClauseRegisterCondition {
                UserProfile = profile.GetName(),
            };
        }

        private WhereClauseRegisterCondition ForModule(FunctionalRole module) {
            return new WhereClauseRegisterCondition {
                Module = module.GetName(),
            };
        }

        private WhereClauseRegisterCondition ForModules(params FunctionalRole[] modules) {
            return new WhereClauseRegisterCondition {
                Module = String.Join(",", modules.Select(m => m.GetName())),
            };
        }

        private WhereClauseRegisterCondition MetadataIdForModules(String metadataId, params FunctionalRole[] modules) {
            return new WhereClauseRegisterCondition {
                Alias = metadataId,
                AppContext = new ApplicationLookupContext { MetadataId = metadataId },
                Module = String.Join(",", modules.Select(m => m.GetName()))
            };
        }

        private WhereClauseRegisterCondition ForAllModules() {
            return new WhereClauseRegisterCondition {
                Module = Conditions.AnyCondition,
            };
        }


        #endregion


    }
}
