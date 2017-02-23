var app = angular.module('sw_layout');

function SamelinePickersController($scope, $rootScope, formatService) {
    "ngInject";
    var getCurrentDateString = function() {
        var date = new Date();
        return date.mmddyyyy();
    }
    
    $scope.formClass = function () {
        return GetBootstrapFormClass(6);
    }

    var joinDates = function (fields) {
        if ($scope.date1 == undefined && $scope.date2 == undefined) {
            return;
        }
        var date1St = $scope.date1 == undefined ? getCurrentDateString() : $scope.date1; 
        var date2St = $scope.date2 == undefined ? "" : " " + $scope.date2;
        fields[$scope.fieldMetadata.parameters['joinattribute']] = date1St + date2St;
    };

    function doInit() {
        var fieldMetadata = $scope.fieldMetadata;
        var defaultValue = fieldMetadata.parameters['default'];
        if (fieldMetadata.parameters['timeformat'] == undefined) {
            fieldMetadata.parameters['timeformat'] = 'HH:mm';
        }
        if (fieldMetadata.parameters['dateformat'] == undefined) {
            fieldMetadata.parameters['dateformat'] = 'MM/dd/yyyy';
        }
        var targetDate = $scope.datamap[fieldMetadata.parameters['joinattribute']];
        var valueToUse = targetDate == undefined ? defaultValue : targetDate;

        $scope.date1 = valueToUse;
        $scope.date2 = valueToUse;
            
        $scope.$on("sw_beforeSave", function (event, fields) {
            joinDates(fields);
        });
    };

    doInit();
}
