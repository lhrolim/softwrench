var app = angular.module('sw_layout');

app.directive('breadcrumb', function (contextService, $log, recursionHelper) {
    var log = $log.getInstance('sw4.breadcrumb');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/breadcrumb.html'),
        scope: {
            schema: '=',
            menu: '=',
            title: '='
        },
        controller: function ($scope) {
            $scope.convertAdminHTMLtoLeafs = function (log, kids) {
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
                                        click = click.replace('doAction', '$scope.adminDoAction');
                                        click = click.replace('$event.target', 'null');
                                        click = click.replace('myProfile', '$scope.adminMyProfile');
                                        click = click.replace('loadApplication', '$scope.adminLoadApplication');
                                        click = click.replace('logout', '$scope.adminLogout');

                                        newObject.click = click;
                                        newObject.type = 'AdminMenuItemDefinition';
                                    }

                                    if (kids[idx].children != null && kids[idx].children.length > 0) {
                                        var childLeafs = $scope.convertAdminHTMLtoLeafs(log, kids[idx].children);

                                        if (childLeafs.length > 0) {
                                            newObject.leafs = childLeafs;
                                            newObject.type = 'MenuContainerDefinition';
                                        }
                                    }

                                    leafs.push(newObject);
                                }

                                break;

                            case 'ul':
                                if (kids[idx].children != null && kids[idx].children.length > 0) {
                                    leafs = $scope.convertAdminHTMLtoLeafs(log, kids[idx].children);
                                }

                                break;
                            }
                        }
                   }

                return leafs;
            }

            $scope.findCurrentPage = function (log, leafs, current) {
                var page = null;

                for (var id in leafs) {
                    if (leafs[id].hasOwnProperty('title')) {
                        var childPage = $scope.findCurrentPage(log, leafs[id].leafs, current);

                        //add page if current or decentant is the current page
                        if (childPage != null || leafs[id].title == current) {
                            if (page == null) {
                                page = [];
                            }

                            page.push(leafs[id]);
                        }

                        //if decentants were found, add to the return
                        if (childPage != null) {
                            for (var x in childPage) {
                                if (childPage[x].hasOwnProperty('title')) {
                                    page.push(childPage[x]);
                                }
                            }
                        }
                    }
                }

                return page;
            }

            $scope.getBreadcrumbItems = function (currentMenu) {
                var foundPages = $scope.findCurrentPage(log, currentMenu.leafs, $scope.title);

                if (foundPages != null) {
                    var newPage;

                    //add the settings menu
                    if (currentMenu.displacement == 'admin') {
                        newPage = {};
                        newPage.icon = 'fa fa-cog';
                        newPage.title = 'Settings';
                        newPage.leafs = currentMenu.leafs;
                        newPage.type = 'MenuContainerDefinition';

                        foundPages.unshift(newPage);
                    }

                    //add the hamburger menu
                    newPage = {};
                    newPage.icon = 'fa fa-bars';
                    newPage.title = 'Hamburger';
                    newPage.leafs = $scope.menu.leafs;
                    newPage.type = 'MenuContainerDefinition';

                    foundPages.unshift(newPage);
                }
  
                return foundPages;
            };

            $scope.getCurrentMenu = function () {
                var currentItem = $('.admin-area .admin-menu .dropdown-menu a:contains("' + $scope.title + '")');
                var menu = {};

                if (currentItem.hasOwnProperty(length)) {
                    var mainMenu = $('.admin-area .admin-menu > .dropdown-menu');
                    var leafs = $scope.convertAdminHTMLtoLeafs(log, mainMenu[0].children);
                    menu.displacement = 'admin';
                    menu.leafs = leafs;
                } else {
                    menu = $scope.menu;
                }

                return menu;
            };

            $scope.$watch('title', function (newValue, oldValue) {
                var currentMenu = $scope.getCurrentMenu();
                var breadcrumbItems = $scope.getBreadcrumbItems(currentMenu);

                log.debug('breadcrumbItems', $scope, currentMenu, breadcrumbItems);
                $scope.breadcrumbItems = breadcrumbItems;
            });
        }
    }
});

app.directive('bcMenuItem', function ($log, menuService, adminMenuService) {
    //var log = $log.getInstance('sw4.breadcrumb Menu Item');

    return {
        controller: function ($scope, alertService, validationService) {

            $scope.goToApplication = function (leaf) {
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

            $scope.doAction = function (leaf) {
                //update title when switching to dashboard
                $scope.$emit('sw_titlechanged', null);

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

            $scope.adminEval = function (click) {
                eval(click);
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

app.directive('bcMenuDropdown', function ($log, contextService, recursionHelper) {
    //var log = $log.getInstance('sw4.breadcrumb Dropdown');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/breadcrumbDropdown.html'),
        scope: {
            leafs: '='
        },
        compile: function (element) {
            return recursionHelper.compile(element, function (scope, iElement, iAttrs, controller, transcludeFn) {
                // Define your normal link function here.
                // Alternative: instead of passing a function,
                // you can also pass an object with 
                // a 'pre'- and 'post'-link function.
            });
        }
    }
});