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
                    "datamap='datamap' savefn='save(selecteditem)'" +
                    "is-dirty='false' " +
                    "is-list='isList' " +
                    "is-detail='true' " +
                    "association-options='associationOptions' " +
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

        controller: function($injector,$scope,$rootScope,fieldService) {
            $scope.$name = "crudbodymodalwrapper";
         

            $scope.$on('sw.modal.hide', function (event) {
                $scope.hideModal();
            });



            $scope.hideModal = function () {
                $('#crudmodal').modal('hide');
                $rootScope.showingModal = false;
            }



            function doInit() {
              
              
            }

            doInit();

        }
    }

});


app.directive('crudBodyModal', function ($rootScope,modalService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: url('/Content/Templates/crud/crud_body_modal.html'),
        scope: {
            isList: '=',
            isDetail: '=',
            blockedassociations: '=',
            associationOptions: '=',
            associationSchemas: '=',
            schema: '=',
            datamap: '=',
            isDirty: '=',
            originalDatamap: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
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

    

        controller: function ($scope, $http, $filter, $injector,
           formatService, fixHeaderService,
           searchService, tabsService,
           fieldService, commandService, i18NService,
           submitService, redirectService,
           associationService, contextService, alertService, validationService) {

            $scope.$name = "crudbodymodal";
            $scope.save = function (selecteditem) {
                $scope.savefn({ selecteditem: selecteditem });
            }

            $scope.$on('sw.modal.show', function (event, modaldata) {
                $scope.showModal(modaldata);
            });

            $scope.showModal = function (modaldata) {
                var schema = modaldata.schema;
                var datamap = modaldata.datamap;
                fieldService.fillDefaultValues(schema.displayables, datamap);
                $scope.schema = schema;
                $scope.originalsavefn = modaldata.savefn;
                $scope.datamap = {
                    fields: datamap
                };
                $('#crudmodal').modal('show');
                $("#crudmodal").draggable();
                $rootScope.showingModal = true;
            }

         
            $scope.save = function (selecteditem) {
                $scope.savefn({ selecteditem: selecteditem });
            }
          
            function doInit() {


                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService
                });
            }

            doInit();


        }
    }
});






