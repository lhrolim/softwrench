using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.pae.classes.com.cts.pae.configuration {
    public class PaeWhereClauseRegistry : ISWEventListener<ApplicationStartedEvent> {

        private const string AssetWhereClause = "paeasset.recordtype='PROP' and paeasset.propcondition=1";

        private const string TransportationAssetWhereClause = @"
                    (paeasset.assetnum not in ('MSI-WO', 'PODS', 'Tenant-TP', 'Tenant-TR')) and 
                    (paeasset.location not in ('CAFB-VM-DRMO', 'CAFB-VM-VRC')) and 
                    (paeasset.recordtype = 'TR')";

        private readonly IWhereClauseFacade _whereClauseFacade;

        public PaeWhereClauseRegistry(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "pae") return;

            var transportationCondition = new WhereClauseRegisterCondition() {
                Global = true,
                Alias = "transportation",
                Description = "Limits assets to transportation assets (VIN scannable)"
            };
            _whereClauseFacade.Register("transportation", TransportationAssetWhereClause, transportationCondition);

            var assetCondition = new WhereClauseRegisterCondition() {
                Global = true,
                Alias = "asset",
            };
            _whereClauseFacade.Register("asset", AssetWhereClause, assetCondition);
        }

    }
}