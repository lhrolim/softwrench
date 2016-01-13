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
            log.debug($scope);

            $scope.clearSearch = function () {
                //reset the seach text
                $scope.ngModel = '';

                //trigger resize to resize scroll panes
                $(window).trigger('resize');
            };

            $scope.getPlaceholder = function () {
                var string = 'Search';

                //if a placeholder has been passed
                if ($scope.placeholder) {
                    string = $scope.placeholder;
                }

                return string;
            };
        }
    }
});
