function ScanFilterController($http, $scope, restService, searchService) {
    $scope.scanOrder = [];
    $scope.filterFields = [];

    $scope.closeScanFilterModal = function() {
        var modal = $('#scanfilterModal');
        modal.appendTo('#scanFilterController');
        modal.modal('hide');
    };
    
    $scope.showScanFilterModal = function (schema) {
        $scope.fullKey = schema.properties['config.fullKey'];
        if (!$scope.filterFields.length) {
            var filterField;
            var displayables = schema.displayables;
            for (item in displayables) {
                if (!displayables[item].isHidden) {
                    filterField = { "label": displayables[item].label, "attribute": displayables[item].attribute };
                    $scope.filterFields.push(filterField);
                }
            }
        }
        
        $scope.loadScanConfiguration();
        var modal = $('#scanfilterModal');
        modal.appendTo('body').modal('show');
    };

    $scope.loadScanConfiguration = function () {
        var getUrl = restService.getActionUrl("Configuration", "GetConfiguration", { fullKey: $scope.fullKey });

        $http.get(getUrl).success(function (data) {
            if (!data) {
                return null;
            }
            if (!$scope.scanOrder.length) {
                data = data.substring(1, data.length - 1);
                var scanOrder = data.split(",");
                var label;
                for (var item in scanOrder) {
                    label = $scope.getLabelByAttribute(scanOrder[item]);
                    $scope.scanOrder.push(label);
                }
            }
        });
    };

    $scope.getScanConfiguration = function () {
        return $scope.scanOrder;
    };

    $scope.newScanRow = function () {
        var selectedField = document.getElementById("newFilterField");
        var newRow = selectedField.selectedIndex;
        var remainingFilterFields = $scope.remainingFilterFields();
        var label = remainingFilterFields[newRow].label;
        if ($scope.scanOrder.indexOf(label) === -1) {
            $scope.scanOrder.push(label);
        }
    };

    $scope.deleteScanRow = function (row) {
        var index = $scope.scanOrder.indexOf(row);
        $scope.scanOrder.splice(index, 1);
    };

    $scope.saveScanList = function () {
        // Convert the list of field labels for scan order to a list of the attributes
        var scanListAttributes = [];
        for (label in $scope.scanOrder) {
            scanListAttributes.push($scope.getAttributeByLabel($scope.scanOrder[label]));
        }
        var scanAttributesString = scanListAttributes.toString();
        var parameters = {
            fullKey: $scope.fullKey,
            value: scanAttributesString
        };
        restService.invokePost("Configuration", "SetConfiguration", parameters, null, null, null);
        $scope.closeScanFilterModal();
    };

    $('#scanfilterModal').on('hidden.bs.modal', function() {
            searchService.refreshGrid();
    });

    $scope.getLabelByAttribute = function(searchAttribute) {
        for (item in $scope.filterFields) {
            if ($scope.filterFields[item].attribute === searchAttribute) {
                return $scope.filterFields[item].label;
            }
        }
        return null;
    };

    $scope.getAttributeByLabel = function (searchLabel) {
        for (item in $scope.filterFields) {
            if ($scope.filterFields[item].label === searchLabel) {
                return $scope.filterFields[item].attribute;
            }
        }
        return null;
    };

    $scope.remainingFilterFields = function() {
        var remainingFilterFields = [];
        for (item in $scope.filterFields) {
            if ($scope.scanOrder.indexOf($scope.filterFields[item].label) === -1) {
                remainingFilterFields.push($scope.filterFields[item]);
            }
        }
        return remainingFilterFields;
    };

}