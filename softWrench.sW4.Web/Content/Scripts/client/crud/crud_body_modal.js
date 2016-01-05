var app = angular.module('sw_layout');

app.directive('crudBodyModalWrapper', function ($compile) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div data-id='crud-modal-wrapper'></div>",

        link: function (scope, element, attrs) {
            if (scope.modalincluded) {
                element.append(
                    "<crud-body-modal " +
                    "ismodal='true' schema='schema' " +
                    "datamap='datamap' " +
                    "savefn='save(selecteditem)'" +
                    "is-dirty='false' " +
                    "original-datamap='OriginalDatamp' " +
                    "association-schemas='associationSchemas' " +
                    "blockedassociations='blockedassociations' " +
                    "paginationdata='paginationData' " +
                    "search-data='searchData' " +
                    "search-operator='searchOperator' " +
                    "search-sort='searchSort' > </crud-body-modal> "
                );
                $compile(element.contents())(scope);
            }


        },

        controller: function ($injector, $scope, $rootScope, fieldService, $element) {
            $scope.$name = "crudbodymodalwrapper";






            function doInit() {


            }

            doInit();

        }
    }

});



(function () {
    'use strict';



    function crudBodyModal($rootScope, modalService, crudContextHolderService) {

        var controller = function ($scope, $http, $filter, $injector,
         formatService, fixHeaderService,
         searchService, tabsService,
         fieldService, commandService, i18NService,
         submitService, redirectService,
         associationService) {

            $scope.$name = "crudbodymodal";
            $scope.save = function (selecteditem) {
                $scope.savefn({ selecteditem: selecteditem });
            }

            $scope.$on('sw.modal.hide', function (event) {
                $scope.closeModal();
            });

            $scope.closeModal = function () {
                $scope.modalshown = false;
                $('#crudmodal').modal('hide');
                $rootScope.showingModal = false;

                $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
                $('.no-touch [rel=tooltip]').tooltip('hide');
                crudContextHolderService.disposeModal();
            }

            $scope.cancel = function () {
                $scope.closeModal();
                if ($scope.cancelfn) {
                    $scope.cancelfn();
                }
            }

            $scope.$on('sw.modal.show', function (event, modaldata) {
                $scope.showModal(modaldata);
                $scope.modalshown = true;
            });

            $scope.showModal = function (modaldata) {
                var schema = modaldata.schema;
                var datamap = modaldata.datamap;
                $scope.schema = schema;
                $scope.originalsavefn = modaldata.savefn;
                $scope.cancelfn = modaldata.cancelfn;
                $scope.previousschema = modaldata.previousschema;
                $scope.previousdata = modaldata.previousdata;
                $scope.modaltitle = modaldata.title;
                $scope.cssclass = modaldata.cssclass;
                //by default modals, should render as detail stereotype mode
                schema.stereotype = schema.stereotype || "detail";
                $scope.isList = schema.stereotype.equalsAny("list", "compositionlist");
                $scope.isDetail = schema.stereotype.equalsAny("detail", "compositionDetail","detailnew");


                $scope.datamap = {
                    fields: datamap
                };
                var datamapToUse = $.isEmptyObject(datamap) ? $scope.previousdata : datamap;
                $scope.originalDatamap = angular.copy(datamapToUse);
                fieldService.fillDefaultValues(schema.displayables, datamap);
                $('#crudmodal').modal('show');
                $("#crudmodal").draggable();
                $rootScope.showingModal = true;
                //TODO: review this decision here it might not be suitable for all the scenarios
                crudContextHolderService.modalLoaded();

                associationService.loadSchemaAssociations(datamapToUse, schema).then(function () {
                    if (modaldata.onloadfn) {
                        modaldata.onloadfn($scope);
                    }
                });

            }


            $scope.save = function (selecteditem) {
                $scope.savefn({ selecteditem: selecteditem });
            }

            function doInit() {


                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService,
                    formatService: formatService
                });
            }

            doInit();


        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: url('/Content/Templates/crud/crud_body_modal.html'),
            scope: {
                blockedassociations: '=',
                associationSchemas: '=',
                schema: '=',
                datamap: '=',
                isDirty: '=',
                originalDatamap: '=',
                cancelfn: '&',
                savefn: '&',
                paginationdata: '=',
                searchData: '=',
                searchOperator: '=',
                searchSort: '=',
                ismodal: '@',
                checked: '='
            },

            compile: function compile(tElement, tAttrs, transclude) {
                return {
                    post: function postLink(scope, iElement, iAttrs, controller) {
                        var modalData = $rootScope.modalTempData;
                        modalService.show(modalData);
                        $rootScope.modalTempData = null;
                    }
                }
            },
            controller: controller

        };

        return directive;
    }

    angular.module('sw_layout').directive('crudBodyModal', ['$rootScope', 'modalService', 'crudContextHolderService', crudBodyModal]);

})(angular);
