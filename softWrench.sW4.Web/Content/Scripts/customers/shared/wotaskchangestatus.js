(function (angular) {
    "use strict";

angular.module('sw_layout')
.controller('WoTaskController', ['$scope', '$http', 'modalService', 'applicationService', function ($scope, $http, modalService, applicationService) {

    $scope.openstatusmodal = function (item, event) {
        if (event) {
            event.stopPropagation();
            event.preventDefault();
        }
        //        currentmetadataparameter: "status={0}".format(item[status])
        applicationService.getApplicationDataPromise("woactivity", "editstatusschema", { id: item["workorderid"] }).then(function (result) {
            modalService.show(result.data.schema, result.data.resultObject.fields, { title: 'Change Status', cssclass: "dashboardmodal" }, function (modalData) {
                modalService.hide();
            });
        });
    }
}
]);

})(angular);