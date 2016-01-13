var app = angular.module('sw_layout');

app.directive('searchBox', function (contextService, $log) {
    var log = $log.getInstance('sw4.searchbox');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/searchbox.html'),
        scope: {
            ngModel: '=',
            placeholder: '='
        },
        controller: function ($scope) {
            $scope.clearSearch = function () {
                //reset the seach text
                $scope.ngModel = '';

                //trigger resize to resize scroll panes
                $(window).trigger('resize');
            };

            //set the default input placeholder
            if (!$scope.placeholder) {
                $scope.placeholder = 'Search';
            }
        }
    }
});
