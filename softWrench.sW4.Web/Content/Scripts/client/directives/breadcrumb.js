﻿var app = angular.module('sw_layout');

app.directive('breadcrumb', function ($rootScope, $log, $compile, menuService) {
    var log = $log.getInstance('sw4.breadcrumb');

    return {
        scope: {
            schema: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {
            scope.$watch('title', function (newValue, oldValue) {
                //log.debug(scope.menu);

                var template;
                var currentItem = $('.admin-area .admin-menu .dropdown-menu a:contains("' + scope.title + '")');

                //if we are in the admin menu
                if (currentItem.hasOwnProperty(length)) {
                    template = getBreadCrumbHTML(log, scope.menu, undefined, menuService);
                    template += seperator;

                    var mainItem = $('.admin-area .admin-menu > a');

                    //build the settings item
                    if (mainItem.hasOwnProperty(length)) {
                        template += '<div class="part">';
                        template += '<a data-toggle="dropdown" aria-expanded="false">';
                        template += mainItem[0].firstElementChild.outerHTML;
                        template += '&ensp;';
                        template += 'Settings';
                        template += '&ensp;<i class="fa fa-caret-down"></i>';
                        template += '</a>';
                    }

                    //get the admin menu items
                    var mainMenu = $('.admin-area .admin-menu > .dropdown-menu');
                    var leafs = getAdminLeafs(log, mainMenu[0].children);

                    var adminMenu = {};
                    adminMenu.leafs = leafs;
                    adminMenu.tpye = 'admin';

                    //append the dropdown menu
                    template += getChildMenu(log, adminMenu.leafs, null, menuService);
                    template += '</div>';

                    //append child parts
                    var foundPath = findCurrentPage(log, adminMenu.leafs, scope.title, adminMenu);
                    if (foundPath) {
                        template += seperator;
                        template += foundPath;
                    } 
                } else {
                    template = getBreadCrumbHTML(log, scope.menu, scope.title, menuService);
                }
               
                if (template != null) {
                    var content = $compile(template)(scope);
                    element.html(content);
                }
            });
        }
    }
});

app.directive('bcMenuItem', function ($rootScope, $log, $compile, menuService, adminMenuService) {
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

            $scope.adminDoAction = function (title, controller, action, parameters, target) {
                adminMenuService.doAction(title, controller, action, parameters, target);
            };

            $scope.adminLoadApplication = function (applicationName, schemaId, mode, id) {
                adminMenuService.loadApplication(applicationName, schemaId, mode, id);
            };

            $scope.adminLogout = function () {
                adminMenuService.logout();
            };

            $scope.adminMyProfile = function () {
                adminMenuService.myProfile();
            };
        }
    }
});

function getAdminLeafs(log, kids) {
    var leafs = [];

    for (var idx = 0; idx < kids.length; idx++) {
        if (kids[idx].localName != undefined) {
            var iconClass = '';

            switch (kids[idx].localName) {
                case 'li':
                    var link = kids[idx].firstElementChild;
                    var icon = link.firstElementChild;
                    var title = link.innerText.trim();

                    if (icon != null) {
                        if (icon.className != undefined) {
                            for (i = 0; i < icon.classList.length; i++) {
                                if (icon.classList[i] != 'fa-fw') {
                                    iconClass += icon.classList[i] + ' ';
                                }
                            }
                        }
                    }

                    if (kids[idx].className != 'user') {
                        var newObject = {};
                        newObject.icon = iconClass.trim();
                        newObject.title = title;

                        if (link.attributes['ng-click']) {
                            var click = link.attributes['ng-click'].nodeValue;

                            //rename the admin function, to avoid confilts
                            click = click.replace('doAction', 'adminDoAction');
                            click = click.replace('myProfile', 'adminMyProfile');
                            click = click.replace('loadApplication', 'adminLoadApplication');
                            click = click.replace('logout', 'adminLogout');

                            newObject.click = click;
                        }

                        if (kids[idx].children != null && kids[idx].children.length > 0) {
                            var childLeafs = getAdminLeafs(log, kids[idx].children);

                            if (childLeafs.length > 0) {
                                newObject.leafs = childLeafs;
                            }
                        }

                        leafs.push(newObject);
                    }

                    break;

                case 'ul':
                    if (kids[idx].children != null && kids[idx].children.length > 0) {
                        leafs = getAdminLeafs(log, kids[idx].children);
                    }

                    break;
                }
            }
       }

    return leafs;
}

function getBreadCrumbHTML(log, menu, current, menuService) {
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

function getChildMenu(log, leafs, parent, menuService) {
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

                    if (leaf.click) {
                        path += leaf.click;
                        path += '">';
                    } else {
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
                }

                //build the menu item
                path += '<i class="' + leaf.icon + ' fa-fw"></i>&ensp;' + leaf.title.trim();
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
                    } else if (leafs[id].type == 'ExternalLinkMenuItemDefinition') {
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

var seperator = '<span class="part seperator">/</span>';
