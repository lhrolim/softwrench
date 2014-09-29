app.directive('menu', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/menu.html'),
        scope: {
            menu: '='
        },
        controller: function ($scope, $rootScope) {

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
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('Content/Templates/menuitem.html'),
        scope: {
            leaf: '=',
            displacement: '=',
            level: '='
        },
        controller: function ($scope, $http, $rootScope, menuService, i18NService, mockService) {

            $scope.level = $scope.level + 1;

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.isButtonStyleContainer = function (level, leaf) {
                if (leaf.module != null) {
                    return false;
                }
                return level == 0;
            }

            $scope.i18NMenuLabel = function (menuItem, tooltip) {
                return menuService.getI18nMenuLabel(menuItem, tooltip);
            };

            $scope.isMenuItemTitle = function (menuItem, tooltip) {
                return menuItem.icon;
            };
                
            //    var menuTitle = menuService.getI18nMenuLabel(menuItem, tooltip);
            //    if (menuTitle == "Request Tasks") {
                  
            //      return "fa fa-paper-plane-o";
            //    }
            //    if (menuTitle == "Work Order Tasks") {
            //        return "fa fa-gavel";
                               
            //    }
            //     if (menuTitle == "Change Grid") {
            //          return "fa fa-refresh";
            //    }
            //     if (menuTitle == "Purchase Tasks") {
            //          return "fa fa-plus-square";
            //    }
            //     if (menuTitle == "Incident Tasks") {
            //           return "fa fa-wrench";
            //    }
             
            //   };


            $scope.goToApplication = function (leaf, target) {
                menuService.goToApplication(leaf, target);
            };

            $scope.doAction = function (leaf, target) {
                menuService.doAction(leaf, target);
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
                    $http.get(url("/api/generic/Data/Search" + "?" + params)).success(
                        function (data) {
                            $rootScope.$broadcast("sw_redirectactionsuccess", data);
                        }
                    );
                }
            };


            $scope.handleContainerClick = function (container, target) {
                if (container.controller != null && !$(target).find("span").hasClass('bottom-caret') && !mockService.isMockedContainerDashBoard()) {
                    menuService.doAction(container, target);
                }
                if ($scope.displacement == 'vertical') {
                    $(target).find("span").toggleClass("right-caret bottom-caret");
                }

                
            };
        }
    };
});