//var app = angular.module('sw_layout');

var app = angular.module('sw_layout');

app.directive('bodyrendered', function ($timeout, $log, menuService) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            element.data('selectenabled', scope.isSelectEnabled(scope.fieldMetadata));
            if (scope.$last === true) {
                $timeout(function () {
                    var parentElementId = scope.elementid;
                    $log.getInstance('application_dir#bodyrendered').debug('sw_body_rendered will get dispatched');
                    menuService.adjustHeight();
                    scope.$emit('sw_bodyrenderedevent', parentElementId);
                });
            }
        }
    };
});


app.directive('listtablerendered', function ($timeout, $log, menuService) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            $log.getInstance('application_dir#bodyrendered').trace('list table rendered');
            if (scope.$last === true || scope.datamap.length == 0) {
                $timeout(function () {
                    $log.getInstance('application_dir#bodyrendered').debug('list table rendered will get dispatched');
                    //                    menuService.adjustHeight();
                    scope.$emit('listTableRenderedEvent');
                });
            }
        }
    };
});

app.directive('filterrowrendered', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    scope.$emit('filterRowRenderedEvent');
                });
            }
        }
    };
});

function ApplicationController($scope, $http, $log, $templateCache, $timeout, fixHeaderService, $rootScope, associationService, validationService, contextService, searchService) {
    $scope.$name = 'applicationController';

    function switchMode(mode, scope) {
        if (scope == null) {
            scope = $scope;
        }
        scope.isDetail = mode;
        scope.isList = !mode;
        var crud_context;
        if (scope.isList) {
            var elements = [];
            for (var i = 0; i < $scope.datamap.length; i++) {

                elements.push($scope.datamap[i].fields[$scope.schema.idFieldName]);
            }
            crud_context = {
                list_elements: elements,
                detail_next: "0",
                detail_previous: "-1"
            };
            contextService.insertIntoContext("crud_context", crud_context);
        }
        if (scope.isDetail) {
            crud_context = contextService.fetchFromContext("crud_context", true);
            var id = $scope.datamap.fields[$scope.schema.idFieldName];
            if (crud_context.list_elements.indexOf(id) != -1) {
                var previous = crud_context.list_elements.indexOf(id) - 1;
                var next = crud_context.list_elements.indexOf(id) + 1;
                crud_context.detail_previous = crud_context.list_elements[previous];
                crud_context.detail_next = crud_context.list_elements[next];

                contextService.insertIntoContext("crud_context", crud_context);
            }
        }

    }

    $scope.toList = function (data, scope) {
        $('#saveBTN').removeAttr('disabled');
        scope.$broadcast("sw_gridrefreshed", data, $rootScope.printRequested);
        if (data != null && $rootScope.printRequested !== true) {
            //if its a printing operation, then leave the pagination data intact
            //this code needs to be here because the crud_list.js might not yet be included while this is loading... 
            //TODO: rethink about it
            $scope.paginationData = {};
            $scope.searchValues = data.searchValues;
            $scope.paginationData.pagesToShow = data.pagesToShow;
            $scope.paginationData.pageNumber = data.pageNumber;
            $scope.paginationData.selectedPage = data.pageNumber;
            $scope.paginationData.pageCount = data.pageCount;
            $scope.paginationData.pageSize = data.pageSize;
            $scope.paginationData.paginationOptions = data.paginationOptions;
            $scope.paginationData.totalCount = data.totalCount;
            $scope.paginationData.hasPrevious = data.hasPrevious;
            $scope.paginationData.hasNext = data.hasNext;
            $scope.paginationData.filterFixedWhereClause = data.filterFixedWhereClause;
            if (data.pageResultDto && data.pageResultDto.searchParams) {
                var result = searchService.buildSearchDataAndOperations(data.pageResultDto.searchParams, data.pageResultDto.searchValues);
                $scope.searchData = result.searchData;
                $scope.searchOperator = result.searchOperator;
            }
        }
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
        if (title == null) {
            title = $scope.title;
        }
        $scope.requestpopup = parameters.popupmode ? parameters.popupmode : 'none';
        //change made to prevent popup for incident detail report
        var log = $log.getInstance("applicationController#renderView");
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
        parameters.customParameters = {};
        parameters.title = title;

        $scope.applicationname = applicationName;
        $scope.requestmode = mode;
        var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));
        if (printMode == undefined) {
            //avoid the print url to be saved on the sessionStorage, breaking page refresh
            sessionStorage.swGlobalRedirectURL = urlToCall;
        }
        log.info("calling url".format(urlToCall));
        $http.get(urlToCall)
            .success(function (data) {
                if (printMode != undefined) {
                    $rootScope.printRequested = true;
                };
                $scope.renderData(data, printMode);
            });
    };


    $scope.renderSelectedSchema = function () {
        var selectedSchema = $scope.selectedSchema.value;
        if (selectedSchema == null || selectedSchema.schemaId == null) {
            return;
        }
        $scope.isDetail = false;
        $scope.renderView($scope.applicationname, selectedSchema.schemaId, $scope.selectedModeRequest, $scope.title, null);

    };

    function setWindowTitle(scope) {
        var strategy = scope.schema.properties["popup.window.titlestrategy"];
        var id = scope.datamap.fields[scope.schema.idFieldName];
        var titleattribute = scope.schema.properties["popup.window.titleattribute"];
        if (titleattribute != null) {
            var overridenTitle = scope.datamap.fields[titleattribute];
            window.document.title = String.format(overridenTitle, id);
            return;
        }
        if (strategy == "idonly") {
            window.document.title = id;
        } else if (strategy == "nameandid") {
            window.document.title = scope.schema.applicationName + " " + id;
        }
    }




    $scope.renderData = function renderData(result) {
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
            $scope.previousdata = $scope.datamap;
        }
        var scope = isModal ? $scope.modal : $scope;
        scope.schema = result.schema;

        // resultObject can be null only when SW is pointing to a Maximo DB different from Maximo WS DB
        scope.datamap = instantiateIfUndefined(result.resultObject);

        scope.mode = result.mode;
        if (scope.schema != null) {
            scope.schema.mode = scope.mode;
        }
        if (result.title != null) {
            $scope.$emit('sw_titlechanged', result.title);
            if (GetPopUpMode() == 'browser') {
                setWindowTitle(scope);
            }
        }
        var log = $log.getInstance("applicationcontroller#renderData");
        validationService.clearDirty();
        if (result.type == 'ApplicationDetailResult') {
            log.debug("Application Detail Result handled");
            if (result.schema.properties['associationstoprefetch'] != "#all") {
                $timeout(function () {
                    log.info('fetching eager associations of {0}'.format(scope.schema.applicationName));
                    associationService.getEagerAssociations(scope);
                });
            }
            associationService.updateAssociationOptionsRetrievedFromServer(scope, result.associationOptions, scope.datamap.fields);
            scope.compositions = result.compositions;
            toDetail(scope);
        } else if (result.type == 'ApplicationListResult') {
            log.debug("Application List Result handled");
            $scope.toList(result, scope);
            fixHeaderService.FixHeader();
        } else if (result.crudSubTemplate != null) {

            log.debug("Crud Sub Template handled");
            $scope.crudsubtemplate = url(result.crudSubTemplate);
        }
        $scope.requestpopup = null;
    };




    //called first time a crud is registered
    function initApplication() {
        $scope.$on('sw_renderview', function (event, applicationName, schemaId, mode, title, parameters) {
            $scope.renderView(applicationName, schemaId, mode, title, parameters);
        });
        $scope.$on('sw.modal.show', function (event, modaldata) {
            if (!$scope.modalincluded) {
                $scope.modalincluded = true;
                //required because we need to store somewhere while the directive is not yet compiled, and retrieve on postcompile...
                $rootScope.modalTempData = modaldata;
            }
        });
        $scope.$on('sw_applicationredirected', function (event, parameters) {
            if (parameters.popupmode == "browser" || parameters.popupmode == "modal") {
                return;
            }

            $scope.multipleSchema = false;
            $scope.schemas = null;
            $scope.isDetail = false;
            contextService.setActiveTab(null);
            //            fixHeaderService.unfix();

        });

        $scope.$on('sw_applicationrenderviewwithdata', function (event, data) {
            var nextSchema = data.schema;
            $scope.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
        });
        window.onbeforeunload = function () {
            spin.stop();
        };

        doInit();
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue) {
                $log.getInstance("applicationcontroller#initAplication").info("redirect detected");
                doInit();
            }
        });

    }

    function doInit() {
        if ($scope.resultObject.redirectURL.indexOf("Application.html") == -1) {
            //pog to identify that this result if of the right type.
            return;
        }
        var dataObject = $scope.resultObject;
        var title = $scope.title;

        var mode = dataObject.mode;
        var applicationName = dataObject.applicationName;
        var schema = dataObject.schema;
        var schemaId = schema == null ? null : schema.schemaId;

        $scope.searchData = {};
        $scope.searchOperator = {};
        $scope.searchSort = {};

        if (dataObject.schemas == null) {
            if (dataObject.resultObject == null) {
                //this means, most likely a security issue
                var error = { errorMessage: 'You don´t have permission to see that register, contact your administrator' }
                $rootScope.$broadcast('sw_ajaxerror', error);
                return;
            }
            $scope.renderViewWithData(applicationName, schemaId, mode, title, dataObject);
            return;
        }
        $scope.multipleSchemaHandling(dataObject);

    }

    $scope.multipleSchemaHandling = function (dataObject) {
        $scope.crudsubtemplate = null;
        var title = dataObject.title;
        var applicationName = dataObject.applicationName;
        var mode = dataObject.mode;

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
        $scope.$emit('sw_titlechanged', title);
        $scope.applicationname = applicationName;
        $scope.selectedModeRequest = mode;
        $scope.schemaSelectionLabel = dataObject.schemaSelectionLabel;
    };

    initApplication();

}