using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.pae.classes.com.cts.pae.configuration {
    public class PaeWhereClauseRegistry : ISWEventListener<ApplicationStartedEvent> {

        private const string MobileAssetWhereClause = @"
                    (paeasset.assetnum not in ('MSI-WO', 'PODS', 'Tenant-TP', 'Tenant-TR')) and 
                    (paeasset.location not in ('CAFB-VM-DRMO', 'CAFB-VM-VRC')) and 
                    (paeasset.recordtype = 'TR')";

        private readonly IWhereClauseFacade _whereClauseFacade;

        public PaeWhereClauseRegistry(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "pae") return;

            var offLineCondition = new WhereClauseRegisterCondition() {
                Alias = "offline", OfflineOnly = true, Global = true
            };

            _whereClauseFacade.Register("transportation", MobileAssetWhereClause, offLineCondition);
        }

    }
}