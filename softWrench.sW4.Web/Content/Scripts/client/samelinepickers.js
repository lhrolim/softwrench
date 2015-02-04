var app = angular.module('sw_layout');

function SamelinePickersController($scope, $rootScope, formatService, $filter) {

    var getCurrentDateString = function() {
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

        // Combine both date and time to form desire tiem.  
        fields[$scope.fieldMetadata.parameters['joinattribute']] = finalize_date + finalize_time;
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

        $scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_date'] = valueToUse;
        $scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_time'] = valueToUse;
            
        $scope.$on("sw_beforesubmitprevalidate_internal", function (event, fields) {
            joinDates(fields);
        });
    };

    doInit();

    $scope.isMobile = function () {
        if (isMobile()) {
            $scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_date'] = $filter('date')($scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_date'], 'yyyy-MM-dd');
            $scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_time'] = $filter('date')($scope.datamap[$scope.fieldMetadata.parameters['joinattribute'] + '_time'], 'HH:mm');
        }

        return isMobile();
    };

    $scope.isDesktop = function () {
        return isDesktop();
    };
}

var hideKeyboard = function () {
    document.activeElement.blur();
    $("input").blur();
};
