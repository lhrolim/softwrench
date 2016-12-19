(function (angular) {
    "use strict";
    const app = angular.module('sw_layout');
    app.directive('bodyrendered', function ($timeout, $log, menuService) {
        "ngInject";
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.schema.mode !== 'output' && scope.isSelectEnabled) {
                    element.data('selectenabled', scope.isSelectEnabled(scope.fieldMetadata));
                }
                if (scope.$last === true) {
                    $timeout(function () {
                        const parentElementId = scope.elementid;
                        $log.getInstance('application_dir#bodyrendered').debug('sw_body_rendered will get dispatched');
                        menuService.adjustHeight();
                        scope.$emit(JavascriptEventConstants.BodyRendered, parentElementId);
                    });
                }
            }
        };
    });


    app.directive('filterrowrendered', function ($timeout) {
        "ngInject";
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$last === true) {
                    $timeout(function () {
                        scope.$emit(JavascriptEventConstants.FilterRowRendered);
                    });
                }
            }
        };
    });

    app.controller("ApplicationController", applicationController);
    function applicationController($scope, $http, $log, $timeout,
        fixHeaderService, $rootScope, associationService, validationService, historyService,
        contextService, searchService, alertService, schemaService, userPreferencesService,
        checkpointService, focusService, detailService, crudContextHolderService, schemaCacheService, redirectService, crudlistViewmodel) {
        "ngInject";

        $scope.$name = 'applicationController';
        const currentFocusedIdx = 0;


        //#region listeners



        $rootScope.$on(JavascriptEventConstants.ResetFocusToCurrent, function (event, schema, field) {
            focusService.resetFocusToCurrent(schema, field);
        });

        $rootScope.$on(JavascriptEventConstants.MoveFocus, function (event, datamap, schema, attribute) {
            if (datamap[attribute] != null && datamap[attribute] != '') {
                focusService.moveFocus(datamap, schema, attribute);
            }
        });

        $scope.$on('sw_canceldetail', function (event, data, schema, msg) {
            $scope.doConfirmCancel(data, schema, "Are you sure you want to go back?");
        });


        $scope.$on(JavascriptEventConstants.NavigateRequestCrawl, function (event, applicationName, schemaId, mode, title, parameters) {
            const skipDirtyMessage = parameters.customParameters ? parameters.customParameters.skipDirtyMessage : false;
            if (!crudContextHolderService.getDirty() || skipDirtyMessage) {
                $scope.renderView(applicationName, schemaId, mode, title, parameters);
                return;
            }

            const msg = "Are you sure you want to leave the page?";
            alertService.confirmCancel(msg).then(function () {
                $scope.renderView(applicationName, schemaId, mode, title, parameters);
                crudContextHolderService.clearDirty();
                crudContextHolderService.clearDetailDataResolved();
                $scope.$digest();
            });
        });

        $scope.$on(JavascriptEventConstants.RenderView, function (event, applicationName, schemaId, mode, title, parameters, dashboardpanelid) {
            // this is to prevent application.js from recv'ing dashboard rendering
            if (dashboardpanelid == null) {
                $scope.renderView(applicationName, schemaId, mode, title, parameters);
            }
        });

        $scope.$on(JavascriptEventConstants.ModalShown, function (event, modaldata) {
            if (!$scope.modalincluded) {
                $scope.modalincluded = true;
                //required because we need to store somewhere while the directive is not yet compiled, and retrieve on postcompile...
                $rootScope.modalTempData = modaldata;
            }
        });

        $scope.$on('sw.redirect', function (event, args) {
            if (args.type && args.type.indexOf("ApplicationMenuItemDefinition") > -1) {
                contextService.deleteFromContext('detail.cancel.click');
            }
        });

        $scope.$on(JavascriptEventConstants.AppBeforeRedirection, function (event, parameters) {
            if (parameters.popupmode === "browser" || parameters.popupmode === "modal") {
                return;
            }

            $scope.multipleSchema = false;
            $scope.schemas = null;
            $scope.isDetail = false;
            crudContextHolderService.setActiveTab(null);
            //            fixHeaderService.unfix();

        });

        $scope.$on(JavascriptEventConstants.RenderViewWithData, function (event, data) {
            const nextSchema = data.schema;
            $scope.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
        });

        //#endregion

        function switchMode(mode, scope) {
            scope = scope || $scope;
            scope.isDetail = mode;
            scope.isList = !mode;
            var crud_context;
            if (scope.isDetail) {
                crud_context = contextService.fetchFromContext("crud_context", true);
                if (!crud_context) {
                    //this might happen if we´re handling a direct link
                    return;
                }

                if (crud_context.panelid != null || crud_context.applicationName !== scope.schema.applicationName) {
                    // the list was from a modal or dashboard panel
                    // or is from another aplication
                    // should not be considered when goint to detail
                    crud_context.detail_previous = null;
                    crud_context.detail_next = null;
                    crud_context.list_elements = null;
                    crud_context.panelid = null;
                    contextService.insertIntoContext("crud_context", crud_context);
                }

                var id = $scope.datamap[$scope.schema.idFieldName];
                const list = crud_context.list_elements;
                if (!list || !id) {
                    return;
                }

                var idAsString = String(id);
                const findById = function (item) {
                    return item.id === id || item.id === idAsString;
                };
                if (list.find(findById)) {
                    const current = list.findIndex(findById);
                    const previous = current - 1;
                    const next = current + 1;
                    crud_context.detail_previous = list[previous];
                    crud_context.detail_next = list[next];
                } else {
                    // not on list, so new created
                    crud_context.detail_previous = null;
                    crud_context.detail_next = list.length > 0 ? list[0] : null;
                    list.unshift({ id: id });
                }
                contextService.insertIntoContext("crud_context", crud_context);
            }

        }

        $scope.toList = function (data, scope) {
            if (scope == null) {
                scope = $scope;
            }
            $('#saveBTN').removeAttr('disabled');
            //we need this because the crud_list.js may not be rendered it when this event is dispatched, in that case it should from here when it starts

            contextService.insertIntoContext("grid_refreshdata", { data: data, panelid: null }, true);

            crudlistViewmodel.initGridFromServerResult(data, null);

            scope.$broadcast(JavascriptEventConstants.GRID_CHANGED);
            switchMode(false, scope);
        };


        function toDetail(scope) {

            switchMode(true, scope);
        };


        $scope.showList = function () {
            $scope.searchData = {};
            $scope.searchOperator = {};
            $scope.searchSort = {};
            $scope.selectPage(1);
        };


        $scope.renderViewWithData = function (applicationName, schemaId, mode, title, dataObject) {
            $scope.applicationname = applicationName;
            $scope.requestmode = mode;
            dataObject.mode = mode;
            $scope.renderData(dataObject);
        };

        //this code will get called when the user is already on a crud page and tries to switch view only.
        $scope.renderView = function (applicationName, schemaId, mode, title, parameters) {
            //Make a list of ticket ids in array to find the adjacent ones

            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $scope.requestpopup = parameters.popupmode ? parameters.popupmode : 'none';
            //change made to prevent popup for incident detail report
            const log = $log.getInstance("applicationController#renderView");
            if ($scope.requestpopup == 'browser' || $scope.requestpopup == 'report') {
                log.debug("calling goToApplicationView for application {0}".format(applicationName));
                $scope.$parent.goToApplicationView(applicationName, schemaId, mode, title, parameters);
                return;
            }

            //remove it, so its not used on server side
            var printMode = parameters.printMode;
            parameters.printMode = null;

            parameters.key = {};
            parameters.key.schemaId = schemaId;
            parameters.key.mode = mode;
            parameters.key.platform = platform();
            parameters.customParameters = parameters.customParameters || {};
            if (title) {
                parameters.title = title;
            }

            $scope.applicationname = applicationName;
            $scope.requestmode = mode;
            const urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));
            if (printMode == undefined) {
                //avoid the print url to be saved on the sessionStorage, breaking page refresh
                contextService.insertIntoContext("swGlobalRedirectURL", urlToCall, false);
                historyService.addToHistory(urlToCall, false, true);
            }
            log.info("calling url".format(urlToCall));

            //save the current scroll position to resotre when switching back
            contextService.insertIntoContext('scrollto', { 'applicationName': applicationName, 'scrollTop': document.body.scrollTop });
            log.info('Scroll From', applicationName, document.body.scrollTop);

            return $http.get(urlToCall)
                .then(function (response) {
                    const data = response.data;
                    // besides printMode is not undefined, we need to verify that printMode is true;
                    // otherwise disable printRequest, that will allow the pagination to update.
                    if (printMode != undefined && printMode) {
                        $rootScope.printRequested = true;
                    };

                    $scope.renderData(data, printMode);

                    if (parameters.autoloadcomposition) {
                        redirectService.redirectToTab(parameters.autoloadcomposition);
                    }
                });
        };


        $scope.renderSelectedSchema = function () {
            const selectedSchema = $scope.selectedSchema.value;
            if (selectedSchema == null || selectedSchema.schemaId == null) {
                return;
            }
            $scope.isDetail = false;
            $scope.renderView($scope.applicationname, selectedSchema.schemaId, $scope.selectedModeRequest, $scope.title, null);

        };

        function setWindowTitle(scope) {
            const strategy = scope.schema.properties["popup.window.titlestrategy"];
            const id = scope.datamap[scope.schema.idFieldName];
            const titleattribute = scope.schema.properties["popup.window.titleattribute"];
            if (titleattribute != null) {
                const overridenTitle = scope.datamap[titleattribute];
                window.document.title = String.format(overridenTitle, id);
                return;
            }
            if (strategy === "idonly") {
                window.document.title = id;
            } else if (strategy === "nameandid") {
                window.document.title = scope.schema.applicationName + " " + id;
            }
        }




        $scope.renderData = function renderData(result) {
            contextService.insertIntoContext("associationsresolved", false, true);
            $scope.isList = $scope.isDetail = false;
            $scope.crudsubtemplate = null;
            $scope.multipleSchema = false;
            $scope.schemas = null;
            var isModal = $scope.requestpopup && ($scope.requestpopup == 'modal' || $scope.requestpopup == 'multiplemodal');
            if (isModal) {
                $('#crudmodal').modal('show');
                $("#crudmodal").draggable();
                $scope.modal = {};
                $scope.showingModal = true;
            } else {
                /*if ($scope.schemas != null) {
                    $scope.previousschema = $scope.schemas;
                } else {*/
                $scope.previousschema = $scope.schema;
                //}
                var crudContext = contextService.fetchFromContext("crud_context", true);
                var contextPreviousData = crudContext == null ? {} : crudContext.previousData;
                var datamap = $scope.datamap ? $scope.datamap : {};
                $scope.previousdata = contextPreviousData == {} ? datamap : contextPreviousData;
            }
            var scope = isModal ? $scope.modal : $scope;

            scope.schema = schemaCacheService.getSchemaFromResult(result);

            // resultObject can be null only when SW is pointing to a Maximo DB different from Maximo WS DB
            scope.datamap = instantiateIfUndefined(result.resultObject);

            //Save the originalDatamap after the body finishes rendering. This will be used in the submit service to update
            //associations that were "removed" with a " ". This is because a null value, when sent to the MIF, is ignored
            scope.originalDatamap = angular.copy(scope.datamap);
            crudContextHolderService.originalDatamap(null,scope.originalDatamap);

            scope.extraparameters = instantiateIfUndefined(result.extraParameters);
            if (result.mode === undefined || "none".equalsIc(result.mode)) {
                //FIX SWWEB 1640 and SWWEB 1797
                result.mode = "input";
            }
            scope.mode = result.mode;

            if (scope.schema != null) {
                // for crud results, otherwise schema might be null
                scope.schema.mode = scope.mode;

                var currentSchema = crudContextHolderService.currentSchema();
                if (!currentSchema || scope.schema.applicationName === currentSchema.applicationName) {
                    crudContextHolderService.updateCrudContext(scope.schema, scope.datamap);
                } else {
                    crudContextHolderService.applicationChanged(scope.schema, scope.datamap);
                }
            }
            if (result.title != null) {
                $scope.$emit(JavascriptEventConstants.TitleChanged, result.title);
                if (GetPopUpMode() === 'browser') {
                    setWindowTitle(scope);
                }
            }
            var log = $log.getInstance("applicationcontroller#renderData");


            if (result.type === ResponseConstants.ApplicationDetailResult) {
                log.debug("Application Detail Result handled");
                detailService.fetchRelationshipData(scope, result);
                toDetail(scope);
                crudContextHolderService.detailLoaded();
            } else if (result.type === ResponseConstants.ApplicationListResult) {
                log.debug("Application List Result handled");
                $scope.toList(result, scope);
                crudContextHolderService.gridLoaded(result);
            } else if (result.crudSubTemplate != null) {
                log.debug("Crud Sub Template handled");
                $scope.crudsubtemplate = url(result.crudSubTemplate);
            }
            $scope.requestpopup = null;
            $rootScope.$broadcast(JavascriptEventConstants.TitleChanged, $scope.schema == null ? null : $scope.schema.title);
            $rootScope.$broadcast(JavascriptEventConstants.ApplicationRedirected, $scope.schema == null ? null : $scope.schema.applicationName, $scope.schema);
        };



        $scope.doConfirmCancel = function (data, schema, msg) {
            $('.no-touch [rel=tooltip]').tooltip('hide');

            // try to redirect from history or breadcrumb history
            //        if (historyService.redirectOneBack(msg)) {
            //            return;
            //        }

            if (!crudContextHolderService.getDirty()) {
                $scope.toSchema(data, schema);
                return;
            }

            alertService.confirmCancel(msg).then(() => {
                $scope.toListSchema(data, schema);
                crudContextHolderService.clearDirty();
                crudContextHolderService.clearDetailDataResolved();
                try {
                    $scope.$digest();
                } catch (e) { }
            });
        }

        $scope.toConfirmCancel = function (data, schema) {
            $scope.doConfirmCancel(data, schema, "Are you sure you want to cancel?");
        };

        $scope.toSchema = function (data, schema) {
            if (schema == null) {
                const currentSchema = crudContextHolderService.currentSchema();
                if (!currentSchema.stereotype.equalsIc("list")) {
                    return $scope.toListSchema(data, schema);
                }
            }
            if (schema.stereotype.equalsIc("list")) {
                $scope.toListSchema(data, schema);
            } else {
                $scope.toDetailSchema(data, schema);
            }
        }

        $scope.toDetailSchema = function (data, schema) {
            const log = $log.getInstance('application#toDetailSchema');
            const params = {};
            params["resultObject"] = data[0];
            params["schema"] = schema;
            params["type"] = "ApplicationDetailResult";
            // If the application has custom parameters get them from the datamap

            $scope.renderViewWithData(schema.applicationName, schema.schemaId, schema.mode, schema.title, params);
        }

        $scope.toListSchema = function (data, schema) {

            crudContextHolderService.disposeDetail();

            var log = $log.getInstance('application#toListSchema');
            $scope.multipleSchema = false;
            $scope.schemas = null;
            //                $('#crudmodal').modal('hide');
            $scope.showingModal = false;
            if (GetPopUpMode() == 'browser') {
                open(location, '_self').close();
            }
            var parameters = {};

            var pageSize;
            if (schema) {
                pageSize = userPreferencesService.getSchemaPreference("pageSize", schema.applicationName, schema.schemaId);
            } else {
                pageSize = userPreferencesService.getSchemaPreference("pageSize", $scope.applicationname, "list");
            }
            if (pageSize) {
                parameters["SearchDTO"] = parameters["SearchDTO"] || {};
                parameters["SearchDTO"].pageSize = pageSize;
            }

            var listName = $scope.schema.applicationName + ".list";
            if (schema != null && data != null) {
                $scope.schema = schema;
                $scope.datamap = data;
            } else {
                //if they are both null, it means that the previous data does not exist (F5 on browser). 
                //Let´s keep them untouched until the new one comes from server, otherwise after the $http call there will be errors on the $digest evalution
                var cancelSchema = $scope.schema.properties['detail.cancel.click'];
                // Check the context for an overloaded detail.cancel.click application and schema
                var overloadedCancelSchema = contextService.retrieveFromContext('detail.cancel.click', null, true);
                cancelSchema = overloadedCancelSchema != 'undefined' ? overloadedCancelSchema : cancelSchema;
                if (cancelSchema) {
                    //if this schema registers another application/schema/mode entry for the cancel click, let´s use it
                    var result = schemaService.parseAppAndSchema(cancelSchema);
                    parameters.application = result.app;
                    parameters.schema = result.schemaId;
                    parameters.mode = result.mode;
                    // Update list name used to retreive checkpoint data with application.schema of target list
                    listName = parameters.application + "." + parameters.schema;
                }

            }

            var checkPointData = checkpointService.fetchCheckpoint(listName);
            if (checkPointData) {
                parameters["SearchDTO"] = checkPointData.listContext;
            }



            // at this point, usually schema should be a list schema, on cancel call for instance, where we pass the previous schema. same goes for the datamap
            // this first if is more of an unexpected case
            if ($scope.schema == null || $scope.datamap == null || $scope.schema.stereotype === 'Detail' || $scope.schema.stereotype === 'DetailNew') {
                log.debug('rendering list view from server');
                $scope.renderListView(parameters);
            } else {
                if (schema) {
                    //SM - SWWEB-619 temp fix, at times (before everything is loaded?), this is run without a schema causing an exception, resulting in a UI glich
                    $scope.$emit(JavascriptEventConstants.TitleChanged, schema.title);
                }
                log.debug('rendering list view with previous data');

                data = {
                    //here we have to reproduce that the request is coming from the server, so use resultObject as the name.
                    //check crud_list#gridRefreshed
                    resultObject: $scope.datamap,
                    schema: schema,
                    pageResultDto: (checkPointData && checkPointData.length > 0) ? checkPointData[0].listContext : {},
                }
                $scope.toList(data);
            }
            //}
        };

        $scope.renderListView = function (parameters) {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="parameters">
            ///  application --> overrides the default application which would be the same as current. Useful for cancel clicks that should span different applications (on F5)
            /// </param>
            var applicationToGo = $scope.applicationname;
            if (parameters && parameters.application) {
                applicationToGo = parameters.application;
            }
            var schemaToGo = 'list';
            if (parameters && parameters.schema) {
                schemaToGo = parameters.schema;
            }

            $scope.multipleSchema = false;
            $scope.schemas = null;
            if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                //if we have a list schema already declared, keep it
                schemaToGo = $scope.schema.schemaId;
            }
            $scope.renderView(applicationToGo, schemaToGo, 'none', $scope.title, parameters);
        };


        function broadcastEx(msg) {
            const error = { errorMessage: msg}
            $rootScope.$broadcast(JavascriptEventConstants.ErrorAjax, error);
            alertService.notifyexception(error);
        }


        function doInit() {
            if ($scope.resultObject.redirectURL.indexOf("Application.html") == -1) {
                //pog to identify that this result if of the right type.
                return;
            }
            const dataObject = $scope.resultObject;
            const title = $scope.title;
            const mode = dataObject.mode;
            const applicationName = dataObject.applicationName;
            const schema = dataObject.schema;
            const schemaId = schema == null ? null : schema.schemaId;
            $scope.searchData = {};
            $scope.searchOperator = {};
            $scope.searchSort = {};

            if (dataObject.schemas == null) {
                if (dataObject.resultObject == null) {
                    if (dataObject.type === "NotFoundResponse") {
                        broadcastEx("Page not found.");
                        return;
                    }

                    broadcastEx("You don´t have permission to see that register, contact your administrator");
                    return;
                }
                $scope.renderViewWithData(applicationName, schemaId, mode, title, dataObject);
                return;
            }
            $scope.multipleSchemaHandling(dataObject);

        }


        //called first time a crud is registered
        function initApplication() {


            doInit();
            $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
                if (oldValue != newValue) {
                    $log.getInstance("applicationcontroller#initAplication").info("redirect detected");
                    doInit();
                }
            });

        }

        $scope.multipleSchemaHandling = function (dataObject) {
            $scope.crudsubtemplate = null;
            const title = dataObject.title;
            const applicationName = dataObject.applicationName;
            const mode = dataObject.mode;
            $scope.isDetail = false;
            $scope.isList = false;
            $scope.multipleSchema = true;
            //just don´t know why i had to bound ng-model to selectedSchema.value==> simply selectedSchema didn´t work as expected.
            $scope.selectedSchema = {};
            if (dataObject.placeHolder != null && dataObject.schemas[0].title != dataObject.placeHolder) {
                dataObject.schemas.unshift({ title: dataObject.placeHolder, schemaid: null });
            }
            $scope.selectedSchema.value = dataObject.schemas[0];
            $scope.schemas = dataObject.schemas;
            $scope.$emit(JavascriptEventConstants.TitleChanged, title);
            $scope.applicationname = applicationName;
            $scope.selectedModeRequest = mode;
            $scope.schemaSelectionLabel = dataObject.schemaSelectionLabel;
        };

        initApplication();

    }

})(angular);