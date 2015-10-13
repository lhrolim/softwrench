//var app = angular.module('sw_layout');

var app = angular.module('sw_layout');

app.directive('bodyrendered', function ($timeout, $log, menuService) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.schema.mode != 'output' && scope.isSelectEnabled) {
                element.data('selectenabled', scope.isSelectEnabled(scope.fieldMetadata));
            }
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
            var log = $log.getInstance('application_dir#bodyrendered');
            if (scope.$first) {
                log.debug('init list table rendered');
            } else {
                log.trace('list table rendered');
            }
            if (scope.$last === true || scope.datamap.length == 0) {
                $timeout(function () {
                    log.debug('list table rendered will get dispatched');
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

function ApplicationController($scope, $http, $templateCache, $timeout, $log, fixHeaderService, $rootScope, associationService, alertService, contextService, detailService, spinService, schemaCacheService, crudContextHolderService) {
    $scope.$name = 'applicationController';

    function switchMode(mode, scope) {
        if (scope == null) {
            scope = $scope;
        }
        scope.isDetail = mode;
        scope.isList = !mode;
    }

    $scope.toList = function (data, scope) {
        $('#saveBTN').removeAttr('disabled');
        if (data != null && $rootScope.printRequested !== true) {
            //if its a printing operation, then leave the pagination data intact
            scope.paginationData = {};
            scope.searchValues = data.searchValues;
            scope.paginationData.pagesToShow = data.pagesToShow;
            scope.paginationData.pageNumber = data.pageNumber;
            $scope.paginationData.selectedPage = data.pageNumber;
            scope.paginationData.pageCount = data.pageCount;
            scope.paginationData.pageSize = data.pageSize;
            scope.paginationData.paginationOptions = data.paginationOptions;
            scope.paginationData.totalCount = data.totalCount;
            scope.paginationData.hasPrevious = data.hasPrevious;
            scope.paginationData.hasNext = data.hasNext;
            scope.paginationData.filterFixedWhereClause = data.filterFixedWhereClause;
            scope.paginationData.unionFilterFixedWhereClause = data.unionFilterFixedWhereClause;
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
        if (nullOrEmpty(strategy)) {
            window.document.title = scope.title;
        }
        else if (strategy == "idonly") {
            window.document.title = id;
            if (id == null) {
                window.document.title = scope.title;
            }
        } else if (strategy == "nameandid") {
            window.document.title = scope.schema.applicationName + " " + id;
        } else if (strategy == "schematitle") {
            window.document.title = scope.schema.title;
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
        scope.schema = schemaCacheService.getSchemaFromResult(result);

        // resultObject can be null only when SW is pointing to a Maximo DB different from Maximo WS DB
        scope.datamap = instantiateIfUndefined(result.resultObject);
        scope.timeStamp = result.timeStamp;

        scope.mode = result.mode;
        if (scope.schema != null) {
            scope.schema.mode = scope.mode;
            crudContextHolderService.updateCrudContext(scope.schema);
        }
        if (result.title != null) {
            $scope.$emit('sw_titlechanged', result.title);
            if (IsPopup()) {
                setWindowTitle(scope);
            }
        }
        var log = $log.getInstance("applicationcontroller#renderData");
        if (result.type == 'ApplicationDetailResult') {
            log.debug("Application Detail Result handled");
            detailService.fetchRelationshipData(scope, result);
            toDetail(scope);
        } else if (result.type == 'ApplicationListResult') {
            log.debug("Application List Result handled");
            $scope.toList(result, scope);
            fixHeaderService.FixHeader();
            $scope.$broadcast('sw_griddatachanged', scope.datamap, scope.schema);
        } else if (result.crudSubTemplate != null) {

            log.debug("Crud Sub Template handled");
            $scope.crudsubtemplate = url(result.crudSubTemplate);
        }

        //HAP-698 - init scrollpage
        //TODO: find event after menu loads to init (remove 1 second timeout)
        $timeout(function () {

            //HAP-876 - resize the nav, to make sure it is scrollable
            $('.menu-primary').height($(window).height());

            //set the scrollpane
            $('.menu-primary').jScrollPane({ maintainPosition: true });
        }, 1000);

        $scope.requestpopup = null;
    };
    
    //called first time a crud is registered
    function initApplication() {
        $scope.$on('sw_renderview', function (event, applicationName, schemaId, mode, title, parameters) {
            $scope.renderView(applicationName, schemaId, mode, title, parameters);
        });

        $scope.$on('sw_applicationredirected', function (event, parameters) {
            if (parameters.popupmode == "browser" || parameters.popupmode == "modal") {
                return;
            }

            $scope.multipleSchema = false;
            $scope.schemas = null;
            //            fixHeaderService.unfix();
            $scope.isDetail = false;
            contextService.setActiveTab(null);
            //            $scope.isList = false;
        });

        $scope.$on('sw_applicationrenderviewwithdata', function (event, data) {
            var nextSchema = data.schema;
            $scope.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
        });
        window.onbeforeunload = function () {
            spinService.stop();
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
                alertService.alert('You don´t have permission to see that register, contact your administrator');
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
        if (GetPopUpMode() == 'browser') {
            window.document.title = title;
        }
        $scope.applicationname = applicationName;
        $scope.selectedModeRequest = mode;
        $scope.schemaSelectionLabel = dataObject.schemaSelectionLabel;
    };

    initApplication();

}