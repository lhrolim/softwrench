var app = angular.module('sw_layout');

app.directive('breadcrumb', function ($rootScope, $log, $timeout) {
    var log = $log.getInstance('sw4.breadcrumb');

    return {
        scope: {
            schema: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {
            scope.$watch('title', function () {
                //log.debug('current title', scope.title);
                element.html(getBreadCrumbHTML(log, scope.menu, scope.title));
            });  
        },
    }
});

function getBreadCrumbHTML(log, menu, current) {
    log.debug(menu, current);

    if (current != undefined) {
        //TODO: look for dashboard, if not there add home icon

        var path = findCurrentPage(log, menu.leafs, current, "")

        return path;
    }
}

function findCurrentPage(log, leafs, current, parent) {
    var path = '';

    if (leafs != null) {
        for (var id in leafs) {
            var newPath = findCurrentPage(log, leafs[id].leafs, current, parent);

            if (newPath != undefined && newPath != '') {
                path = '<i class="' + leafs[id].icon + '"></i>&ensp;' + leafs[id].title + '&ensp;/&ensp;' + newPath;
            } else if (leafs[id].title == current) {
                path = '<i class="' + leafs[id].icon + '"></i>&ensp;' + leafs[id].title;
            }
        }
    }

    return path;
}