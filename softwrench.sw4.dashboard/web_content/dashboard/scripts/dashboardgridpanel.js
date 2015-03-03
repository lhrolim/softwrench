var app = angular.module('sw_layout');

app.directive('dashboardgridpanel', function ($timeout, $log, $rootScope, contextService, redirectService, searchService,
                                                validationService, associationService, fixHeaderService, dashboardAuxService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboardgridpanel.html'),
        scope: {
            panelrow: '=',
            panelcol: '@',
            paneldatasource: '=',
            dashboardid:'='
        },

        controller: function ($scope, $http, $rootScope) {

            $scope.getPanelSourceData = function () {
                // TODO: See if we can pass the object as json instead of converting it.
                //Luiz: yes "=" instead of "@" and removing {{ }} on html
                var dashboardPanelInfo = $scope.paneldatasource;

                if (dashboardPanelInfo != null) {
                    $scope.title = dashboardPanelInfo.panel['title'];

                    var schemaReference = dashboardPanelInfo.panel['schemaRef'];
                    var application = dashboardPanelInfo.panel['application'];

                    var controller = 'data';

                    // TODO: Additional changes to allow field grid and sort?? 
                    var redirectUrl = redirectService.getApplicationUrl(application, schemaReference, null, $scope.title);

                    // call service and apply data into crud list object.
                    $http.get(redirectUrl)
                        .success(
                            function (data) {
                                // store data into individual panel
                                $scope.dashboardpanel = data;

                                if ($scope.dashboardpanel.pageResultDto['searchSort'] == null) {
                                    var searchSort = {};

                                    // TODO: will add logic for sorting when available
                                    // sortSelection.field
                                    // sortSelection.order

                                    $scope.searchSort = searchSort;
                                } else {
                                    $scope.searchSort = $scope.dashboardpanel.pageResultDto['searchSort'];
                                }

                                $scope.schema = data.schema;
                                $scope.dashboardpanelid = dashboardPanelInfo.id; 
                                $scope.datamap = data.resultObject;
                                $scope.associationOptions = data.associationOptions;
                                $scope.searchValues = data.searchValues;

                                // TODO: In the future this might not be true if we allow other functionalities
                                $scope.isList = true;

                                $scope.paginationData = {};
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

                                // Maybe I should use a observer at this point... 
                                $scope.isDataAvailable = true;
                            })
                        .error(
                            function (data) {
                                var errordata = {
                                    errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                                    errorStack: data.message 
                                }

                                $rootScope.$broadcast("sw_ajaxerror", errordata);
                        });
                }
            }

            $scope.isDataAvailable = false; 

            $scope.getPanelSourceData();

            $scope.$on('sw_lazyloaddashboard', function(event,tabId) {
                if (tabId == $scope.dashboardid) {
                    $scope.getPanelSourceData();
                }
            });

            //this code will get called when the user is already on a crud page and tries to switch view only.
            $scope.renderView = function (applicationName, schemaId, mode, title, parameters) {
                if (parameters === undefined || parameters == null) {
                    parameters = {};
                }

                if (title == null) {
                    title = $scope.title;
                }

                parameters.key = {};
                parameters.key.schemaId = schemaId;
                parameters.key.mode = mode;
                parameters.key.platform = platform();
                parameters.customParameters = {};

                parameters.title = title;
                parameters.printMode = null;

                $scope.applicationname = applicationName;
                $scope.requestmode = mode;

                var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));

                $http.get(urlToCall)
                    .success(function (data) {
                        $scope.renderData(data);
                    });
            };

            $scope.renderData = function renderData(result) {
                contextService.insertIntoContext("associationsresolved", false, true);
                $scope.isList = true;
                $scope.crudsubtemplate = null;
                $scope.multipleSchema = false;
                $scope.schemas = null;

                $scope.previousschema = $scope.schema;
                $scope.previousdata = $scope.datamap;

                $scope.schema = result.schema;

                // resultObject can be null only when SW is pointing to a Maximo DB different from Maximo WS DB
                $scope.datamap = instantiateIfUndefined(result.resultObject);

                $scope.extraparameters = instantiateIfUndefined(result.extraParameters);

                $scope.mode = result.mode;
                if ($scope.schema != null) {
                    $scope.schema.mode = $scope.mode;
                }

                validationService.clearDirty();

                // It should be application list result; however, we should do a validation to make sure
                if (result.type == 'ApplicationListResult') {
                    $scope.toList(result, $scope);

                    associationService.updateAssociationOptionsRetrievedFromServer($scope, result.associationOptions, null);
                    fixHeaderService.FixHeader();

                    $scope.$broadcast('sw_griddatachanged', $scope.datamap, $scope.schema);
                } 
            };

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
            }

            $scope.toList = function (data, scope) {
                if (scope == null) {
                    scope = $scope;
                }

                $scope.$broadcast("sw_gridrefreshed", data, $rootScope.printRequested);

                if (data != null) {
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
                    $scope.searchData = {};
                    $scope.searchOperator = {};

                    if (data.pageResultDto && data.pageResultDto.searchParams) {
                        var result = searchService.buildSearchDataAndOperations(data.pageResultDto.searchParams, data.pageResultDto.searchValues);
                        $scope.searchData = result.searchData;
                        $scope.searchOperator = result.searchOperator;
                    }
                }

                switchMode(false, $scope);
            };

            $scope.$on('sw_renderview', function (event, applicationName, schemaId, mode, title, parameters, dashboardpanelid) {
                if (dashboardpanelid != null) {
                    $scope.renderView(applicationName, schemaId, mode, title, parameters);
                }
            });
        },

        link: function (scope, element, attrs) {

        }
    }
});