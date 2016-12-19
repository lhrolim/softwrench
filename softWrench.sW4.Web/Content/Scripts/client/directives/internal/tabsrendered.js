(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .directive('tabsrendered', ["$timeout","$q","$log","$rootScope","eventService","schemaService","redirectService","spinService",
            function ($timeout, $q, $log, $rootScope, eventService, schemaService, redirectService, spinService) {
        

        /// <summary>
        /// This directive allows for a hookup method when all the tabs of the crud_body have finished rendered successfully.
        /// 
        /// Since the tabs are lazy loaded, we will replace default bootstrap behaviour of tab-toggle to use a custom engine that will dispatch an event, listened by all possible 
        /// tab implementations (compositionlist.js, crud_output.js and crud_input.js)
        /// 
        /// </summary>
        /// <param name="$timeout"></param>
        /// <param name="$log"></param>
        /// <param name="$rootScope"></param>
        /// <returns type=""></returns>
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                // Do not execute until the last iteration of ng-repeat has been reached,
                // or if $last is undefined (this happens when the tabsrendered directive 
                // is placed on something other than ng-repeat).
                if (scope.$last === false) {
                    return $q.when();
                }
                var log = $log.getInstance('tabsrendered');
                log.debug("finished rendering tabs of detail screen");
                if (scope.$last === undefined) {
                    //0 tabs scenario
                    return $rootScope.$broadcast(JavascriptEventConstants.TabsLoaded, null, scope.panelid);
                }

                // covers a redirect for same application and schema but to another entry
                $rootScope.$on(JavascriptEventConstants.ApplicationRedirected, function (event, applicationName, renderedSchema) {
                    scope.schema = renderedSchema;
                    $rootScope.$broadcast(JavascriptEventConstants.TabsLoaded, null, scope.panelid);
                });

                $timeout(function () {
                    var firstTabId = null;
                    $('.compositiondetailtab li>a').each(function () {
                        var $this = $(this);
                        if (firstTabId == null) {
                            firstTabId = $(this).data('tabid');
                        }
                        $this.click(function (e) {
                            e.preventDefault();
                            $this.tab('show');
                            const tabId = $(this).data('tabid');
                            log.trace('lazy loading tab {0}'.format(tabId));
                            spinService.stop({ compositionSpin: true });
                            $rootScope.$broadcast('sw_lazyloadtab', tabId);

                        });

                    });
                    $rootScope.$broadcast(JavascriptEventConstants.TabsLoaded, firstTabId, scope.panelid);

                }, 0, false);

            }
        };
    }]);

})(angular);