(function (angular) {
    "use strict";

angular.module("sw_layout").controller("SamelinePickersController", SamelinePickersController);
function SamelinePickersController($scope, $rootScope, formatService, $filter) {
    "ngInject";

    var getCurrentDateString = function () {
        var date = new Date();
        return date.mmddyyyy();
    }

    var joinDates = function (fields) {
        // Set value to null if neither date or time are present
        var dateParameterName = $scope.fieldMetadata.parameters['joinattribute'] + '_date';
        var timeParameterName = $scope.fieldMetadata.parameters['joinattribute'] + '_time';
        if (fields[dateParameterName] == undefined && fields[timeParameterName] == undefined) {
            return;
        }

        // If date is not present, then apply today's date with desire time
        var finalize_date = fields[dateParameterName] == undefined ? getCurrentDateString() : fields[dateParameterName];

        // If time is not present, then apply no time. 
        var finalize_time = fields[timeParameterName] == undefined ? "" : " " + fields[timeParameterName];

        var combinedDates = finalize_date + finalize_time;

        // Combine both date and time to form desire tiem.  
        if (combinedDates.trim() === "") {
            fields[$scope.fieldMetadata.parameters['joinattribute']] = null;
        } else {
            fields[$scope.fieldMetadata.parameters['joinattribute']] = combinedDates;
        }

        
        
    };

    function addRequiredDisplayable(fieldMetadata, displayables) {
        if ("true" !== fieldMetadata.parameters["required"]) {
            return;
        }
        var alreadyadded = displayables.filter(function (displayable) {
            return displayable.attribute === fieldMetadata.parameters['joinattribute'];
        });
        if (alreadyadded.length >= 2) {
            return;
        }
        displayables.push({
            //if the field is required, adding it to the schema so that validation proceeds
            attribute: fieldMetadata.parameters['joinattribute'],
            label: fieldMetadata.header.label,
            requiredExpression: 'true',
            rendererParameters: {}
        });
    }

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

        $scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_date'] = valueToUse;
        $scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_time'] = valueToUse;

        addRequiredDisplayable($scope.fieldMetadata, $scope.schema.displayables);


        $scope.$on("sw_beforesubmitprevalidate_internal", function (event, fields) {
            joinDates(fields);
        });
    };

    doInit();

    $scope.isMobile = function () {
        var isMobileVar = isMobile();
        if (isMobileVar) {
            var dateField = $scope.fieldMetadata.parameters['joinattribute'] + '_date';
            var timeField = $scope.fieldMetadata.parameters['joinattribute'] + '_time';
            $scope.datamap[dateField] = $filter('date')($scope.datamap[dateField], 'yyyy-MM-dd');
            $scope.datamap[timeField] = $filter('date')($scope.datamap[timeField], 'HH:mm');
        }

        return isMobileVar;
    };

    $scope.isDesktop = function () {
        return isDesktop();
    };
}

window.SamelinePickersController = SamelinePickersController;

var hideKeyboard = function () {
    document.activeElement.blur();
    $("input").blur();
};

window.hideKeyboard = hideKeyboard;

})(angular);