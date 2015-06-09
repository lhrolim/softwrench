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

var seperator = '&emsp;<span class="seperator">/</span>&emsp;';

function getBreadCrumbHTML(log, menu, current) {
    log.debug(menu, current);

    var path = '<i class="fa fa-home"></i>&ensp;';

    if (menu.leafs[0].controller == 'Dashboard') {
        path += 'Dashboard';
    } else {
        path += 'Home';
    }

    if (current != undefined) {
        var foundPath = findCurrentPage(log, menu.leafs, current);

        if (foundPath) {
            path += seperator;
            path += foundPath;
        }
    }

    return path;
}

function findCurrentPage(log, leafs, current) {
    var path = '';

    if (leafs != null) {
        for (var id in leafs) {
            var newPath = findCurrentPage(log, leafs[id].leafs, current);

            var icon = '<i class="' + leafs[id].icon + '"></i>&ensp;'
            if (newPath != undefined && newPath != '') {
                path = icon + leafs[id].title + seperator + newPath;
            } else if (leafs[id].title == current) {
                path = icon + leafs[id].title;
            }
        }
    }

    return path;
}