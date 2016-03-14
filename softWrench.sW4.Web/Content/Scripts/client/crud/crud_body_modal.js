(function (angular) {
    "use strict";

angular.module('sw_layout')
    .directive('crudBodyModalWrapper', function ($compile) {
    "ngInject";

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
        controller: function ($scope) {
            $scope.$name = "crudbodymodalwrapper";
        }
    }
});

function crudBodyModal($rootScope, modalService, crudContextHolderService, schemaService) {

    var controller = function($scope, $http, $filter, $injector,
        formatService, fixHeaderService,
        searchService, tabsService,
        fieldService, commandService, i18NService,
        submitService, redirectService,
        associationService, gridSelectionService) {
        "ngInject";

        $scope.$name = "crudbodymodal";
        $scope.save = function(selecteditem) {
            $scope.savefn({ selecteditem: selecteditem });
        };

        $scope.getPosition = function (schema, propertyName, defaultPosition) {
            if (!schema.properties || !schema.properties[propertyName]) {
                return defaultPosition;
            }
            return schema.properties[propertyName];
        };

        $scope.$on('sw.modal.hide', function(event) {
            crudContextHolderService.clearCrudContext(modalService.panelid);
            $scope.closeModal();
        });

        $scope.closeModal = function() {
            $scope.modalshown = false;
            $('#crudmodal').modal('hide');
            $rootScope.showingModal = false;

            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
            $('.no-touch [rel=tooltip]').tooltip('hide');
            crudContextHolderService.disposeModal();
        };

        $scope.cancel = function() {
            $scope.closeModal();
            if ($scope.cancelfn) {
                $scope.cancelfn();
            }
        };

        $scope.$on('sw.modal.show', function(event, modaldata) {
            $scope.showModal(modaldata);
            $scope.modalshown = true;
        });

        $scope.$on("sw.crud.list.filter.modal.clear", function(event, args) {
            gridSelectionService.clearSelection(null, null, modalService.panelid);
        });

        $scope.showModal = function(modaldata) {
            var schema = modaldata.schema;
            var datamap = modaldata.datamap;
            $scope.schema = schema;
            $scope.originalsavefn = modaldata.savefn;
            $scope.cancelfn = modaldata.cancelfn;
            $scope.previousschema = modaldata.previousschema;
            $scope.previousdata = modaldata.previousdata;
            $scope.modaltitle = modaldata.title;
            $scope.cssclass = modaldata.cssclass;
            $scope.closeAfterSave = modaldata.closeAfterSave || true;
            //by default modals, should render as detail stereotype mode
            $scope.isDetail = schemaService.isDetail(schema, true);
            $scope.isList = schemaService.isList(schema);

            $scope.datamap = {
                fields: datamap
            };
            var datamapToUse = $.isEmptyObject(datamap) ? $scope.previousdata : datamap;
            $scope.originalDatamap = angular.copy(datamapToUse);
            fieldService.fillDefaultValues(schema.displayables, datamap, { parentdata: modaldata.previousdata, parentschema: modaldata.previousschema });
            $('#crudmodal').modal('show');
            $("#crudmodal").draggable();
            $rootScope.showingModal = true;
            //TODO: review this decision here it might not be suitable for all the scenarios
            crudContextHolderService.modalLoaded(datamapToUse);

            associationService.loadSchemaAssociations(datamapToUse, schema).then(function () {
                if (modaldata.onloadfn) {
                    modaldata.onloadfn($scope);
                }
            });
        };

        $scope.save = function(selecteditem) {
            $scope.savefn({ selecteditem: selecteditem });
        };

        $scope.$on('sw.crud.detail.savecompleted', function (event, modaldata) {
            if ($scope.closeAfterSave) {
                $scope.closeModal();
            }
        });

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
    };

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
        link: function (scope, element, attrs) {
            var modalData = $rootScope.modalTempData;
            modalService.show(modalData);
            $rootScope.modalTempData = null;
        },
        controller: controller
    };

    return directive;
}

angular.module('sw_layout').directive('crudBodyModal', ['$rootScope', 'modalService', 'crudContextHolderService', 'schemaService', 'gridSelectionService', crudBodyModal]);

})(angular);
