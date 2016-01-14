(function (app) {
    "use strict";

app.directive('dashboardsdone', function ($timeout) {
    "ngInject";

    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
                });
            }
        }
    };
});

})(app);