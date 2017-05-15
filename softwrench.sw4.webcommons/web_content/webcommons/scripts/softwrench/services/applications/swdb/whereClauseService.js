
(function (angular) {
    "use strict";

    class whereClauseService {
        constructor(alertService, crudContextHolderService, applicationService) {
            this.alertService = alertService;
            this.crudContextHolderService = crudContextHolderService;
            this.applicationService = applicationService;
        }

        removeWhereClause() {
            const dm = this.crudContextHolderService.rootDataMap();
            if (dm["systemvalue"]) {
                return this.alertService.alert("Cannot delete a whereclause that contains a System Value");
            }

            return this.applicationService.deleteCrud("Are you sure you want to delete this Whereclause?");
        }


    }
  
    whereClauseService.$inject = ['alertService', 'crudContextHolderService', 'applicationService'];

    angular.module("sw_layout").service("whereClauseService", whereClauseService);

    //#endregion

})(angular);