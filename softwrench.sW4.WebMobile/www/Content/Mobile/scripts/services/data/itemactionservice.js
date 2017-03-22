(function (angular) {
    "use strict";

    function itemActionService($q, synchronizationFacade, crudContextService, $ionicPopup) {
        //#region Utils
        //#endregion

        //#region Public methods

        /**
         * Checks if current item can be quicksynced 
         * i.e. isDirty and does not have unsaved changes.
         * 
         * @returns {Boolean} 
         */
        function shouldAllowQuickSyncForCurrentItem() {
            const item = crudContextService.currentDetailItem();
            return !!item && item.isDirty && !crudContextService.hasDirtyChanges();
        }

        /**
         * Checks if the current item can be deleted or restored.
         * 
         * @returns {Boolean} 
         */
        function shouldAllowDeleteOrRestoreForCurrentItem() {
            const item = crudContextService.currentDetailItem();
            if (!item) {
                return false;
            }
            const restorable = item.remoteId && item.isDirty && !!item.originaldatamap;
            const deletable = !item.remoteId;
            return !item.pending && (restorable || deletable) && !crudContextService.hasDirtyChanges();
        }

        /**
         * Quicksyncs current item.
         * If successful will redirect to the grid.
         * 
         * @returns {Promise<Void>} 
         */
        function quickSyncCurrentItem() {
            const item = crudContextService.currentDetailItem();
            return synchronizationFacade.syncItem(item)
                .then(() => $ionicPopup.alert({
                    title: "Quick Sync",
                    template: "Quick Sync was successful"
                }))
                .then(() => crudContextService.refreshGrid())
                .catch(error => $ionicPopup.alert({
                    title: "Quick Sync",
                    template: error.message || "An error happened during quick sync"
                }));
        }

        /**
         * Attempts to delete or restore the item to it's original state (from server).
         * 
         * @returns {Promise<Boolean>} whether or not the item was deleted/restored 
         */
        function deleteOrRestoreItem(item) {
            const restorable = item.remoteId && item.isDirty && !!item.originaldatamap;
            const deletable = !item.remoteId;

            if (item.pending || (!restorable && !deletable)) {
                return $q.when(false);
            }

            const currentTitle = crudContextService.currentTitle();

            const confirmConfig = restorable
                ? { title: "Cancel Changes", template: `Are you sure you want to cancel changes made to this ${currentTitle}` }
                : { title: `Delete ${currentTitle}`, template: `Are you sure you want to delete this ${currentTitle} created locally` }

            return $ionicPopup.confirm(confirmConfig).then(res => {
                if (!res) return false;

                const promise = restorable
                    ? crudContextService.restoreItemToOriginalState(item)
                    : crudContextService.deleteLocalItem(item);

                return promise
                    .then(() => $ionicPopup.alert({ title: `${currentTitle} was successfuly ${restorable ? "restored" : "deleted"}` }))
                    .then(() => true);
            });
        }

        /**
         * Attempts to delete or restore the current item to it's original state (from server).
         * If successfull, will redirect to the grid.
         * 
         * @returns {Promise<Void>}
         */
        function deleteOrRestoreCurrentItem() {
            const item = crudContextService.currentDetailItem();
            return deleteOrRestoreItem(item).then(res => res ? crudContextService.refreshGrid() : null);
        }

        //#endregion

        //#region Service Instance
        const service = {
            quickSyncCurrentItem,
            deleteOrRestoreCurrentItem,
            deleteOrRestoreItem,
            shouldAllowQuickSyncForCurrentItem,
            shouldAllowDeleteOrRestoreForCurrentItem
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("itemActionService", ["$q", "synchronizationFacade", "crudContextService", "$ionicPopup", itemActionService]);

    //#endregion

})(angular);