
(function (angular, moment) {
    "use strict";

    angular.module("sw_layout").controller("dispatchbuttonctrl", ["$scope", "$q", "applicationService", "crudContextHolderService", "alertService", dispatchbuttonctrl]);

    function dispatchbuttonctrl($scope, $q, applicationService, crudContextHolderService, alertService) {

        const doDispatch = function(datamap) {
            datamap["#dispatching"] = true;
            return applicationService.save({ datamap});
        }

        const shouldNotify = function (inverters, dispatchexpecteddate) {
            const deadLineHours = !!inverters.find(inverter => inverter.failureclass === "1") ? 24 : 48;
            const expectedDispatch = moment(dispatchexpecteddate ? dispatchexpecteddate : undefined);
            const arrivedDeadline = expectedDispatch.add(deadLineHours, "hour");

            return !!inverters.find((inverter) => {
                if (!inverter["parts_"] || inverter["parts_"].length === 0) return false;
                return !!inverter["parts_"].find((part) => {
                    return part["deliverymethod"] === "shipment" && arrivedDeadline.isBefore(part["expecteddate"]);
                }); 
            });
        }

        $scope.isOutputMode = () => {
            return crudContextHolderService.isOutputMode();
        }

        $scope.dispatch = () => {
            const dm = crudContextHolderService.rootDataMap();

            if (!dm["inverters_"] || dm["inverters_"].length === 0) return doDispatch(dm);

            const inverters = dm["inverters_"];
            const dispatchexpecteddate = dm["immediatedispatch"] === "false" ? dm["dispatchexpecteddate"] : null;

            const promise = !shouldNotify(inverters, dispatchexpecteddate) ? $q.when(null) : alertService.confirm("Are you sure you want to dispatch prior to having all the parts?");

            return promise.then(() => doDispatch(dm));
        }
    }
})(angular, moment);
