(function (angular) {
    "use strict";



    function crudBodyModalController($scope, $http,$rootScope,$timeout, $filter, $injector,schemaService,
            formatService, fixHeaderService,
            searchService, tabsService,modalService,
            fieldService, commandService, i18NService,crudContextHolderService,
            submitService, redirectService,
            associationService, gridSelectionService, crudlistViewmodel) {

        $scope.$name = "crudbodymodal";
        $scope.save = function (selecteditem) {
            $scope.savefn({ selecteditem });
        };

        $scope.getPosition = function (schema, propertyName, defaultPosition) {
            if (!schema.properties || !schema.properties[propertyName]) {
                return defaultPosition;
            }
            return schema.properties[propertyName];
        };

        $scope.$on(JavascriptEventConstants.HideModal, function (event, selfThrown) {
            crudContextHolderService.clearCrudContext(modalService.panelid);
            if (selfThrown !== true) {
                $scope.cancel();
            }
        });

        $scope.closeModal = function () {
            $scope.modalshown = false;
            $('#crudmodal').modal("hide");
            $('#crudmodal').unbind("keypress");
            $rootScope.$broadcast(JavascriptEventConstants.HideModal, true);

            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
            $('.no-touch [rel=tooltip]').tooltip('hide');
            crudContextHolderService.disposeModal();
        };

        $scope.cancel = function () {
            $scope.closeModal();
            if ($scope.cancelfn) {
                $scope.cancelfn();
            }
        };

        $scope.$on(JavascriptEventConstants.ModalShown, function (event, modaldata) {
            $scope.showModal(modaldata);
            $scope.modalshown = true;
        });

        $scope.$on("sw.crud.list.filter.modal.clear", function () {
            gridSelectionService.clearSelection(null, null, modalService.panelid);
        });

        $scope.registerModalGlobalFunction = function (modaldata) {
            crudContextHolderService.registerSaveFn(modaldata.savefn);

            const position = this.getPosition($scope.schema, 'commandbar.bottom', 'modal.detail.primary');
            const commands = commandService.getBarCommands($scope.schema, position);
            if (!!commands) {
                const primaryCommand = commands.find(c => c.primary);
                if (primaryCommand) {
                    crudContextHolderService.registerPrimaryCommand(primaryCommand);
                }
            }
        }

        $scope.setJQueryListeners = function() {

            //make sure the scroll is sized correctly
            $timeout(function () {
                $(window).trigger("resize");
                $('#crudmodal').keypress(e=> {
                    if (e.which === 13) {
                        const command = crudContextHolderService.getPrimaryCommand();
                        if (!!command) {
                            $timeout(() => {
                                commandService.doCommand($scope, command);
                            }, 0, false);
                        }
                        
                    }
                });
            }, 310, false);
        }

        $scope.showModal = function (modaldata) {
            const schema = modaldata.schema;
            const datamap = modaldata.datamap;
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

            $scope.datamap = datamap;
            const datamapToUse = $.isEmptyObject(datamap) ? $scope.previousdata : datamap;
            $scope.originalDatamap = angular.copy(datamapToUse);

            fieldService.fillDefaultValues(schema.displayables, datamap, { parentdata: modaldata.previousdata, parentschema: modaldata.previousschema });
            $scope.registerModalGlobalFunction(modaldata);
            $scope.setJQueryListeners();

            $('#crudmodal').modal('show');

            //TODO: review this decision here it might not be suitable for all the scenarios
            crudContextHolderService.modalLoaded(datamapToUse, schema);

          

            if (schema.stereotype.equalsIc("list")) {
                //forcing underlying grid to refresh
                if (modaldata.appResponseData) {
                    crudlistViewmodel.initGridFromServerResult(modaldata.appResponseData, "#modal");
                } else if (datamap.length > 0) {
                    crudlistViewmodel.initGridFromDatamapAndSchema(datamap, schema, "#modal");
                } else if (!modaldata.fromLink) {
                    searchService.refreshGrid({}, {}, { panelid: "#modal", forcecleanup: true });
                }


            }

            return associationService.loadSchemaAssociations(datamapToUse, schema, { contextData: new ContextData("#modal") }).then(function () {
                if (modaldata.onloadfn) {
                    modaldata.onloadfn($scope);
                }
            });

        };

        $scope.save = function (selecteditem) {
            $scope.savefn({ selecteditem: selecteditem });
        };

        $scope.$on(JavascriptEventConstants.CrudSaved, () => {
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



    }

    crudBodyModalController.$inject = ["$scope", "$http","$rootScope","$timeout", "$filter", "$injector",
            "schemaService","formatService", "fixHeaderService",
            "searchService", "tabsService","modalService",
            "fieldService", "commandService", "i18NService","crudContextHolderService",
            "submitService", "redirectService",
            "associationService", "gridSelectionService", "crudlistViewmodel"];


    angular.module('sw_layout').controller("ExtractedCrudBodyModalController", crudBodyModalController);

    angular.module('sw_layout').directive('crudBodyModalWrapper', function ($compile) {
           "ngInject";

           return {
               restrict: 'E',
               replace: true,
               template: "<div data-id='crud-modal-wrapper'></div>",

               link: function (scope, element) {
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

    angular.module('sw_layout').directive('crudBodyModal',["contextService", "$rootScope", "modalService", function (contextService, $rootScope, modalService) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_body_modal.html'),
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

            link: function (scope, element) {
                const modalData = $rootScope.modalTempData;
                modalData.fromLink = true;
                modalService.showWithModalData(modalData);
                $rootScope.modalTempData = null;

                scope.setPaneHeight = function () {
                    const headerHeight = $('.modal-header:visible', element).outerHeight(true);
                    const footerHeight = $('.modal-footer:visible', element).outerHeight(true);
                    const contentHeight = $(element).outerHeight(true);
                    return contentHeight - headerHeight - footerHeight - 2;
                };
            },
            controller: "ExtractedCrudBodyModalController"
        };
    }]
    );

})(angular);
