
(function (angular) {
    'use strict';

    class compositionListViewModel {



        constructor(crud_inputcommons,fieldService, schemaService, tabsService) {
            this.crud_inputcommons = crud_inputcommons;
            this.fieldService = fieldService;
            this.schemaService = schemaService;
            this.tabsService = tabsService;
        }



        filterForColumn(schema, column) {
            if (!schema || !schema.schemaFilters || !schema.schemaFilters.filters) {
                return null;
            }
            return schema.schemaFilters.filters.find((filter) => {
                return filter.attribute === column.attribute;
            });
        }

        buildExpandAllParams(relationship,parentSchema, parentdata, compositionlistschema, compositiondetailschema) {
            const params = {
                key: {},
                options: {},
                application: parentSchema.applicationName
            };
            const key = {};
            params.detailRequest.key = key;
            params.detailRequest.id = this.fieldService.getId(parentdata, parentSchema);
            key.schemaId = parentSchema.schemaId;
            key.mode = parentSchema.mode;
            key.platform = platform();
            const compositionsToExpand = {};
            compositionsToExpand[relationship] = { schema: compositionlistschema, value: true };
            //                var compositionsToExpand = { 'worklog_': true };

            params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(compositionsToExpand, parentSchema,
                parentdata, compositiondetailschema.schemaId);
            params.options.printMode = true;
            return params;
        }

      


    }

    compositionListViewModel.$inject = ['crud_inputcommons','fieldService', 'schemaService', 'tabsService'];

    angular.module('sw_layout').service('compositionListViewModel', compositionListViewModel);



})(angular);
