var app = angular.module('sw_layout');

app.directive('breadcrumb', function ($rootScope, $log, $compile) {
    var log = $log.getInstance('sw4.breadcrumb');

    return {
        scope: {
            schema: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {
            scope.$watch('title', function () {
                //element.html(getBreadCrumbHTML(log, scope.menu, scope.title));

                var template = (getBreadCrumbHTML(log, scope.menu, scope.title));
                var content = $compile(template)(scope);
                element.html(content);

            });
        },
        controller: function ($scope, $rootScope) {
            log.debug('Breadcrumb Controller');

            $scope.clicktest = function () {
                log.debug('click test');
            };
        },
    }
});

var seperator = '<span class="part seperator">/</span>';

function getBreadCrumbHTML(log, menu, current) {
    var path = '<div class="part"><a data-toggle="dropdown" aria-expanded="false"><i class="fa fa-home"></i>&ensp;';

    //get the title of the home item
    if (menu.leafs[0].controller == 'Dashboard') {
        path += 'Dashboard';
    } else {
        path += 'Home';
    }
    path += '</a>';

    //add submenu
    path += getChildMenu(log, menu.leafs, null);
    path += '</div>';

    //append child parts
    if (current != undefined) {
        var foundPath = findCurrentPage(log, menu.leafs, current, null);

        if (foundPath) {
            path += seperator;
            path += foundPath;
        }
    }

    return path;
}

function findCurrentPage(log, leafs, current, parent) {
    var path = '';

    if (leafs != null) {
        for (var id in leafs) {
            var newPath = findCurrentPage(log, leafs[id].leafs, current, leafs[id]);
            var icon = '<i class="' + leafs[id].icon + '"></i>&ensp;'

            //if this is part of the breadcrumb
            if ((newPath != undefined && newPath != '') || (leafs[id].title == current)) {

                //get the child menu items
                var childMenu = getChildMenu(log, leafs, leafs[id]);

                //build the breadcrumb part and menu
                path += '<div class="part">';

                //if child menu found, add the dropdown toggle
                if (newPath) {
                    path += '<a data-toggle="dropdown" aria-expanded="false">';
                } else {
                    path += '<a>';
                }

                //add the icon and title
                path += icon + leafs[id].title;
                path += '</a>';

                //add the child menu items
                if (newPath) {
                    path += getChildMenu(log, leafs, leafs[id]);
                }

                path += '</div>';

                //if found add the next breadcrumb part
                if (newPath != undefined && newPath != '') {
                    path += seperator + newPath;
                }
            }
        }
    }

    return path;
}


function getChildMenu(log, leafs, parent) {
    var path = '';
    var searchLeafs = null;

    //if no parent use the get the whole menu, else the child items from the parent
    if (parent == null) {
        searchLeafs = leafs
    } else {
        if (parent.leafs != null) {
            searchLeafs = parent.leafs;
        }
    }

    if (searchLeafs != null) {
        path += '<ul class="dropdown-menu" role="menu">';
        for (var id in searchLeafs) {
            if (searchLeafs[id].title != null) {
                var childMenu = getChildMenu(log, searchLeafs[id].leafs, searchLeafs[id]);

                //if child menu found, display as submenu
                if (childMenu) {
                    path += '<li class="dropdown-submenu"><a data-toggle="dropdown" aria-expanded="false">';
                } else {
                    path += '<li><a ng-click="testclick()">';
                }

                //build the menu item
                path += '<i class="' + searchLeafs[id].icon + '"></i>&ensp;' + searchLeafs[id].title;
                path += '</a>';

                //add the child menu items
                if (childMenu) {
                    path += childMenu;
                }

                path += '</li>';
            }
        }
        path += '</ul>';
    }

    return path;
}