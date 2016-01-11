(function (angular) {
    "use strict";

var app = angular.module('sw_layout');

app.directive('breadcrumb', function (contextService, $log, $timeout, recursionHelper, crudContextHolderService, i18NService) {
    "ngInject";

    var log = $log.getInstance('sw4.breadcrumb');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/breadcrumb.html'),
        scope: {
            menu: '=',
            title: '='
        },
        controller: function ($scope) {
            $scope.convertAdminHTMLtoLeafs = function (kids) {
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
                                        for (var i = 0; i < icon.classList.length; i++) {
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
                                        click = click.replace('$event', 'null');
                                        click = click.replace('myProfile', '$scope.adminMyProfile');
                                        click = click.replace('loadApplication', '$scope.adminLoadApplication');
                                        click = click.replace('logout', '$scope.adminLogout');

                                        newObject.click = click;
                                        newObject.type = 'AdminMenuItemDefinition';
                                    }

                                    if (kids[idx].children != null && kids[idx].children.length > 0) {
                                        var childLeafs = $scope.convertAdminHTMLtoLeafs(kids[idx].children);

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
                                    leafs = $scope.convertAdminHTMLtoLeafs(kids[idx].children);
                                }

                                break;
                            }
                        }
                   }

                return leafs;
            }

            $scope.findCurrentPage = function (leafs, current) {
                var page = null;

                if (current != null) {
                    for (var id in leafs) {
                        if (leafs[id].hasOwnProperty('title')) {
                            var childPage = $scope.findCurrentPage(leafs[id].leafs, current);

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

                            var schema = crudContextHolderService.currentSchema();
                            if (schema != null) {
                                //if the current leaf matches the current application
                                var isParent = leafs[id].applicationContainer == schema.applicationName;

                                //if the lcurrent leaf is likely the parent
                                var possibleParent = leafs[id].application == schema.applicationName;
                                possibleParent = possibleParent && leafs[id].schema.toLowerCase().indexOf('list') > -1;
                                possibleParent = possibleParent && leafs[id].title != schema.title;

                                if ((isParent || possibleParent) && childPage == null) {
                                    //add to the breadcrumb
                                    if (page == null) {
                                        page = [];
                                    }

                                    page.push(leafs[id]);

                                    if (!$scope.pageFoundInMenu(current)) {

                                        //add a breadcrumb item for the unknown page
                                        var newPage = {};
                                        newPage.title = $scope.getBreadcrumbTitle(current);
                                        newPage.icon = $scope.getBreadcrumbIcon(current);
                                        newPage.type = 'UnknownMenuItemDefinition';
                                        page.push(newPage);
                                    }
                                }
                            }
                        }
                    }
                } else {
                    //if the current title is null, use the first menu leaf as the current
                    page = [];
                    page.push(leafs[0]);
                }

                return page;
            }

            $scope.getBreadcrumbIcon = function (title) {
                var icon = 'fa fa-circle-o';

                if (title.indexOf("Detail") > -1) {
                    icon = 'fa fa-file-text-o';
                }

                return icon;
            }

            $scope.getBreadcrumbItems = function (currentMenu) {
                var foundPages = $scope.findCurrentPage(currentMenu.leafs, $scope.currentTitle);
                var newPage;

                //add the settings menu
                if (currentMenu.displacement == 'admin') {
                    newPage = {};
                    newPage.icon = 'fa fa-cog';
                    newPage.title = 'Settings';
                    newPage.leafs = currentMenu.leafs;
                    newPage.type = 'MenuContainerDefinition';

                    if (foundPages != null) {
                        foundPages.unshift(newPage);
                    } else {
                        foundPages = [];
                        foundPages.push(newPage);
                    }
                }

                //add the hamburger menu
                newPage = {};
                newPage.icon = 'fa fa-bars';
                newPage.title = 'Hamburger';
                newPage.leafs = $scope.menu.leafs;
                newPage.type = 'MenuContainerDefinition';

                if (foundPages != null) {
                    foundPages.unshift(newPage);
                } else {
                    foundPages = [];
                    foundPages.push(newPage);

                    //if no menu items where found, add the current schema to the breadcrumb
                    var current = crudContextHolderService.currentSchema();
                    if (current != null) {
                        newPage = {};
                        newPage.title = $scope.getBreadcrumbTitle(current.title);
                        newPage.icon = $scope.getBreadcrumbIcon(current.title);
                        newPage.type = 'UnknownMenuItemDefinition';
                        foundPages.push(newPage);
                    }
                }
  
                return foundPages;
            };

            $scope.getBreadcrumbTitle = function (title) {
                var string = title;

                if (title.indexOf("Detail") > -1) {
                    var record = i18NService.getI18nRecordLabel(crudContextHolderService.currentSchema(), crudContextHolderService.rootDataMap());
                    if (record) {
                        string = record;
                    }
                }

                return string;
            }

            $scope.getCurrentMenu = function () {
                var currentItem = $('.admin-area .admin-menu .modern .dropdown-menu a:contains("' + $scope.title + '")');
                var menu = {};

                if (currentItem.hasOwnProperty(length)) {
                    var mainMenu = $('.admin-area .admin-menu .modern > .dropdown-menu');
                    var leafs = $scope.convertAdminHTMLtoLeafs(mainMenu[0].children);
                    menu.displacement = 'admin';
                    menu.leafs = leafs;
                } else {
                    menu = $scope.menu;
                }

                return menu;
            };

            $scope.getCurrentTitle = function () {
                var menu = $scope.getCurrentMenu();
                var schema = crudContextHolderService.currentSchema();

                $scope.currentTitle = $scope.title;

                if (menu.displacement != 'admin') {
                    if (schema != null) {
                        $scope.currentTitle = schema.title;
                    }
                }
            };

            $scope.isDesktop = function () {
                return isDesktop();
            };

            $scope.isMobile = function () {
                return isMobile();
            };

            $scope.pageFoundInMenu = function (title) {
                var pageFound = false;
                var menu = $scope.getCurrentMenu();

                if (menu.explodedLeafs != null) {
                    for (var id in menu.explodedLeafs) {
                        if (menu.explodedLeafs[id].hasOwnProperty('title')) {
                            if (menu.explodedLeafs[id].title == title) {
                                pageFound = true;
                            }
                        }
                    }
                }

                return pageFound;
            }

            $scope.processBreadcrumb = function () {
                var currentMenu = $scope.getCurrentMenu();
                var breadcrumbItems = $scope.getBreadcrumbItems(currentMenu);

                log.debug('$scope', $scope);
                log.debug('schema', crudContextHolderService.currentSchema());
                //log.debug('datamap', crudContextHolderService.rootDataMap());
                //log.debug('currentMenu', currentMenu);
                log.debug('breadcrumbItems', breadcrumbItems);

                $scope.breadcrumbItems = breadcrumbItems;
            };

            $scope.toggleOpen = function (event) {
                $('.hamburger').toggleClass('open');
            };

            $scope.$watch('title', function (newValue, oldValue) {
                //log.debug('titleChange');

                $scope.getCurrentTitle();
                $scope.processBreadcrumb();
            });
        }
    };
});

app.directive('bcMenuDropdown', function ($log, contextService, recursionHelper) {
    "ngInject";

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/breadcrumbDropdown.html'),
        scope: {
            leafs: '='
        },
        controller: function ($scope) {
            $scope.isDesktop = function () {
                return isDesktop();
            };

            $scope.isMobile = function () {
                return isMobile();
            };

            $scope.toggleOpen = function (event) {
                $(event.target).next().toggleClass('open');
            };
        },
        compile: function (element) {
            return recursionHelper.compile(element, function (scope, iElement, iAttrs, controller, transcludeFn) {
                // Define your normal link function here.
                // Alternative: instead of passing a function,
                // you can also pass an object with 
                // a 'pre'- and 'post'-link function.
            });
        }
    };
});

app.directive('bcMenuItem', function ($log, menuService, adminMenuService) {
    "ngInject";

    return {
        controller: function ($scope, alertService, validationService, crudContextHolderService) {
            $scope.goToApplication = function (leaf) {
                var msg = "Are you sure you want to leave the page?";
                if (crudContextHolderService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        menuService.goToApplication(leaf, null);
                        $scope.$digest();
                    }, msg, function () { return; });
                } else {
                    menuService.goToApplication(leaf, null);
                }

                $scope.closeBreadcrumbs();
            };

            $scope.doAction = function (leaf) {
                //update title when switching to dashboard
                $scope.$emit('sw_titlechanged', null);

                var msg = "Are you sure you want to leave the page?";
                if (crudContextHolderService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        menuService.doAction(leaf, null);
                        $scope.$digest();
                    }, msg, function () { return; });
                } else {
                    menuService.doAction(leaf, null);
                }

                crudContextHolderService.clearCrudContext();
                $scope.closeBreadcrumbs();
            };

            $scope.adminEval = function (click) {
                eval(click);
            };

            $scope.adminDoAction = function (title, controller, action, parameters, $event) {
                adminMenuService.doAction(title, controller, action, parameters, $event.target);
                $scope.closeBreadcrumbs();
            };

            $scope.adminLoadApplication = function (applicationName, schemaId, mode, id) {
                adminMenuService.loadApplication(applicationName, schemaId, mode, id);
                $scope.closeBreadcrumbs();
            };

            $scope.adminLogout = function () {
                adminMenuService.logout();
                $scope.closeBreadcrumbs();
            };

            $scope.adminMyProfile = function () {
                adminMenuService.myProfile();
                $scope.closeBreadcrumbs();
            };

            $scope.closeBreadcrumbs = function () {
                $('.breadcrumb .open').removeClass('open');
            };
        }
    };
});

})(angular);