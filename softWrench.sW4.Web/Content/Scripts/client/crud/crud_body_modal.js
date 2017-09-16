(function (angular) {
    "use strict";



    function crudBodyModalController($q,$scope, $http, $rootScope, $timeout, $filter, $injector, schemaService,
        formatService, fixHeaderService,
        searchService, tabsService, modalService,
        fieldService, commandService, i18NService, crudContextHolderService,
        submitService, redirectService,
        associationService, gridSelectionService, crudlistViewmodel, alertService) {

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

        $scope.$on(JavascriptEventConstants.HideModal, function (event, selfThrown, ignoreConfirmClose) {
            crudContextHolderService.clearCrudContext(modalService.panelid);
            if (selfThrown !== true) {
                $scope.cancel(ignoreConfirmClose);
            }
        });

        $scope.closeModal = function (ignoreConfirmClose) {

            const doClose = function() {
                $scope.modalshown = false;


                const modal = $('#crudmodal');

                modal.modal("hide");
                modal.unbind("keypress");

                if ($scope.resizable) {
                    $(".modal-content").removeAttr('style');
                    $('.modal-content').resizable('destroy');
                }

                modal.height($scope.originalHeight);
                $rootScope.$broadcast(JavascriptEventConstants.HideModal, true);

                $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
                $('.no-touch [rel=tooltip]').tooltip('hide');
                crudContextHolderService.disposeModal();
            }


            if ("true" === $scope.schema.properties["detail.modal.confirmclose"] && !ignoreConfirmClose) {
                return alertService.confirm("Are you sure you want to cancel?").then(doClose);
            }
            return $q.when().then(doClose);

        };



        $scope.clickOutside = function () {
            if ($scope.cancelOnClickOutside) {
                crudContextHolderService.clearCrudContext(modalService.panelid);
                $scope.cancel();
            }
        }

        $scope.cancel = function (ignoreConfirmClose) {
            $scope.closeModal(ignoreConfirmClose).then(() => {
                if ($scope.cancelfn) {
                    $scope.cancelfn();
                }    
            });
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

        $scope.setJQueryListeners = function () {

            //make sure the scroll is sized correctly
            $timeout(function () {
                $(window).trigger("resize");
                $('#crudmodal').keypress(e => {
                    if (e.which === 13 && !$(e.target).is("input") && !$(e.target).is("textarea")) {
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

            const notCloseAfterSave = modaldata.closeAfterSave === false;
            $scope.closeAfterSave = !notCloseAfterSave;

            $scope.cancelOnClickOutside = modaldata.cancelOnClickOutside || false;
            //by default modals, should render as detail stereotype mode
            $scope.isDetail = schemaService.isDetail(schema, true);
            $scope.isList = schemaService.isList(schema);
            $scope.useavailableheight = modaldata.useavailableheight || false;


            $scope.datamap = datamap;
            const datamapToUse = $.isEmptyObject(datamap) ? $scope.previousdata : datamap;
            $scope.originalDatamap = angular.copy(datamapToUse);

            fieldService.fillDefaultValues(schema.displayables, datamap, { parentdata: modaldata.previousdata, parentschema: modaldata.previousschema });
            $scope.registerModalGlobalFunction(modaldata);
            $scope.setJQueryListeners();
            const modal = $('#crudmodal');

            modal.modal('show');
            const originalHeight = modal.height();
            $scope.originalHeight = originalHeight;
            if (!!modaldata.resizable) {
                $scope.resizable = true;
                let elements = "#crudmodal .jspContainer, modal-footer";
                if (!!modaldata.resizableElements) {
                    elements += "," + modaldata.resizableElements;
                }

                $(".modal-content").resizable({
                    alsoResize: elements,
                    handles: 'n, s',
                    resize: function (event, ui) {
                        //TODO: it seems like there´s a bug on the resize whereas a vertical scroll would change the width from 100% to a calcualted value which in turn breaks the screen
                        $("#crudmodal iframe").width(100 + '%');
                    }
                });
            }




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

    crudBodyModalController.$inject = ["$q","$scope", "$http", "$rootScope", "$timeout", "$filter", "$injector",
        "schemaService", "formatService", "fixHeaderService",
        "searchService", "tabsService", "modalService",
        "fieldService", "commandService", "i18NService", "crudContextHolderService",
        "submitService", "redirectService",
        "associationService", "gridSelectionService", "crudlistViewmodel", "alertService"];


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

    angular.module('sw_layout').directive('crudBodyModal', ["contextService", "$rootScope", "modalService", function (contextService, $rootScope, modalService) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_body_modal.html'),
            scope: {
                blockedassociations: '=',
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
