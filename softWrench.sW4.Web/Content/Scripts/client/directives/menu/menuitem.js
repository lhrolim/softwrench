(function (app) {
    "use strict";

    app.directive('menuWrapper', function ($compile) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            template: "<div></div>",
            scope: {
                menu: '=',
                popupmode: '@'
            },
            link: function (scope, element, attrs) {
                if (scope.popupmode == 'none') {
                    element.append(
                      "<menu menu='menu'/>"
                  );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('menu', function (contextService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/menu/menu.html'),
            scope: {
                menu: '='
            },
            controller: function ($scope, $rootScope, $timeout) {

                $scope.level = -1;

                $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {

                    if ($scope.menu.displacement == 'vertical') {

                        $('.dropdown-container').on({
                            "shown.bs.dropdown": function (event) {
                                if ($(this)[0] === event.target) {
                                    $(this).data('closable', false);
                                }
                            },
                            "click": function (event) {
                                if ($(this).children()[0] === event.target) {
                                    $(this).data('closable', true);
                                }

                                //reinit scrollpane after menu item opens/closes
                                $timeout(function () {
                                    var api = $('.vertical-menu .menu-primary').jScrollPane({ maintainPosition: true }).data('jsp');
                                    api.reinitialise();
                                }, 250);
                            },
                            "hide.bs.dropdown": function (event) {
                                if ($(this)[0] === event.target) {
                                    return $(this).data('closable');
                                }
                            }
                        });

                        // workaround to expand all sub-menus for gric/scottsdale/manchester in horizontal menu
                        if ($rootScope.clientName == 'gric' || $rootScope.clientName == 'scottsdale' || $rootScope.clientName == 'manchester') {
                            $('.dropdown-container').find("span").toggleClass("right-caret bottom-caret");
                            $('.dropdown-container').addClass("open");
                            $('.dropdown-container').data('closable', false);
                        }

                        // To scroll the Sidebar along with the window
                        if ($rootScope.clientName == 'hapag') {

                            $(window).scroll(function (event) {
                                var scroll = $(event.target).scrollTop();
                                $("#sidebar").scrollTop(scroll);
                            });

                            $('.dropdown-container').on(
                            {
                                "shown.bs.dropdown": function (event) {
                                    var windowHeight = $(window).height();
                                    var sidebarHeigth = $('.alignment-logo').outerHeight() + $('[role=menu]').outerHeight();

                                    if (sidebarHeigth > windowHeight) {
                                        $('.col-main-content').height(sidebarHeigth);
                                    }
                                },
                                "hide.bs.dropdown": function (event) {
                                    var windowHeight = $(window).height();
                                    var sidebarHeigth = $('.alignment-logo').outerHeight() + $('[role=menu]').outerHeight();

                                    if (sidebarHeigth < windowHeight) {
                                        $('.col-main-content').height(sidebarHeigth);
                                    }
                                }

                            });

                        }
                    }
                });
            }
        };
    });

    app.directive('subMenu', function ($compile) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            template: "<ul class='dropdown-menu submenu'></ul>",
            scope: {
                leaf: '=',
                displacement: '=',
                level: '='
            },
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.leaf.leafs)) {
                    element.append(
                        "<menu-item leaf='leaf' displacement='displacement' level='level' ng-repeat='leaf in leaf.leafs'></menu-item>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('menuItem', function (contextService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('Content/Templates/menu/menuitem.html'),
            scope: {
                leaf: '=',
                displacement: '=',
                level: '='
            },
            controller: function ($scope, $http, $rootScope, menuService, i18NService, mockService, alertService, validationService, crudContextHolderService, dispatcherService) {

                $scope.level = $scope.level + 1;

                $scope.i18N = function (key, defaultValue, paramArray) {
                    return i18NService.get18nValue(key, defaultValue, paramArray);
                };

                $scope.isMenuContainer = function (level, leaf) {
                    return leaf.type == 'MenuContainerDefinition';
                }

                $scope.isButtonStyleContainer = function (level, leaf) {
                    if (leaf.module != null) {
                        return false;
                    }
                    return level == 0;
                }

                $scope.i18NMenuLabel = function (menuItem, tooltip) {
                    return menuService.getI18nMenuLabel(menuItem, tooltip);
                };

                $scope.isMenuItemIcon = function (menuItem) {
                    //return menuItem.icon;
                    return menuService.getI18nMenuIcon(menuItem);

                };

                $scope.goToApplication = function (leaf, $event) {
                    var target = $event.target;
                    var msg = "Are you sure you want to leave the page?";
                    if (crudContextHolderService.getDirty()) {
                        alertService.confirmCancel(msg).then(function () {
                            menuService.goToApplication(leaf, target);
                            crudContextHolderService.clearDirty();
                            crudContextHolderService.clearDetailDataResolved();
                            $scope.$digest();
                        }).catch(err => console.log(err));
                    }
                    else {
                        menuService.goToApplication(leaf, target);
                    }
                };

                $scope.dispatch = function (leaf) {
                    dispatcherService.invokeService(leaf.service, leaf.method);
                }

                $scope.doAction = function (leaf, $event) {
                    var target = $event.target;
                    //update title when switching to dashboard
                    $scope.$emit(JavascriptEventConstants.TitleChanged, null);

                    var msg = "Are you sure you want to leave the page?";
                    if (crudContextHolderService.getDirty()) {
                        alertService.confirmCancel(msg).then(function () {
                            menuService.doAction(leaf, target);
                            crudContextHolderService.clearDirty();
                            crudContextHolderService.clearDetailDataResolved();
                            $scope.$digest();
                        });
                    }
                    else {
                        menuService.doAction(leaf, target);
                    }
                };

                $scope.contextPath = function (path) {
                    return url(path);
                };

                $scope.search = function (module, application, searchFields, id, schema) {
                    var searchText = $('#' + id).val();
                    if (nullOrUndef(schema)) {
                        schema = "list";
                    }
                    contextService.insertIntoContext('currentmodule', module);
                    if (searchText != null && searchText != '') {
                        searchText = '%' + searchText + '%';
                        var params = $.param({ 'application': application, 'searchFields': searchFields, 'searchText': searchText, 'schema': schema });
                        return $http.get(url("/api/generic/Data/Search" + "?" + params)).then(function (response) {
                            const data = response.data;
                            $rootScope.$broadcast(JavascriptEventConstants.ActionAfterRedirection, data);
                        }
                        );
                    }
                };

                $scope.getDataToggle = function (container) {
                    return container.hasMainAction ? "dropdown" : "null";
                };

                $scope.handleContainerClick = function (container, $event) {
                    var target = $event.target;
                    if (container.controller != null && !mockService.isMockedContainerDashBoard()) {
                        if (menuService.isSelectedLeaf(container)) {
                            return;
                        }


                        var msg = "Are you sure you want to leave the page?";
                        if (crudContextHolderService.getDirty()) {
                            alertService.confirmCancel(msg).then(function () {
                                menuService.doAction(container, target);
                                crudContextHolderService.clearDirty();
                                crudContextHolderService.clearDetailDataResolved();
                                $scope.$digest();
                            });
                        }
                        else {
                            menuService.doAction(container, target);
                        }
                    }
                    if ($scope.displacement == 'vertical') {
                        $(target).find("span").toggleClass("right-caret bottom-caret");
                    }


                };
            }
        };
    });

})(app);