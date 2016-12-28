(function (angular, $) {
    "use strict";

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';

angular.module('sw_layout')
.service('excelService', [
    "$rootScope", "$http", "$timeout", "$log", "tabsService", "fixHeaderService",
    "i18NService",
    "redirectService", "searchService", "contextService", "fileService", "alertService",
    function ($rootScope, $http, $timeout, $log, tabsService, fixHeaderService,
        i18NService,
        redirectService, searchService, contextService, fileService, alertService) {

    return {
        showModalExportToExcel: function (parameters) {
            //TODO: fix this crap
            const modalTitle = i18NService.get18nValue('_exportotoexcel.export', 'Export') + " " + parameters.application +
                " " + i18NService.get18nValue('_exportotoexcel.toexcel', 'to excel');
            const selectText = i18NService.get18nValue('_exportotoexcel.selectexportmode', 'Select export mode');
            const exportModeGridOnlyText = i18NService.get18nValue('_exportotoexcel.gridonly', 'Grid only');
            const exportModeAllTheColumnsText = i18NService.get18nValue('_exportotoexcel.allthecolumns', 'Asset List');
            const exportModeCategoriesText = i18NService.get18nValue('_exportotoexcel.categories', 'Categories');
            
            bootbox.dialog({
                onEscape: true,
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
                        callback: () => null
                    },
                    main: {
                        label: i18NService.get18nValue('_exportotoexcel.export', 'Export'),
                        className: "btn-primary",
                        callback: (result) => {
                            if (!result) return;
                            const exportModeSelected = $('input[name=exportMode]:checked', '#infos').val();
                            const mode = exportModeSelected !== parameters.key.schemaId ? exportModeSelected : parameters.key.schemaId;
                            this.exportToExcel(mode);
                        }
                    }
                },
                className: "hapag-modal-exporttoexcel"
            });
        },


        exporttoexcel: function (schema, searchData, searchSort, searchOperator, paginationData, advancedsearchdata) {
            const schemaId = schema.schemaId;
            const parameters = {
                key: {
                    schemaId: schemaId,
                    mode: schema.mode,
                    platform: schema.platform
                },
                application: schema.applicationName
            };
            if (nullOrUndef(schemaId) && contextService.isClient("hapag") && parameters.application === "asset") {
                this.showModalExportToExcel(parameters);
                return null;
            }

            const quickSearchDTO = (advancedsearchdata && advancedsearchdata.quickSearchData) ? { quickSearchData: advancedsearchdata.quickSearchData } : null;
            const searchDTO = searchService.buildSearchDTO(searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause, null, null, quickSearchDTO);
            const currentModule = contextService.currentModule();
            if (currentModule != null) {
                parameters.module = currentModule;
            }
            searchDTO.pageNumber = 0;
            searchDTO.totalCount = paginationData.totalCount;
            searchDTO.pageSize = paginationData.pageSize;

            parameters.searchDTO = searchDTO;
            const downloadUrl = url(`/Excel/Export?${$.param(parameters)}`);
            fileService.downloadPromise(downloadUrl)
                .catch(error => {
                    const exception = {
                        exceptionMessage: "Error Downloading Excel File",
                        outlineInformation: error.url,
                        stackTrace: error.html
                    };
                    alertService.notifyexception(exception);
                });
        }
    };

}]);

})(angular, jQuery);