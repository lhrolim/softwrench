(function (mobileServices, angular, _) {
    "use strict";

    class synchronizationResyncService {

        constructor($q,swdbDAO,configurationService){
            this.$q = $q;
            this.swdbDAO = swdbDAO;
            this.configurationService = configurationService;
        }


        shouldFullResync(considerDirty, forceresync) {
            const dirtyPromise = considerDirty ? this.hasDataToSync() : this.$q.when(false);

            return dirtyPromise.then(hasDirty => {
                if (hasDirty) {
                    //at initial sync, or if there´s something to be uploaded (dirty) regardless we should not do a full resync
                    return false;
                }
                return this.configurationService.getConfigs([ConfigurationKeys.FacilitiesChanged, ConfigurationKeys.ServerConfig]).then(configs => {
                    if (!configs[ConfigurationKeys.ServerConfig]){
                        return false;
                    }

                    if (configs[ConfigurationKeys.ServerConfig]["client"] === "firstsolar") {
                        //First solar has been using (controversially) full reset due to a bug on assingment dates
                        return forceresync;
                    }
                    return !!configs[ConfigurationKeys.FacilitiesChanged];
                })
            });
        }

        hasDataToSync() {
            return this.swdbDAO.countByQuery("DataEntry", "(isDirty=1 or hasProblem=1) and pending=0").then(count => count > 0);
        }

    }

    synchronizationResyncService.$inject = ["$q", "swdbDAO", "configurationService"];

    mobileServices.service('synchronizationResyncService', synchronizationResyncService);

})(mobileServices, angular, _);
