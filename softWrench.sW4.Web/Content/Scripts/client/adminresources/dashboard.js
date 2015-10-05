
app.directive('dashboardsdone', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
                });
            }
        }
    };
});

function DashboardController($scope, $http, $templateCache, $rootScope, formatService, i18NService, contextService, schemaService,fixHeaderService) {

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.i18NLabel = function (fieldMetadata) {
        return i18NService.getI18nLabel(fieldMetadata, $scope.schema);
    };


    $scope.renderDashboard = function (index, fetchData) {

        $scope.schema = null;
        $scope.datamap = null;
        $scope.totalCount = null;
        $scope.currentDashboardIndex = index;


        if (index < 0 && index >= $scope.dashboards.length) {
            $scope.$emit('sw_titlechanged', $scope.i18N('general.error', 'Error'));
            return;
        }

        var currentDashboard = $scope.dashboards[index];
        var parameters = {};
        parameters.key = {};
        parameters.key.schemaId = currentDashboard.schemaId;
        parameters.key.mode = currentDashboard.mode;
        parameters.key.platform = platform();
        parameters.SearchDTO = {};
        parameters.SearchDTO.pageSize = currentDashboard.pageSize;
        parameters.SearchDTO.searchParams = currentDashboard.searchParams;
        parameters.SearchDTO.searchValues = currentDashboard.searchValues;
        parameters.SearchDTO.Context = {};
        parameters.SearchDTO.Context.MetadataId = currentDashboard.id;


        $http.get(url("/api/data/" + currentDashboard.applicationName + "?" + $.param(parameters)))
            .success(function (result) {
                $scope.schema = result.schema;
                $scope.datamap = result.resultObject;
                $scope.totalCount = result.totalCount;
                $scope.pageSize = result.pageSize;
                $scope.loaded = true;
                $scope.dashboards[index].totalCount = result.totalCount;
            });

    };

    $scope.format = function (value, column) {
        return formatService.format(value, column, $scope.datamap);
    };

    $scope.viewAll = function (index) {
        var dashboard;
        if (index < 0 && index >= $scope.dashboards.length) {
            $scope.$emit('sw_titlechanged', $scope.i18N('general.error', 'Error'));
            return;
        } else {
            dashboard = $scope.dashboards[index];
        }

        var searchDTO = {
            searchParams: dashboard.searchParams,
            searchValues: dashboard.searchValues
        };
        //this is used for keeping the same whereclause on the viewAll grid
        contextService.insertIntoContext('currentmetadata', dashboard.id);
        var schemaObj = schemaService.parseAppAndSchema(dashboard.viewAllSchema);
        var applicationToUse = schemaObj.app == null ? dashboard.applicationName : schemaObj.app;
        var schemaId = schemaObj.schemaId;
        $scope.goToApplicationView(applicationToUse, schemaId, dashboard.mode, dashboard.title, { SearchDTO: searchDTO });
    };



    $scope.showDetail = function (datamap) {


        var dashboard = $scope.dashboards[$scope.currentDashboardIndex];
        var schemaObj = schemaService.parseAppAndSchema(dashboard.detailSchema);
        var applicationToUse = schemaObj.app == null ? dashboard.applicationName : schemaObj.app;
        var id = dashboard.idFieldName != "" ? datamap[dashboard.idFieldName] : datamap[$scope.schema.idFieldName];
        var parameters = {
            id: id,
            popupmode: $scope.schema.properties['list.click.popupmode'],
            key: {
                schemaId: schemaObj.schemaId,
                mode: $scope.schema.properties['list.click.mode'],
                platform: "web"
            }
        };

        if (parameters.popupmode == 'browser') {
            $scope.$parent.goToApplicationView(applicationToUse, parameters.key.schemaId, parameters.key.mode, $scope.schema.title, parameters);
            return;
        }

        $scope.applicationname = applicationToUse;

        $http.get(url("/api/data/" + applicationToUse + "?" + $.param(parameters)))
            .success(function renderData(result) {

                $('#crudmodal').modal('show');
                $("#crudmodal").draggable();

                $scope.modal = {};
                $scope.modal.schema = result.schema;
                $scope.modal.datamap = result.resultObject;

                if (result.schema.title != null && $scope.modal.title == null) {
                    $scope.modal.title = result.schema.title;
                }

                $scope.modal.associationOptions = result.associationOptions;
                $scope.modal.associationSchemas = result.associationSchemas;
                $scope.modal.compositions = result.Compositions;
                $scope.modal.isDetail = true;
                $scope.modal.isList = false;
            });

    };

    function initDashboard() {
        //        $scope.$emit('sw_titlechanged', "Home");
        $scope.dashboards = $scope.resultData;

        $scope.renderDashboard(0,false);
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.crudSubTemplate != null && $scope.resultObject.crudSubTemplate.indexOf("HapagHome.html") != -1) {
                $scope.dashboards = $scope.resultData;
                $scope.renderDashboard(0,false);
            }
        });

    }


    initDashboard();



}