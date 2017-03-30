(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    app.directive('breadcrumb', function (contextService, $log, $timeout, $rootScope, recursionHelper, crudContextHolderService, i18NService, breadcrumbService) {
        "ngInject";

        var log = $log.getInstance('breadcrumb');

        return {
            templateUrl: contextService.getResourceUrl('/Content/Templates/breadcrumb.html'),
            scope: {
                menu: '=',
                title: '='
            },
            controller: function ($scope) {
                //TODO: improve for dual menus

                $scope.isDesktop = function () {
                    return isDesktop();
                };

                $scope.isMobile = function () {
                    return isMobile();
                };

                $scope.processBreadcrumb = function () {
                    var breadcrumbItems = breadcrumbService.getBreadcrumbItems($scope.title);
                    $scope.breadcrumbItems = breadcrumbItems;

                    log.debug('breadcrumbItems', $scope.title, breadcrumbItems);
                };

                $scope.toggleOpen = function (event) {
                    $('.hamburger').toggleClass('open');
                };

                $scope.$watch("title", function (newValue, oldValue) {
                    $scope.processBreadcrumb();
                });

                $scope.$on("sw.breadcrumb.history.redirect.sametitle", function () {
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

    app.directive('bcMenuItem', function ($log, menuService, adminMenuService, checkpointService, dispatcherService) {
        "ngInject";

        return {
            controller: function ($scope, alertService, validationService, crudContextHolderService, historyService) {
                $scope.goToApplication = function (leaf, event, fromDropdownOrMenu) {
                    var msg = "Are you sure you want to leave the page?";

                    var parameters = {};
                    if (!fromDropdownOrMenu) {
                        var checkPointData = checkpointService.fetchCheckpoint(leaf.application + "." + leaf.schema);
                        if (checkPointData) {
                            parameters["SearchDTO"] = checkPointData.listContext;
                        }
                    }

                    if (crudContextHolderService.getDirty()) {
                        alertService.confirmCancel(msg).then(function () {
                            menuService.goToApplication(leaf, null, parameters);
                            crudContextHolderService.clearDirty();
                            crudContextHolderService.clearDetailDataResolved();
                            $scope.$digest();
                        });
                    } else {
                        menuService.goToApplication(leaf, null, parameters);
                    }

                    $scope.closeBreadcrumbs();
                };

                $scope.doAction = function (leaf) {
                    //update title when switching to dashboard
                    $scope.$emit(JavascriptEventConstants.TitleChanged, null);

                    var msg = "Are you sure you want to leave the page?";
                    if (crudContextHolderService.getDirty()) {
                        alertService.confirmCancel(msg).then(function () {
                            menuService.doAction(leaf, null);
                            crudContextHolderService.clearDirty();
                            crudContextHolderService.clearDetailDataResolved();
                            $scope.$digest();
                        });
                    } else {
                        menuService.doAction(leaf, null);
                    }

                    crudContextHolderService.clearCrudContext();
                    $scope.closeBreadcrumbs();
                };

                $scope.dispatch = function(leaf) {
                    dispatcherService.invokeService(leaf.service, leaf.method);
                }

                $scope.redirectIfNeeded = function (leaf) {
                    if (leaf.redirectURL) {
                        historyService.breadcrumbRedirect(leaf.redirectURL, leaf.historyIndex);
                    }
                }

                $scope.adminEval = function (click) {
                    eval(click);
                };

                $scope.adminDoAction = function (title, controller, action, parameters, $event) {
                    adminMenuService.doAction(title, controller, action, parameters, $event ? $event.target : null);
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