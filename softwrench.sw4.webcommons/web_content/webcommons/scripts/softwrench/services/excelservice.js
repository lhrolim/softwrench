(function (angular) {
    "use strict";

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';

angular.module('sw_layout')
.factory('excelService', [
    "$rootScope", "$http", "$timeout", "$log", "tabsService", "fixHeaderService",
    "i18NService",
    "redirectService", "searchService", "contextService", "fileService",
    function ($rootScope, $http, $timeout, $log, tabsService, fixHeaderService,
        i18NService,
        redirectService, searchService, contextService, fileService) {

    return {
        showModalExportToExcel: function (parameters) {
            //TODO: fix this crap
            var modalTitle = i18NService.get18nValue('_exportotoexcel.export', 'Export') + " " + parameters.application +
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
                    "</label><br>" +
                    "<label class='control-label' for='categoriesid'>" +
                    "<input type='radio' name='exportMode' id='categoriesid' value='categories' /> "
                    + exportModeCategoriesText +
                    "</label>" +
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
                                if (exportModeSelected != parameters.key.schemaId) {
                                    this.exportToExcel(exportModeSelected);
                                } else {
                                    this.exportToExcel(parameters.key.schemaId);
                                }
                            }
                        }
                    }
                },
                className: "hapag-modal-exporttoexcel"
            });
        },


        exporttoexcel: function (schema, searchData, searchSort, searchOperator, paginationData, advancedsearchdata) {
            var schemaId = schema.schemaId;
            var parameters = {};
            parameters.key = {};
            parameters.key.schemaId = schema.schemaId;
            parameters.key.mode = schema.mode;
            parameters.key.platform = "web";
            parameters.application = schema.applicationName;
            if ((nullOrUndef(schemaId))) {
                if (contextService.isClient("hapag") && parameters.application == "asset") {
                    this.showModalExportToExcel(parameters);
                    return null;
                }
            } else {
                parameters.key.schemaId = schemaId;
            }

            var quickSearchDTO = (advancedsearchdata && advancedsearchdata.quickSearchData) ? { quickSearchData: advancedsearchdata.quickSearchData } : null;
            var searchDTO = searchService.buildSearchDTO(searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause, null, null, quickSearchDTO);
            var currentModule = contextService.currentModule();
            if (currentModule != null) {
                parameters.module = currentModule;
            }
            searchDTO.pageNumber = 0;
            searchDTO.totalCount = paginationData.totalCount;
            searchDTO.pageSize = paginationData.pageSize;

            parameters.searchDTO = searchDTO;
            fileService.download(url("/Excel/Export" + "?" + $.param(parameters)));
        }
    };

}]);

})(angular);