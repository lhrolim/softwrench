(function (angular) {
    "use strict";



    angular.module("sw_layout").directive("autocompleteClientInput", ["cmplookup", "contextService", 'expressionService', 'cmpfacade',
        'dispatcherService', 'modalService', 'compositionCommons', 'i18NService',
        function (cmplookup, contextService, expressionService, cmpfacade, dispatcherService, modalService, compositionCommons, i18NService) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl('/Content/modules/components/autocompleteclient/templates/autocompleteClientInput.html'),
                scope: {
                    datamap: '=',
                    parentdata: '=',
                    schema: '=',
                    fieldMetadata: '=',
                    displayablepath: '@',
                    mode: '@'
                },



                link: function (scope, element, attrs) {
                    scope.$name = "autocompleteClientInput";
                },

                controller: ["$scope", "$injector", "i18NService", "fieldService", "formatService", "layoutservice", "expressionService",
                    function ($scope, $injector, i18NService, fieldService, formatService, layoutservice, expressionService) {
                    function init() {
                        $injector.invoke(BaseController, this, {
                            $scope: $scope,
                            i18NService: i18NService,
                            fieldService: fieldService,
                            formatService: formatService,
                            layoutservice: layoutservice,
                            expressionService: expressionService
                        });
                    }
                    init();
                }]

            };

            return directive;

        }]);

})(angular);