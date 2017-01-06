(function (angular) {
    "use strict";

    function statusHandlerService(crudContextHolderService) {
        function filterSelectableOptions(item) {
            let dm = crudContextHolderService.rootDataMap();
            if (dm && !dm.originalstatus.equalsAny('INPROG', 'RESOLVED') && item.value.equalIc('RESOLVED')) {
                return false;
            }

            return true;
        }
                
        var service = {
            filterSelectableOptions: filterSelectableOptions
        };
        return service;
    }

    angular.module("sw_layout").service("statusHandlerService", ["crudContextHolderService", statusHandlerService]);
})(angular);