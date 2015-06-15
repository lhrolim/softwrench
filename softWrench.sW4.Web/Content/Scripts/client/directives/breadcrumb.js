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
                element.html(getBreadCrumbHTML(log, scope.menu, scope.title));
            });  
        },
    }
});

var seperator = '<span class="part seperator">/</span>';

function getBreadCrumbHTML(log, menu, current) {
    log.debug(menu, current);

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
            if (newPath != undefined && newPath != '') {

                //build button and dropdown menu
                path += '<div class="part">';
                path += '<a data-toggle="dropdown" aria-expanded="false">';
                path += icon + leafs[id].title;
                path += '</a>';
                path += getChildMenu(log, leafs, leafs[id]);
                path += '</div>';
                path += seperator + newPath;

            } else if (leafs[id].title == current) {

                //build the button and dropdown menu
                path += '<div class="part">';
                path += '<a>';
                path += icon + leafs[id].title;

                //build the dropdown menu
                if (leafs[id].leafs != null) {
                    //TODO: get children menu items
                    log.debug('get children menu items', leafs[id]);
                }

                path += '</a>';
                path += '</div>';
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
                var menuItem = getChildMenu(log, searchLeafs[id].leafs, searchLeafs[id]);

                //if child items found, display as submenu
                if (menuItem) {
                    path += '<li class="dropdown-submenu">';
                } else {
                    path += '<li>';
                }

                //build the menu item
                path += '<a data-toggle="dropdown" aria-expanded="false"><i class="' + searchLeafs[id].icon + '"></i>&ensp;' + searchLeafs[id].title + '</a>';

                //append the child menu items
                if (menuItem) {
                    path += menuItem;
                }

                path += '</li>';
            }
        }
        path += '</ul>';
    }

    return path;
}