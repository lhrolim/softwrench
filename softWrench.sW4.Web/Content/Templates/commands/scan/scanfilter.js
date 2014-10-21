function ScanFilterController($http, $scope, restService, searchService) {
    $scope.scanOrder = [];
    $scope.filterFields = [];
    
    $scope.showScanFilterModal = function (schema) {
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
        var modal = $('[data-class="scanfilterModal"]');
        modal.appendTo('body').modal('show');
    };

    $scope.loadScanConfiguration = function () {
        var parameters = {
            fullKey: "/Global/Grids/ScanBar"
        };
        var getUrl = restService.getActionUrl("Configuration", "GetConfiguration", parameters);

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
            fullKey: "/Global/Grids/ScanBar",
            value: scanAttributesString
        };
        restService.invokePost("Configuration", "SetConfiguration", parameters, null, null, null);
        searchService.refreshGrid();
        // Once the modal is saved, move the modal back to the controller to avoid appending multiple modals to the body
        var modal = $('[data-class="scanfilterModal"]');
        modal.appendTo('#scanFilterController');
    };

    // When the modal is hidden append it to its original location in the controller
    $('#scanfilterModal').on('hidden.bs.modal', function() {
        var modal = $('[data-class="scanfilterModal"]');
        modal.appendTo('#scanFilterController');
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