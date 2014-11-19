var app = angular.module('sw_layout');

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';

app.factory('excelService', function ($rootScope, $http, $timeout, $log, tabsService, fixHeaderService,
    i18NService,
    redirectService, searchService, contextService, fileService) {

    return {
        showModalExportToExcel: function (fn,schema, searchData, searchSort, searchOperator, paginationData) {
            //TODO: fix this crap
            var modalTitle = i18NService.get18nValue('_exportotoexcel.export', 'Export') + " " + schema.applicationName+
                " " + i18NService.get18nValue('_exportotoexcel.toexcel', 'to excel');
            var selectText = i18NService.get18nValue('_exportotoexcel.selectexportmode', 'Select export mode');
            var exportModeGridOnlyText = i18NService.get18nValue('_exportotoexcel.gridonly', 'Grid only');
            var exportModeAllTheColumnsText = i18NService.get18nValue('_exportotoexcel.allthecolumns', 'Asset List');
            var exportModeCategoriesText = i18NService.get18nValue('_exportotoexcel.categories', 'Categories');

            bootbox.dialog({
                message: "<form id='infos' action=''>" +
                    "<label class='control-label'>" + selectText + ":</label><br>" +
                    "<label class='control-label' for='gridonlyid'>" +
                    "<input type='radio' name='exportMode' id='gridonlyid' value='list' checked='checked' /> "
                    + exportModeGridOnlyText +
                    "</label><br>" +
                    "<label class='control-label' for='allthecolumnsid'>" +
                    "<input type='radio' name='exportMode' id='allthecolumnsid' value='exportallthecolumns' /> "
                    + exportModeAllTheColumnsText +
                    "</label>"+
                    "</form>",
                title: modalTitle,
                buttons: {
                    cancel: {
                        label: i18NService.get18nValue('_exportotoexcel.cancel', 'Cancel'),
                        className: "btn btn-default",
                        callback: function () {
                            return null;
                        }
                    },
                    main: {
                        label: i18NService.get18nValue('_exportotoexcel.export', 'Export'),
                        className: "btn-primary",
                        callback: function (result) {
                            if (result) {
                                var exportModeSelected = $('input[name=exportMode]:checked', '#infos').val();
                                fn(schema, searchData, searchSort, searchOperator, paginationData, exportModeSelected);
                            }
                        }
                    }
                },
                className: "hapag-modal-exporttoexcel"
            });
        },


        exporttoexcel: function (schema, searchData, searchSort, searchOperator, paginationData,schemaSelected) {
            var application = schema.applicationName;
            if (contextService.isClient("hapag") && application == "asset" && !schemaSelected) {
                var fn = this.exporttoexcel;
                this.showModalExportToExcel(fn, schema, searchData, searchSort, searchOperator, paginationData);
                return null;
            }
            var schemaToUse = schemaSelected ? schemaSelected : schema.schemaId;

            var parameters = {};
            parameters.key = {};
            parameters.key.schemaId = schemaToUse;
            parameters.key.mode = schema.mode;
            parameters.key.platform = "web";
            parameters.application = application;
            var searchDTO = searchService.buildSearchDTO(searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause);
            var currentModule = contextService.currentModule();
            if (currentModule != null) {
                parameters.module = currentModule;
            }
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = paginationData.totalCount;
            searchDTO.pageSize = paginationData.pageSize;

            parameters.searchDTO = searchDTO;
            fileService.download(url("/Excel/Export" + "?" + $.param(parameters)));
        }
    }

});

