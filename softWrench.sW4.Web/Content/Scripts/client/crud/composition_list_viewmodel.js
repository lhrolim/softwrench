
(function (angular) {
    'use strict';

    class compositionListViewModel {



        constructor(crud_inputcommons, schemaService, eventService) {
            this.crud_inputcommons = crud_inputcommons;
            this.schemaService = schemaService;
        }



        filterForColumn(schema, column) {
            if (!schema || !schema.schemaFilters || !schema.schemaFilters.filters) {
                return null;
            }
            return schema.schemaFilters.filters.find((filter) => {
                return filter.attribute === column.attribute;
            });
        }

      


    }

    compositionListViewModel.$inject = ['crud_inputcommons', 'schemaService', 'eventService'];

    angular.module('sw_layout').service('compositionListViewModel', compositionListViewModel);



})(angular);
