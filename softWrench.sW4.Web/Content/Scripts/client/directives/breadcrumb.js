var app = angular.module('sw_layout');

app.directive('breadcrumb', function ($rootScope, $log, $compile, menuService) {
    var log = $log.getInstance('sw4.breadcrumb');

    return {
        scope: {
            schema: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {
            log.debug(scope.menu);

            scope.$watch('title', function (newValue, oldValue) {
                var template = (getBreadCrumbHTML(log, scope.menu, scope.title, menuService));
                if (template != null) {
                    var content = $compile(template)(scope);
                    element.html(content);
                }
            });
        },

    }
});


app.directive('bcMenuItem', function ($rootScope, $log, $compile,menuService) {
    var log = $log.getInstance('sw4.breadcrumb Menu Item');



    return {
        link: function (scope, element, attr) {
            $compile(element.contents())(scope);
        },
        controller: function ($scope, alertService, validationService) {



            $scope.goToApplication = function (title) {
                var leaf = findleafByTitle(log, $scope.menu.leafs, title);
                var msg = "Are you sure you want to leave the page?";
                if (validationService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        menuService.goToApplication(leaf, null);
                        $scope.$digest();
                    }, msg, function () { return; });
                }
                else {
                    menuService.goToApplication(leaf, null);
                }
            };

            $scope.doAction = function (title) {
                //update title when switching to dashboard
                $scope.$emit('sw_titlechanged', null);

                var leaf = findleafByTitle(log, $scope.menu.leafs, title);
                var msg = "Are you sure you want to leave the page?";
                if (validationService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        menuService.doAction(leaf, null);
                        $scope.$digest();
                    }, msg, function () { return; });
                }
                else {
                    menuService.doAction(leaf, null);
                }
            };
        },
    }
});


function getBreadCrumbHTML(log, menu, current,menuService) {
    var path = '<div class="part main" bc-menu>';
    path += '<a data-toggle="dropdown" aria-expanded="false">';
    path += '<i class="fa fa-bars"></i>';
    path += '&ensp;<i class="fa fa-caret-down"></i>';
    path += '</a>';

    //add submenu
    path += getChildMenu(log, menu.leafs, null, menuService);
    path += '</div>';

    //append child parts
    if (current != undefined) {
        var foundPath = findCurrentPage(log, menu.leafs, current, null);

        if (foundPath) {
            path += seperator;
            path += foundPath;
        } else {
            return null;
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
                    path += '<a ng-click="';

                    if (leafs[id].type == 'ActionMenuItemDefinition') {
                        path += 'doAction';
                    } else if (leafs[id].type == 'ApplicationMenuItemDefinition') {
                        path += 'goToApplication';
                    } else if (leaf.type == 'ExternalLinkMenuItemDefinition') {
                        path += '<a target="_blank" href="{0}"'.format(leaf.link);
                    }

                    path += '(\'' + leafs[id].title + '\')">';
                }


                //add the icon and title
                path += icon + leafs[id].title;

                if (newPath) {
                    path += '&ensp;<i class="fa fa-caret-down"></i>';
                }


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

function findleafByTitle(log, leafs, title) {

    var found = null;

    if (leafs != null) {
        for (var id in leafs) {
            var search = findleafByTitle(log, leafs[id].leafs, title);

            //if a child is the current item, pass it along
            if (search != null) {
                found = search;
            }

            //if this is the current item
            if (leafs[id].title == title) {
                found = leafs[id];
            }
        }
    }

    return found;
}

function getChildMenu(log, leafs, parent,menuService) {
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
        path += '<ul class="dropdown-menu" role="menu" bc-menu>';
        for (var id in searchLeafs) {
            var leaf = searchLeafs[id];
            if (leaf.title != null) {
                var childMenu = getChildMenu(log, leaf.leafs, leaf, menuService);

                //if child menu found, display as submenu
                if (childMenu) {
                    path += '<li class="dropdown-submenu"><a data-toggle="dropdown" aria-expanded="false">';
                } else {
                    path += '<li><a bc-menu-item ng-click="';

                    if (leaf.type == 'ActionMenuItemDefinition') {
                        path += 'doAction';
                    } else if (leaf.type == 'ApplicationMenuItemDefinition') {
                        path += 'goToApplication';
                    } else if (leaf.type == 'ExternalLinkMenuItemDefinition') {
                        if (!leaf.link.startsWith("http")) {
                            leaf.link = "http://" + leaf.link;
                        }
                        var externalLink = menuService.parseExternalLink(leaf);
                        path += '\" target="_blank" href="{0}"'.format(externalLink);
                    }

                    path += '(\'' + leaf.title + '\')">';
                }

                //build the menu item
                path += '<i class="' + leaf.icon + '"></i>&ensp;' + leaf.title;
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

var seperator = '<span class="part seperator">/</span>';

