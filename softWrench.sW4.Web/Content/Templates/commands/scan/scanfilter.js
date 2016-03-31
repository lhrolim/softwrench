(function (angular) {
    "use strict";

angular
    .module("sw_layout")
    .controller("ScanFilterController", ["$http", "$scope", "$rootScope", "restService", "searchService", "contextService", ScanFilterController]);

function ScanFilterController($http, $scope, $rootScope, restService, searchService, contextService) {
    $scope.scanOrder = [];
    $scope.filterFields = [];
    initScanFilter();

    function initScanFilter() {
        var scanOrderString = contextService.retrieveFromContext($scope.schema.schemaId + "ScanOrder");
        if (scanOrderString) {
            $scope.scanOrder = scanOrderString.split(",");
        } else {
            $scope.scanOrder = [];
        }

        var filterField;
        var displayables = $scope.schema.displayables;
        getFilterFields(displayables);
    };

    function getFilterFields(displayables) {
        for (var i = 0; i < displayables.length; i++) {
            if (!displayables[i].isHidden && !displayables[i].isReadOnly) {
                if (displayables[i].type == "ApplicationSection") {
                    getFilterFields(displayables[i].displayables);
                } else {
                    $scope.filterFields.push(
                        { "label": displayables[i].label, "attribute": displayables[i].attribute });
                }
            }
        }
    };

    $scope.closeScanFilterModal = function() {
        var modal = $('#scanfilterModal');
        modal.appendTo('#scanFilterController');
        modal.modal('hide');
    };

    $scope.showLabel = function () {
        //use global property to hide/show labels
        return contextService.getFromContext("UIShowToolbarLabels", false, true);
    };
    
    $scope.showScanFilterModal = function () {
        var modal = $('#scanfilterModal');
        modal.appendTo('body').modal('show');
    };

    $scope.getScanConfiguration = function () {
        return $scope.scanOrder;
    };

    $scope.newScanRow = function () {
        var selectedField = document.getElementById("newFilterField");
        var newRow = selectedField.selectedIndex;
        var remainingFilterFields = $scope.remainingFilterFields();
        var attribute = remainingFilterFields[newRow].attribute;
        if ($scope.scanOrder.indexOf(attribute) === -1) {
            $scope.scanOrder.push(attribute);
        }
    };

    $scope.deleteScanRow = function (row) {
        var index = $scope.scanOrder.indexOf(row);
        $scope.scanOrder.splice(index, 1);
    };

    $scope.saveScanList = function () {
        // Convert the list of field labels for scan order to a list of the attributes
        var scanListAttributes = [];
        for (var attribute in $scope.scanOrder) {
            if (!$scope.scanOrder.hasOwnProperty(attribute)) continue;
            scanListAttributes.push($scope.scanOrder[attribute]);
        }
        var scanAttributesString = scanListAttributes.toString();
        var parameters = {
            fullKey: $scope.schema.properties['config.fullKey'],
            value: scanAttributesString
        };
        restService.invokePost("Configuration", "SetConfiguration", parameters, null,
            contextService.insertIntoContext($scope.schema.schemaId + "ScanOrder", scanAttributesString), null);
        $scope.closeScanFilterModal();
    };

    $('#scanfilterModal').on('hidden.bs.modal', function() {
            //searchService.refreshGrid();
    });

    $scope.getLabelByAttribute = function(searchAttribute) {
        for (var item in $scope.filterFields) {
            if (!$scope.filterFields.hasOwnProperty(item)) continue;
            if ($scope.filterFields[item].attribute === searchAttribute) {
                return $scope.filterFields[item].label;
            }
        }
        return null;
    };

    $scope.getAttributeByLabel = function (searchLabel) {
        for (var item in $scope.filterFields) {
            if (!$scope.filterFields.hasOwnProperty(item)) continue;
            if ($scope.filterFields[item].label === searchLabel) {
                return $scope.filterFields[item].attribute;
            }
        }
        return null;
    };

    $scope.remainingFilterFields = function() {
        var remainingFilterFields = [];
        for (var item in $scope.filterFields) {
            if (!$scope.filterFields.hasOwnProperty(item)) continue;
            if ($scope.scanOrder.indexOf($scope.filterFields[item].attribute) === -1) {
                remainingFilterFields.push($scope.filterFields[item]);
            }
        }
        return remainingFilterFields;
    };

    $scope.getSchemaType = function () {
        return $scope.schema.stereotype;
    };
}

})(angular);